using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace ResolveThisInsteadPoc
{
    // Proof of concept, test only: hardcoded alias for one real case (Adaptive Storage Neolithic Module -> our own Adaptive Storage Neolithic Renew).
    // Question answered: from a Verse.Mod constructor, can we make (a) name guards, (b) MayRequire / id lookups and
    // (c) ModsConfig.IsActive see an old mod as active when its successor is loaded?
    internal static class Aliases
    {
        internal const string Tag = "[RTI-POC] ";

        // old display name -> successor packageId (lower case)
        internal static readonly Dictionary<string, string> Names = new Dictionary<string, string>
        {
            { "Adaptive Storage Neolithic Module", "nelim.adaptivestorageneolithic" },
        };

        // old packageId (lower case) -> successor packageId (lower case)
        internal static readonly Dictionary<string, string> Ids = new Dictionary<string, string>
        {
            { "adaptive.storage.neolithic", "nelim.adaptivestorageneolithic" },
        };

        internal static ModMetaData FindActive(string packageId)
        {
            foreach (ModMetaData mod in ModsConfig.ActiveModsInLoadOrder)
            {
                string id = mod.PackageId.ToLowerInvariant();
                if (id == packageId || id == packageId + "_steam" || id == packageId + "_copy")
                {
                    return mod;
                }
            }
            return null;
        }
    }

    public class RtiPocMod : Mod
    {
        public RtiPocMod(ModContentPack content) : base(content)
        {
            Log.Message(Aliases.Tag + "Mod constructor running (before ApplyPatches / ParseAndProcessXML).");
            var harmony = new Harmony("nelim.resolvethisinstead.poc");
            harmony.Patch(AccessTools.Method(typeof(ModLister), nameof(ModLister.GetActiveModWithIdentifier)),
                postfix: new HarmonyMethod(typeof(RtiPocMod), nameof(GetActiveModWithIdentifierPostfix)));
            harmony.Patch(AccessTools.Method(typeof(ModLister), nameof(ModLister.HasActiveModWithName)),
                postfix: new HarmonyMethod(typeof(RtiPocMod), nameof(HasActiveModWithNamePostfix)));
            harmony.Patch(AccessTools.Method(typeof(ModMetaData), nameof(ModMetaData.SamePackageId)),
                postfix: new HarmonyMethod(typeof(RtiPocMod), nameof(SamePackageIdPostfix)));
            harmony.Patch(AccessTools.Method(typeof(ModsConfig), "RecacheActiveMods"),
                postfix: new HarmonyMethod(typeof(RtiPocMod), nameof(RecachePostfix)));
            InjectIntoActiveSet("constructor");
        }

        public static void GetActiveModWithIdentifierPostfix(string identifier, ref ModMetaData __result)
        {
            if (__result != null) return;
            if (Aliases.Ids.TryGetValue(identifier.ToLowerInvariant().Trim(), out string successor))
            {
                __result = Aliases.FindActive(successor);
            }
        }

        public static void HasActiveModWithNamePostfix(string name, ref bool __result)
        {
            if (__result) return;
            if (Aliases.Names.TryGetValue(name, out string successor))
            {
                __result = Aliases.FindActive(successor) != null;
            }
        }

        public static void SamePackageIdPostfix(ModMetaData __instance, string otherPackageId, ref bool __result)
        {
            if (__result || otherPackageId == null) return;
            if (Aliases.Ids.TryGetValue(otherPackageId.ToLowerInvariant().Trim(), out string successor))
            {
                __result = __instance.PackageId != null && __instance.PackageId.ToLowerInvariant() == successor;
            }
        }

        public static void RecachePostfix() => InjectIntoActiveSet("RecacheActiveMods postfix");

        internal static void InjectIntoActiveSet(string when)
        {
            HashSet<string> set = AccessTools.StaticFieldRefAccess<HashSet<string>>(typeof(ModsConfig), "activeModsHashSet");
            foreach (KeyValuePair<string, string> alias in Aliases.Ids)
            {
                if (Aliases.FindActive(alias.Value) != null)
                {
                    set.Add(alias.Key);
                    Log.Message(Aliases.Tag + "added '" + alias.Key + "' to activeModsHashSet (" + when + ").");
                }
            }
        }
    }

    [StaticConstructorOnStartup]
    public static class RtiPocReport
    {
        static RtiPocReport()
        {
            int fails = 0;
            void Check(string label, bool actual, bool expected)
            {
                bool ok = actual == expected;
                if (!ok) fails++;
                Log.Message(Aliases.Tag + (ok ? "PASS " : "FAIL ") + label + " = " + actual + " (expected " + expected + ")");
            }

            ModMetaData successor = Aliases.FindActive("nelim.adaptivestorageneolithic");
            Log.Message(Aliases.Tag + "successor nelim.adaptivestorageneolithic active: " + (successor != null)
                + (successor != null ? " (name '" + successor.Name + "')" : " -> the checks below are NOT valid: enable Adaptive Storage Neolithic Renew"));
            Log.Message(Aliases.Tag + "an active mod really named \"Adaptive Storage Neolithic Module\": "
                + ModsConfig.ActiveModsInLoadOrder.Any(m => m.Name == "Adaptive Storage Neolithic Module"));
            Log.Message(Aliases.Tag + "an active mod with packageId adaptive.storage.neolithic (the original): "
                + ModsConfig.ActiveModsInLoadOrder.Any(m => m.PackageId.ToLowerInvariant().StartsWith("adaptive.storage.neolithic")));

            bool originalActive = ModsConfig.ActiveModsInLoadOrder.Any(m => m.PackageId.ToLowerInvariant().StartsWith("adaptive.storage.neolithic"));
            bool valid = successor != null && !originalActive;
            if (originalActive) Log.Message(Aliases.Tag + "the ORIGINAL mod is active: the checks below prove nothing, disable it and restart.");
            Check("HasActiveModWithName(\"Adaptive Storage Neolithic Module\")", ModLister.HasActiveModWithName("Adaptive Storage Neolithic Module"), valid);
            Check("HasActiveModWithName(control name)", ModLister.HasActiveModWithName("Resolve This Instead Nonexistent Mod"), false);
            Check("ModsConfig.IsActive(\"adaptive.storage.neolithic\")", ModsConfig.IsActive("adaptive.storage.neolithic"), valid);
            Check("ModsConfig.IsActive(control id)", ModsConfig.IsActive("nelim.rti.nonexistent"), false);
            Check("ModLister.AnyModActiveNoSuffix([adaptive.storage.neolithic])", ModLister.AnyModActiveNoSuffix(new List<string> { "adaptive.storage.neolithic" }), valid);
            Check("ModLister.AllModsActiveNoSuffix([adaptive.storage.neolithic])", ModLister.AllModsActiveNoSuffix(new List<string> { "adaptive.storage.neolithic" }), valid);
            Check("XML patch guard (PatchOperationFindMod by name) applied: def RTI_PocPatchGuard exists",
                DefDatabase<StatCategoryDef>.GetNamedSilentFail("RTI_PocPatchGuard") != null, valid);
            Check("XML patch control (nonexistent name) applied: def RTI_PocPatchControl exists",
                DefDatabase<StatCategoryDef>.GetNamedSilentFail("RTI_PocPatchControl") != null, false);
            Check("MayRequire=\"adaptive.storage.neolithic\" on a def: RTI_PocMayRequire exists",
                DefDatabase<StatCategoryDef>.GetNamedSilentFail("RTI_PocMayRequire") != null, valid);
            Check("MayRequire=control id on a def: RTI_PocMayRequireControl exists",
                DefDatabase<StatCategoryDef>.GetNamedSilentFail("RTI_PocMayRequireControl") != null, false);
            bool ccl = Aliases.FindActive("ccl.patchoperation.highlander") != null;
            bool xmlExt = Aliases.FindActive("imranfish.xmlextensions") != null;
            Log.Message(Aliases.Tag + "providers active: CCL Highlander=" + ccl + ", XML Extensions=" + xmlExt + " (their checks below are valid only if true).");
            if (xmlExt)
            {
                Check("XmlExtensions.FindMod (packageId, OLD id only) applied: RTI_PocXmlExtOldId exists (expected False: its method does not know replacements)",
                    DefDatabase<StatCategoryDef>.GetNamedSilentFail("RTI_PocXmlExtOldId") != null, false);
                Check("XmlExtensions.FindMod (packageId, OLD and NEW ids listed) applied: RTI_PocXmlExtBoth exists",
                    DefDatabase<StatCategoryDef>.GetNamedSilentFail("RTI_PocXmlExtBoth") != null, valid);
            }
            if (ccl)
            {
                Check("CCL FindModByID (OLD id) applied through the SamePackageId postfix: RTI_PocCclOldId exists",
                    DefDatabase<StatCategoryDef>.GetNamedSilentFail("RTI_PocCclOldId") != null, valid);
            }
            Log.Message(Aliases.Tag + (fails == 0 ? "RESULT: all checks passed." : "RESULT: " + fails + " check(s) failed.")
                + " Not covered by design: LoadFolders.xml IfModActive (evaluated before any mod DLL loads).");
        }
    }
}
