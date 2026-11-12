using System;
using System.IO;
using RimWorks.Pickle;
using UnityEngine;
using Verse;

namespace ResolveThisInstead.PickleSteps
{
    /// <summary>
    /// The settings window's three small promises that only a started game shows: the "Debug logs" switch really gates the debug lines,
    /// "Restore defaults" empties every choice, and a settings file written before the debug option existed still loads.
    /// Direct manipulation of the settings object, as the checkbox and the button do (the window code sets the same fields).
    /// </summary>
    [PickleSteps]
    public class DefaultsSteps
    {
        private static RtiSettings Settings(PickleContext ctx)
        {
            var settings = ResolveThisInsteadMod.Settings;
            ctx.Require(settings != null, "ResolveThisInsteadMod.Settings is null");
            return settings;
        }

        [Then("Resolve This Instead debug logs are off on a clean configuration")]
        public void DebugOffByDefault(PickleContext ctx)
        {
            ctx.Assert(!Settings(ctx).debugLogs, "debugLogs is on in a clean configuration");
            ctx.Assert(!Runtime.DebugEnabled, "Runtime.DebugEnabled is on in a clean configuration");
        }

        [When("Resolve This Instead turns its debug logs {word} as the checkbox does")]
        public void TurnDebug(PickleContext ctx, string state)
        {
            ctx.Require(state == "on" || state == "off", "state must be 'on' or 'off'");
            var settings = Settings(ctx);
            settings.debugLogs = state == "on";
            Runtime.DebugEnabled = settings.debugLogs;
        }

        [Then("Resolve This Instead a debug line is {word}")]
        public void DebugLine(PickleContext ctx, string state)
        {
            ctx.Require(state == "logged" || state == "suppressed", "state must be 'logged' or 'suppressed'");
            string probe = "pickle-debug-probe-" + Guid.NewGuid().ToString("N");
            bool seen = false;
            Application.LogCallback handler = (text, stack, type) => { if (text != null && text.Contains(probe)) seen = true; };
            Application.logMessageReceived += handler;
            try
            {
                Runtime.Debug(probe);
            }
            finally
            {
                Application.logMessageReceived -= handler;
            }
            ctx.Assert(seen == (state == "logged"), $"a debug line was {(seen ? "logged" : "suppressed")}, expected {state}");
        }

        [Given("Resolve This Instead has stored choices and debug logs on")]
        public void StoreChoices(PickleContext ctx)
        {
            var settings = Settings(ctx);
            settings.enabled["pickle.old.mod->pickle.new.mod"] = false;
            settings.picks["pickle.old.mod"] = "pickle.new.mod";
            settings.debugLogs = true;
            Runtime.DebugEnabled = true;
        }

        [When("Resolve This Instead restores its defaults as the button does")]
        public void RestoreDefaults(PickleContext ctx)
        {
            var settings = Settings(ctx);
            settings.ResetToDefaults();
            Runtime.DebugEnabled = settings.debugLogs;
        }

        [Then("Resolve This Instead no choice is stored and debug logs are off")]
        public void NothingStored(PickleContext ctx)
        {
            var settings = Settings(ctx);
            ctx.Assert(settings.enabled.Count == 0, $"{settings.enabled.Count} enabled choice(s) remain after restoring defaults");
            ctx.Assert(settings.picks.Count == 0, $"{settings.picks.Count} pick(s) remain after restoring defaults");
            ctx.Assert(!settings.debugLogs && !Runtime.DebugEnabled, "debug logs are still on after restoring defaults");
        }

        [Then("Resolve This Instead reads a settings file written before the debug option existed")]
        public void ReadsOldFile(PickleContext ctx)
        {
            string path = Path.Combine(GenFilePaths.ConfigFolderPath, "RtiPickleOldSettings.xml");
            File.WriteAllText(path,
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<SettingsBlock><ModSettings Class=\"ResolveThisInstead.RtiSettings\">"
                + "<enabled><keys><li>pickle.old.mod->pickle.new.mod</li></keys><values><li>False</li></values></enabled>"
                + "</ModSettings></SettingsBlock>");
            RtiSettings loaded = null;
            try
            {
                Scribe.loader.InitLoading(path);
                Scribe.EnterNode("SettingsBlock");
                Scribe_Deep.Look(ref loaded, "ModSettings");
                Scribe.ExitNode();
                Scribe.loader.FinalizeLoading();
            }
            finally
            {
                try { File.Delete(path); } catch { }
            }
            ctx.Assert(loaded != null, "the old settings file did not load");
            ctx.Assert(!loaded.debugLogs, "debugLogs is on after loading a file that never had it");
            ctx.Assert(loaded.picks != null && loaded.picks.Count == 0, "picks is not an empty dictionary after loading a file without it");
            ctx.Assert(loaded.enabled.Count == 1 && loaded.enabled["pickle.old.mod->pickle.new.mod"] == false, "the stored choice of the old file was lost");
        }

        [AfterScenario]
        public void Teardown(PickleContext ctx)
        {
            var settings = ResolveThisInsteadMod.Settings;
            if (settings != null)
            {
                settings.ResetToDefaults();
                Runtime.DebugEnabled = false;
            }
        }
    }
}
