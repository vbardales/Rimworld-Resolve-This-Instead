using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace ResolveThisInstead
{
    // The player's decisions: which proposed replacements are switched on or off, and which successor to use when several are loaded.
    public class RtiSettings : ModSettings
    {
        public Dictionary<string, bool> enabled = new Dictionary<string, bool>();
        public Dictionary<string, string> picks = new Dictionary<string, string>();
        // Debug lines in the log (every alias answer, load phases, caller walk). On while the library is being stabilised.
        public bool debugLogs = true;

        public PlayerChoices ToChoices()
        {
            var choices = new PlayerChoices();
            foreach (KeyValuePair<string, bool> pair in enabled) choices.Enabled[pair.Key] = pair.Value;
            foreach (KeyValuePair<string, string> pair in picks) choices.Picks[pair.Key] = pair.Value;
            return choices;
        }

        public void ResetToDefaults()
        {
            enabled.Clear();
            picks.Clear();
            debugLogs = true;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref enabled, "enabled", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref picks, "picks", LookMode.Value, LookMode.Value);
            Scribe_Values.Look(ref debugLogs, "debugLogs", true);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                // An older or hand-edited file must not break the game.
                enabled = enabled ?? new Dictionary<string, bool>();
                picks = picks ?? new Dictionary<string, string>();
            }
        }
    }

    public class ResolveThisInsteadMod : Mod
    {
        public const string PackageId = "nelim.resolvethisinstead";
        private const float RowHeight = 28f;

        public static ResolveThisInsteadMod Instance;
        public static RtiSettings Settings;

        private Vector2 scroll;

        // What the window shows: the player's pending choices (read by the NEXT launch, never by the load that already happened) and
        // the lists derived from them, computed once and again only when a choice changes.
        private PlayerChoices pending;
        private List<AliasRule> inPlay;
        private List<string> ambiguous;

        public ResolveThisInsteadMod(ModContentPack content) : base(content)
        {
            Instance = this;
            Settings = GetSettings<RtiSettings>();
            Runtime.DebugEnabled = Settings.debugLogs;
            Runtime.Info("Mod constructor start (RimWorld " + VersionControl.CurrentVersionString + ", " + ModsConfig.ActiveModsInLoadOrder.Count() + " active mods).");
            try
            {
                Runtime.Initialize(Settings);
            }
            catch (System.Exception ex)
            {
                Runtime.Failure("Initialize", ex);
            }
            try
            {
                var harmony = new Harmony(PackageId);
                harmony.PatchAll(typeof(ResolveThisInsteadMod).Assembly);
                foreach (var method in harmony.GetPatchedMethods())
                {
                    Runtime.Debug("patched: " + method.DeclaringType?.Name + "." + method.Name);
                }
            }
            catch (System.Exception ex)
            {
                Runtime.Failure("PatchAll", ex);
            }
            Runtime.Info("Mod constructor done.");
        }

        public override string SettingsCategory() => "RTI_SettingsCategory".Translate();

        private void ChoicesChanged()
        {
            pending = null;
            inPlay = null;
            ambiguous = null;
        }

        private void EnsureLists()
        {
            if (inPlay != null)
            {
                return;
            }
            pending = Settings.ToChoices();
            // Replacements that matter in this game: the old mod is absent and its successor is loaded.
            inPlay = Runtime.Catalog.All
                .Where(r => Runtime.IsLoaded(r.NewPackageId) && (r.OldPackageId.Length == 0 || !Runtime.IsLoaded(r.OldPackageId)))
                .OrderBy(r => r.OldName ?? r.OldPackageId, System.StringComparer.OrdinalIgnoreCase)
                .ToList();
            // Old ids for which several successors are loaded at the same distance and the player has not chosen.
            ambiguous = new List<string>();
            foreach (AliasRule rule in inPlay.Where(r => r.OldPackageId.Length > 0))
            {
                if (!ambiguous.Contains(rule.OldPackageId)
                    && Runtime.Resolver.ResolvePackageId(rule.OldPackageId, Runtime.IsLoaded, pending).Status == ResolutionStatus.Ambiguous)
                {
                    ambiguous.Add(rule.OldPackageId);
                }
            }
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            EnsureLists();
            var listing = new Listing_Standard();
            listing.Begin(inRect);
            listing.Label(Runtime.UtiPath == null
                ? "RTI_DataMissing".Translate()
                : "RTI_DataFound".Translate(Runtime.UtiRules, Runtime.UtiVersion ?? "?", Runtime.Embedded.Sum(e => e.Rules)));
            if (Runtime.Problems.Count > 0)
            {
                listing.Label("RTI_Problems".Translate(Runtime.Problems.Count), tooltip: string.Join("\n", Runtime.Problems.Take(10)));
            }
            listing.Label("RTI_RestartNote".Translate());
            listing.CheckboxLabeled("RTI_DebugLogs".Translate(), ref Settings.debugLogs, "RTI_DebugLogsTip".Translate());
            Runtime.DebugEnabled = Settings.debugLogs;
            if (listing.ButtonText("RTI_ResetDefaults".Translate()))
            {
                Settings.ResetToDefaults();
                ChoicesChanged();
                EnsureLists();
            }
            listing.GapLine();
            listing.Label("RTI_ListHeader".Translate());
            float used = listing.CurHeight;
            listing.End();

            var outRect = new Rect(inRect.x, inRect.y + used + 6f, inRect.width, inRect.height - used - 6f);
            var viewRect = new Rect(0f, 0f, outRect.width - 16f, System.Math.Max(1, inPlay.Count + ambiguous.Count) * RowHeight);
            Widgets.BeginScrollView(outRect, ref scroll, viewRect);
            float y = 0f;
            if (inPlay.Count == 0 && ambiguous.Count == 0)
            {
                Widgets.Label(new Rect(0f, y, viewRect.width, RowHeight), "RTI_ListEmpty".Translate());
            }
            bool changed = false;
            foreach (AliasRule rule in inPlay)
            {
                bool on = AliasCatalog.IsEnabled(rule, pending);
                bool before = on;
                var row = new Rect(0f, y, viewRect.width, RowHeight);
                Widgets.CheckboxLabeled(row, "RTI_RowLabel".Translate(rule.OldName ?? rule.OldPackageId, rule.NewName ?? rule.NewPackageId), ref on);
                TooltipHandler.TipRegion(row, RowTip(rule));
                if (on != before)
                {
                    Settings.enabled[rule.Id] = on;
                    changed = true;
                }
                y += RowHeight;
            }
            foreach (string start in ambiguous)
            {
                var row = new Rect(0f, y, viewRect.width, RowHeight);
                Widgets.Label(new Rect(row.x, row.y, row.width - 180f, row.height), "RTI_Ambiguous".Translate(start));
                if (Widgets.ButtonText(new Rect(row.xMax - 170f, row.y, 170f, row.height), "RTI_Choose".Translate()))
                {
                    ChooseSuccessor(start);
                }
                y += RowHeight;
            }
            Widgets.EndScrollView();
            if (changed)
            {
                ChoicesChanged();
            }
        }

        private void ChooseSuccessor(string start)
        {
            Resolution res = Runtime.Resolver.ResolvePackageId(start, Runtime.IsLoaded, pending);
            var options = new List<FloatMenuOption>();
            foreach (string candidate in res.Candidates)
            {
                string chosen = candidate;
                options.Add(new FloatMenuOption(Runtime.ActiveMod(chosen)?.Name ?? chosen, () =>
                {
                    Settings.picks[start] = chosen;
                    ChoicesChanged();
                }));
            }
            Find.WindowStack.Add(new FloatMenu(options));
        }

        private static string RowTip(AliasRule rule)
        {
            string source = rule.Source == RuleSource.Embedded ? "RTI_SourceDeclared".Translate() : "RTI_SourceProposed".Translate();
            string compat = rule.Compat == Compat.Yes ? "RTI_CompatYes".Translate()
                : rule.Compat == Compat.No ? "RTI_CompatNo".Translate() : "RTI_CompatUnknown".Translate();
            return "RTI_RowTip".Translate(rule.OldName ?? "?", rule.OldPackageId.Length > 0 ? rule.OldPackageId : "-",
                rule.NewName ?? "?", rule.NewPackageId, source, compat + (string.IsNullOrEmpty(rule.Note) ? "" : "\n" + rule.Note));
        }
    }

    // The same settings, opened from a MainButtons shortcut that is hidden by default.
    public class MainButtonWorker_ResolveThisInsteadSettings : MainButtonWorker
    {
        public override void Activate()
        {
            if (ResolveThisInsteadMod.Instance != null)
            {
                Find.WindowStack.Add(new Dialog_ModSettings(ResolveThisInsteadMod.Instance));
            }
        }
    }

    // The doors of the game that ask "is this mod loaded?". See Runtime.MayAnswer for the three windows: never during Mod construction,
    // always while loading (the engine processing mod-authored XML), and afterward only when a mod's own code is asking.
    [HarmonyPatch(typeof(ModLister), nameof(ModLister.GetActiveModWithIdentifier))]
    public static class ModLister_GetActiveModWithIdentifier_Patch
    {
        // MayRequire, MayRequireAnyOf, LoadFolders-style lookups and AnyModActiveNoSuffix all go through here.
        public static void Postfix(string identifier, ref ModMetaData __result)
        {
            try
            {
                if (__result != null || !Runtime.MayAnswer)
                {
                    return;
                }
                string target = Runtime.TargetForId(identifier);
                if (target != null)
                {
                    Runtime.DebugOnce("id:" + identifier, "GetActiveModWithIdentifier(" + identifier + ") answered by " + target);
                    __result = Runtime.ActiveMod(target);
                }
            }
            catch (System.Exception ex)
            {
                Runtime.Failure("GetActiveModWithIdentifier(" + identifier + ")", ex);
            }
        }
    }

    [HarmonyPatch(typeof(ModLister), nameof(ModLister.HasActiveModWithName))]
    public static class ModLister_HasActiveModWithName_Patch
    {
        // PatchOperationFindMod compares display names.
        public static void Postfix(string name, ref bool __result)
        {
            try
            {
                if (!__result && Runtime.MayAnswer && Runtime.TargetForName(name) != null)
                {
                    Runtime.DebugOnce("name:" + name, "HasActiveModWithName(" + name + ") answered true");
                    __result = true;
                }
            }
            catch (System.Exception ex)
            {
                Runtime.Failure("HasActiveModWithName(" + name + ")", ex);
            }
        }
    }

    [HarmonyPatch(typeof(ModsConfig), nameof(ModsConfig.IsActive), new[] { typeof(string) })]
    public static class ModsConfig_IsActive_Patch
    {
        // Direct callers of IsActive (a mod's own runtime code, not just XML). By construction TargetForId only returns a package id that
        // Runtime.IsLoaded already found loaded, so no recursive call into the (patched) method is needed here.
        public static void Postfix(string id, ref bool __result)
        {
            try
            {
                if (!__result && Runtime.MayAnswer)
                {
                    __result = Runtime.TargetForId(id) != null;
                    if (__result)
                    {
                        Runtime.DebugOnce("isactive:" + id, "ModsConfig.IsActive(" + id + ") answered true");
                    }
                }
            }
            catch (System.Exception ex)
            {
                Runtime.Failure("ModsConfig.IsActive(" + id + ")", ex);
            }
        }
    }

    [HarmonyPatch(typeof(LoadedModManager), nameof(LoadedModManager.LoadModXML))]
    public static class LoadedModManager_LoadModXML_Patch
    {
        // Every Mod constructor has run: from here the game reads XML, patches and MayRequire, and the aliases start to answer.
        public static void Prefix()
        {
            try
            {
                Runtime.BeginLoading();
            }
            catch (System.Exception ex)
            {
                Runtime.Failure("BeginLoading", ex);
            }
        }
    }

    [HarmonyPatch(typeof(StaticConstructorOnStartupUtility), nameof(StaticConstructorOnStartupUtility.CallAll))]
    public static class StaticConstructorOnStartupUtility_CallAll_Patch
    {
        // The data is loaded: from here on the real mod list is what everyone sees.
        public static void Prefix()
        {
            try
            {
                Runtime.EndLoading();
            }
            catch (System.Exception ex)
            {
                Runtime.Failure("EndLoading", ex);
            }
        }
    }
}
