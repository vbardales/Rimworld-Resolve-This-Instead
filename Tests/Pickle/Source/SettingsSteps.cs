using System;
using System.IO;
using System.Linq;
using System.Reflection;
using RimWorks.Pickle;
using RimWorld;
using Verse;

namespace ResolveThisInstead.PickleSteps
{
    /// <summary>
    /// What only a started game shows about the settings window: that it belongs to this mod and edits the instance the hooks read from,
    /// that a change reaches an alias immediately in the window but only the NEXT launch reads it back (Runtime.Choices is fixed at load),
    /// and that a value outlives the process. The engine's own rules (merge, resolution) are proved by Tests/UnitTests.
    /// </summary>
    [PickleSteps]
    public class SettingsSteps
    {
        private const string ShortcutDefName = "RTI_Settings";

        private static ResolveThisInsteadMod Mod(PickleContext ctx)
        {
            var mod = LoadedModManager.GetMod<ResolveThisInsteadMod>();
            ctx.Require(mod != null, "ResolveThisInsteadMod is not loaded in this process");
            return mod;
        }

        private static RtiSettings Settings(PickleContext ctx)
        {
            var settings = ResolveThisInsteadMod.Settings;
            ctx.Require(settings != null, "ResolveThisInsteadMod.Settings is null");
            return settings;
        }

        private static MainButtonDef Shortcut(PickleContext ctx)
        {
            var def = DefDatabase<MainButtonDef>.GetNamedSilentFail(ShortcutDefName);
            ctx.Require(def != null, $"no MainButtonDef named '{ShortcutDefName}'");
            return def;
        }

        private static Dialog_ModSettings OpenDialog(PickleContext ctx)
        {
            var dialog = Find.WindowStack.Windows.OfType<Dialog_ModSettings>().FirstOrDefault();
            ctx.Require(dialog != null, "no Dialog_ModSettings is open");
            return dialog;
        }

        private static Verse.Mod DialogMod(PickleContext ctx)
        {
            var dialog = OpenDialog(ctx);
            var field = typeof(Dialog_ModSettings)
                .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .FirstOrDefault(f => typeof(Verse.Mod).IsAssignableFrom(f.FieldType));
            ctx.Require(field != null, "Dialog_ModSettings has no Mod-typed field in this game build");
            return field.GetValue(dialog) as Verse.Mod;
        }

        private static AliasRule FindRule(PickleContext ctx, string oldId, string newId)
        {
            AliasRule rule = Runtime.Catalog.All.FirstOrDefault(r =>
                r.OldPackageId == PackageIds.Normalize(oldId) && r.NewPackageId == PackageIds.Normalize(newId));
            ctx.Require(rule != null, $"no known alias from '{oldId}' to '{newId}'");
            return rule;
        }

        // --- the window and its two routes ----------------------------------------------------

        [When("Resolve This Instead opens its settings window")]
        public void OpenSettings(PickleContext ctx) => Find.WindowStack.Add(new Dialog_ModSettings(Mod(ctx)));

        [Then("Resolve This Instead sees its own settings window open")]
        public void SeesOwnDialog(PickleContext ctx)
        {
            var mod = DialogMod(ctx);
            ctx.Assert(mod is ResolveThisInsteadMod, $"the open settings window belongs to '{mod?.GetType().Name ?? "nothing"}'");
        }

        [Then("Resolve This Instead the open window edits the same settings instance")]
        public void SameInstance(PickleContext ctx)
        {
            var mod = DialogMod(ctx) as ResolveThisInsteadMod;
            ctx.Assert(mod != null, "the open settings window does not belong to Resolve This Instead");
            ctx.Assert(ReferenceEquals(ResolveThisInsteadMod.Settings, Settings(ctx)),
                "the window edits a different settings object than the one the hooks read");
        }

        // --- what the mod loaded with -----------------------------------------------------------

        [Then("Resolve This Instead found Use This Instead's data")]
        public void FoundUtiData(PickleContext ctx) => ctx.Assert(Runtime.UtiPath != null, "Runtime.UtiPath is null: Use This Instead's replacements.json was not found");

        // --- changing settings (direct manipulation, as the checkbox would, not a pixel click) ----

        [When("Resolve This Instead disables the alias from {string} to {string}")]
        public void Disable(PickleContext ctx, string oldId, string newId) => Settings(ctx).enabled[FindRule(ctx, oldId, newId).Id] = false;

        // --- ambiguity: several successors of the same old id, at the same distance -------------

        [Then("Resolve This Instead the pair from {string} is ambiguous between its loaded successors")]
        public void IsAmbiguous(PickleContext ctx, string oldId)
        {
            Resolution res = Runtime.Resolver.ResolvePackageId(oldId, Runtime.IsLoaded, Runtime.Choices);
            ctx.Assert(res.Status == ResolutionStatus.Ambiguous, $"expected Ambiguous, got {res.Status}");
            ctx.Assert(Runtime.TargetForId(oldId) == null, "an ambiguous pair must not resolve to a target on its own");
        }

        [When("Resolve This Instead picks {string} as the successor of {string} for the next launch")]
        public void Pick(PickleContext ctx, string newId, string oldId) => Settings(ctx).picks[PackageIds.Normalize(oldId)] = PackageIds.Normalize(newId);

        [Then("Resolve This Instead the pair from {string} resolved to {string} after the previous launch's pick")]
        public void ResolvedToPickAfterRestart(PickleContext ctx, string oldId, string newId)
        {
            ctx.Assert(Runtime.TargetForId(oldId) == PackageIds.Normalize(newId),
                $"'{oldId}' resolved to '{Runtime.TargetForId(oldId) ?? "nothing"}', expected '{newId}'");
        }

        // --- persistence, played as a pair: -Filter restart-write -Then restart-read -------------

        /// <summary>Tracks whether the write ran in THIS process, and whether teardown must leave the file as this run set it.</summary>
        private static bool wroteInThisProcess;
        private static bool keepSettings;
        private static bool cleanFileAfterRead;

        [When("Resolve This Instead keeps its settings for the next launch")]
        public void Keep(PickleContext ctx)
        {
            Mod(ctx).WriteSettings();
            wroteInThisProcess = true;
            keepSettings = true;
        }

        [Given("Resolve This Instead the settings kept by the previous launch are in place")]
        public void KeptByPreviousLaunch(PickleContext ctx)
        {
            ctx.Assert(!wroteInThisProcess,
                "the writer ran in THIS process, so nothing here was read back from disk: this is a restart test that never restarted. "
                + "Play the pair with -Filter restart-write -Then restart-read.");
            keepSettings = false;
            cleanFileAfterRead = true;
        }

        /// <summary>
        /// The DESERIALIZED value, not a guess at the settings file's raw XML shape for a Dictionary&lt;string, bool&gt; (Scribe's exact
        /// tag encoding for a dictionary key containing '.' and '&gt;' is not established here). In the second process of a restart pair,
        /// this dictionary was populated by Mod.GetSettings -&gt; LoadedModManager.ReadModSettings before any scenario ran: reading the field
        /// checks the real deserialization the game performed, not a pattern in a file. A missing entry means "no opinion" (falls back to
        /// the rule's Compat), so an absent key is asserted separately from an explicit False.
        /// </summary>
        [Then("Resolve This Instead the alias from {string} to {string} was loaded as {word}")]
        public void LoadedAs(PickleContext ctx, string oldId, string newId, string state)
        {
            ctx.Require(state == "on" || state == "off" || state == "unset", "state must be 'on', 'off' or 'unset'");
            string id = FindRule(ctx, oldId, newId).Id;
            bool has = Settings(ctx).enabled.TryGetValue(id, out bool value);
            if (state == "unset")
            {
                ctx.Assert(!has, $"'{id}' has an explicit value ({value}) after this process's own load; expected none");
                return;
            }
            ctx.Assert(has && value == (state == "on"),
                $"'{id}' was loaded as {(has ? value.ToString() : "unset")}, expected {state}. Settings file: {SettingsFilePath(Mod(ctx))}");
        }

        private static string SettingsFilePath(ResolveThisInsteadMod mod) =>
            Path.Combine(GenFilePaths.ConfigFolderPath, GenText.SanitizeFilename($"Mod_{mod.Content.FolderName}_{mod.GetType().Name}.xml"));

        [Then("Resolve This Instead a settings file exists on disk")]
        public void FileExists(PickleContext ctx) => ctx.Assert(File.Exists(SettingsFilePath(Mod(ctx))), $"no settings file at '{SettingsFilePath(Mod(ctx))}'");

        // --- teardown -------------------------------------------------------------------------

        [AfterScenario]
        public void Teardown(PickleContext ctx)
        {
            try
            {
                var def = DefDatabase<MainButtonDef>.GetNamedSilentFail(ShortcutDefName);
                if (def != null) def.buttonVisible = false;
                if (keepSettings) return;
                ResolveThisInsteadMod.Settings?.ResetToDefaults();
                if (cleanFileAfterRead)
                {
                    cleanFileAfterRead = false;
                    ResolveThisInsteadMod.Instance?.WriteSettings();
                }
            }
            catch (Exception e)
            {
                Log.Warning($"[Resolve This Instead tests] the scenario's state could not be restored: {e.Message}");
            }
        }
    }

    /// <summary>The optional MainButtons shortcut: hidden by its own def, revealable, opens the same settings.</summary>
    [PickleSteps]
    public class ShortcutSteps
    {
        private const string ShortcutDefName = "RTI_Settings";

        private static MainButtonDef Shortcut(PickleContext ctx)
        {
            var def = DefDatabase<MainButtonDef>.GetNamedSilentFail(ShortcutDefName);
            ctx.Require(def != null, $"no MainButtonDef named '{ShortcutDefName}'");
            return def;
        }

        [Then("Resolve This Instead the shortcut is hidden on a clean configuration")]
        public void ShortcutHidden(PickleContext ctx)
        {
            var def = Shortcut(ctx);
            ctx.Assert(!def.buttonVisible && !def.Worker.Visible, $"the shortcut is visible with buttonVisible={def.buttonVisible} and Worker.Visible={def.Worker.Visible}");
        }

        [When("Resolve This Instead reveals its shortcut as a customization mod would")]
        public void Reveal(PickleContext ctx) => Shortcut(ctx).buttonVisible = true;

        [When("Resolve This Instead hides its shortcut again")]
        public void Hide(PickleContext ctx) => Shortcut(ctx).buttonVisible = false;

        [Then("Resolve This Instead the shortcut is drawn and enabled")]
        public void ShortcutDrawn(PickleContext ctx)
        {
            var worker = Shortcut(ctx).Worker;
            ctx.Assert(worker.Visible, "the revealed shortcut is still not drawn");
            ctx.Assert(!worker.Disabled, "the revealed shortcut is greyed out");
        }

        // Wrapped: the first real run (2026-09-27) failed here with a bare "Object reference not set to an instance of an
        // object", no attribution to step or line. Whatever throws next time, the scenario's own failure message will
        // carry the real exception type and stack instead of Pickle's generic top-level text.
        [When("Resolve This Instead activates its shortcut")]
        public void Activate(PickleContext ctx)
        {
            MainButtonWorker worker = Shortcut(ctx).Worker;
            try
            {
                worker.Activate();
            }
            catch (Exception ex)
            {
                throw new Exception($"{worker.GetType().Name}.Activate() threw: {ex}", ex);
            }
        }
    }
}
