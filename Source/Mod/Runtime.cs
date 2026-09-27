using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace ResolveThisInstead
{
    // What the library knows once the game has started: the catalog (rules read from Use This Instead and from the files that mods embed),
    // the resolver, the player's choices and what was found where. Built once, in the Mod constructor, which runs before any patch guard or
    // MayRequire is read and after every DLL is loaded.
    public static class Runtime
    {
        public const string LogTag = "[Resolve This Instead] ";

        public sealed class EmbeddedInfo
        {
            public string ModName;
            public string PackageId;
            public int Rules;
            public List<string> Errors = new List<string>();
        }

        public static AliasCatalog Catalog = new AliasCatalog();
        public static AliasResolver Resolver = new AliasResolver(new AliasCatalog());
        // The player's choices as they were when the game started. Fixed for the whole load: the settings window edits a pending copy that the next launch reads.
        public static PlayerChoices Choices = new PlayerChoices();

        // Three windows, not two:
        // 1. Mod construction (CreateModClasses, before any of this runs): NEVER answer, whoever asks. A mod's own constructor must see
        //    reality, or it could go on to use another mod's types or settings that are not really there.
        // 2. Loading (LoadedModManager.LoadModXML to StaticConstructorOnStartupUtility.CallAll): ALWAYS answer. Every caller here is the
        //    engine itself, processing mod-authored XML (patches, MayRequire, cross-references) on everyone's behalf.
        // 3. Afterward, forever (menus reopened later, a mod's own runtime code, hot reload): answer ONLY when the immediate caller is a
        //    mod's own assembly, never the engine's. The engine's own bookkeeping (the mod list screen, ModMetaData.Active) runs in this
        //    same window and must see reality; a mod's own C# asking the same question is exactly what this library exists to help.
        public static volatile bool Loading;
        // Set once, at the first BeginLoading, and never cleared: mod constructors do not run a second time, hot reload included (checked
        // in the decompiled LoadedModManager.LoadAllActiveMods: InitializeMods/CreateModClasses are skipped when hotReload is true).
        public static volatile bool PastConstructionPhase;
        public static readonly List<EmbeddedInfo> Embedded = new List<EmbeddedInfo>();
        public static readonly List<string> Problems = new List<string>();

        // Use This Instead's data, when it was found.
        public static string UtiPath;
        public static string UtiVersion;
        public static int UtiRules;

        private static HashSet<string> loaded = new HashSet<string>();
        private static readonly Dictionary<string, string> idCache = new Dictionary<string, string>();
        private static readonly Dictionary<string, string> nameCache = new Dictionary<string, string>();

        public static bool IsLoaded(string normalizedPackageId) => loaded.Contains(normalizedPackageId);

        public static void Initialize(RtiSettings settings)
        {
            Choices = settings.ToChoices();
            loaded = new HashSet<string>(ModsConfig.ActiveModsInLoadOrder.Select(m => PackageIds.Normalize(m.PackageId)));

            var catalog = new AliasCatalog();
            ReadEmbeddedFiles(catalog);
            ReadUtiData(catalog);
            Catalog = catalog;
            Resolver = new AliasResolver(catalog);
            ClearCaches();
            Log.Message(LogTag + "ready: " + catalog.Count + " replacements known, " + Embedded.Count + " mod(s) embed a file, Use This Instead's data "
                + (UtiPath == null ? "not found" : "version " + UtiVersion) + ".");
        }

        // Called when the game starts turning XML into data (LoadModXML, after every Mod constructor has run).
        public static void BeginLoading()
        {
            Loading = true;
            PastConstructionPhase = true;
        }

        // Called once, when the game has finished loading its data (before any StaticConstructorOnStartup runs). From here on an alias
        // answers only to a mod's own code (MayAnswer), never unconditionally.
        public static void EndLoading()
        {
            Loading = false;
            ClearCaches();
            Log.Message(LogTag + "loading finished: aliases now answer only to a mod's own code, never the engine's.");
        }

        // Whether an alias may answer right now. See the three windows above.
        public static bool MayAnswer => Loading || (PastConstructionPhase && CallerIsTrustedForRuntime());

        private static readonly Assembly OwnAssembly = typeof(Runtime).Assembly;
        private static Assembly harmonyAssembly;

        // Assemblies of mods whose embedded file asked for <runtime>true</runtime> on at least one rule. NOT "any mod": a diagnostic or
        // compatibility-checking mod (or our own Pickle steps) wants the TRUTH when it asks IsActive, exactly like the engine's own
        // bookkeeping does, and "caller is some mod" cannot tell those two apart from "a consumer that wants the convenience" - only an
        // explicit declaration can. Built once in Initialize from ReadEmbeddedFiles, immutable afterward.
        private static readonly HashSet<Assembly> trustedForRuntime = new HashSet<Assembly>();

        // The immediate caller of the door that is answering (GetActiveModWithIdentifier, HasActiveModWithName, IsActive), found by walking
        // the stack up from here and skipping frames that are not a real caller: our own postfix, Harmony's dynamic trampoline (which has no
        // real declaring type, so its assembly reads null), and Harmony's own library. Not cached: correctness first, see STATUS.md for the
        // one thing this needs a real run to confirm (whether the walk survives across a Mono JIT that might inline a frame away).
        public static Assembly FindRealCaller()
        {
            harmonyAssembly = harmonyAssembly ?? typeof(Harmony).Assembly;
            var trace = new StackTrace(false);
            for (int i = 0; i < trace.FrameCount; i++)
            {
                Assembly asm = trace.GetFrame(i)?.GetMethod()?.DeclaringType?.Assembly;
                if (asm == null || asm == OwnAssembly || asm == harmonyAssembly)
                {
                    continue;
                }
                return asm;
            }
            return null;
        }

        public static bool CallerIsTrustedForRuntime()
        {
            Assembly asm = FindRealCaller();
            return asm != null && trustedForRuntime.Contains(asm);
        }

        public static void ClearCaches()
        {
            idCache.Clear();
            nameCache.Clear();
        }

        private static void ReadEmbeddedFiles(AliasCatalog catalog)
        {
            trustedForRuntime.Clear();
            foreach (ModContentPack mod in LoadedModManager.RunningMods)
            {
                string path = Path.Combine(mod.RootDir, EmbeddedFile.RelativePath);
                if (!File.Exists(path))
                {
                    continue;
                }
                var info = new EmbeddedInfo { ModName = mod.Name, PackageId = mod.PackageId };
                try
                {
                    EmbeddedFile.ParseResult parsed = EmbeddedFile.Parse(File.ReadAllText(path));
                    catalog.AddRange(parsed.Rules);
                    info.Rules = parsed.Rules.Count;
                    info.Errors.AddRange(parsed.Errors);
                    if (parsed.Rules.Any(r => r.RuntimeOptIn))
                    {
                        foreach (Assembly asm in mod.assemblies.loadedAssemblies)
                        {
                            trustedForRuntime.Add(asm);
                        }
                        Log.Message(LogTag + mod.Name + " asked that its own code also see an alias after loading (<runtime>true</runtime>).");
                    }
                }
                catch (Exception ex)
                {
                    info.Errors.Add("could not be read: " + ex.Message);
                }
                foreach (string error in info.Errors)
                {
                    Problems.Add(mod.Name + ": " + error);
                    Log.Warning(LogTag + mod.Name + " (" + EmbeddedFile.RelativePath + "): " + error);
                }
                Embedded.Add(info);
            }
        }

        private static void ReadUtiData(AliasCatalog catalog)
        {
            UtiPath = UtiData.Locate(LoadedModManager.RunningMods.Select(m => new KeyValuePair<string, string>(m.PackageId, m.RootDir)), File.Exists);
            if (UtiPath == null)
            {
                Log.Warning(LogTag + "Use This Instead's replacements.json was not found: only the pairs that mods declare are used.");
                Problems.Add("Use This Instead's data not found");
                return;
            }
            try
            {
                UtiData.ParseResult data = UtiData.Parse(File.ReadAllText(UtiPath));
                catalog.AddRange(data.Rules);
                UtiVersion = data.Version;
                UtiRules = data.Rules.Count;
                foreach (string error in data.Errors)
                {
                    Problems.Add("Use This Instead's data: " + error);
                    Log.Warning(LogTag + "Use This Instead's data: " + error);
                }
            }
            catch (Exception ex)
            {
                UtiPath = null;
                Problems.Add("Use This Instead's data could not be read: " + ex.Message);
                Log.Warning(LogTag + "Use This Instead's data could not be read: " + ex.Message);
            }
        }

        // The loaded package id that stands in for this one, or null.
        public static string TargetForId(string packageId)
        {
            string key = PackageIds.Normalize(packageId);
            if (key.Length == 0)
            {
                return null;
            }
            if (!idCache.TryGetValue(key, out string target))
            {
                Resolution res = Resolver.ResolvePackageId(key, IsLoaded, Choices);
                target = res.IsAlias ? res.Target : null;
                idCache[key] = target;
            }
            return target;
        }

        // The same, for a display name (what PatchOperationFindMod compares).
        public static string TargetForName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }
            if (!nameCache.TryGetValue(name, out string target))
            {
                Resolution res = Resolver.ResolveName(name, IsLoaded, Choices);
                target = res.IsAlias ? res.Target : null;
                nameCache[name] = target;
            }
            return target;
        }

        public static ModMetaData ActiveMod(string normalizedPackageId)
        {
            foreach (ModMetaData mod in ModsConfig.ActiveModsInLoadOrder)
            {
                if (PackageIds.Normalize(mod.PackageId) == normalizedPackageId)
                {
                    return mod;
                }
            }
            return null;
        }
    }
}
