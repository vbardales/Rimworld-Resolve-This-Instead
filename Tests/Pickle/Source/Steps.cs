using System.Collections.Generic;
using System.Linq;
using RimWorks.Pickle;
using Verse;

namespace ResolveThisInstead.PickleSteps
{
    /// <summary>
    /// What only a started game shows about the side effects of the aliases. They must answer while the game LOADS its data (the defs and
    /// patches of the scenarios prove that) and stop once loading is over: the mod manager screens, other mods' runtime checks and the game's
    /// own incompatibleWith warnings have to see the real mod list. The engine's rules are proved by Tests/UnitTests.
    /// </summary>
    [PickleSteps]
    public class AliasSideEffectSteps
    {
        [Then("Resolve This Instead: the load is over and aliases no longer answer")]
        public void LoadIsOver(PickleContext ctx)
        {
            ctx.Assert(!Runtime.Loading, "Runtime.Loading is still true: StaticConstructorOnStartupUtility.CallAll did not end the loading phase");
        }

        [Then("Resolve This Instead: the game does not treat {string} as loaded")]
        public void NotTreatedAsLoaded(PickleContext ctx, string packageId)
        {
            ctx.Assert(!ModsConfig.IsActive(packageId), $"ModsConfig.IsActive(\"{packageId}\") is true after loading: the id stayed in the IsActive set");
            ctx.Assert(ModLister.GetActiveModWithIdentifier(packageId) == null,
                $"ModLister.GetActiveModWithIdentifier(\"{packageId}\") returns {ModLister.GetActiveModWithIdentifier(packageId)?.Name}");
            ctx.Assert(ModLister.GetActiveModWithIdentifier(packageId, ignorePostfix: true) == null,
                $"ModLister.GetActiveModWithIdentifier(\"{packageId}\", ignorePostfix) returns {ModLister.GetActiveModWithIdentifier(packageId, ignorePostfix: true)?.Name}");
            ctx.Assert(!ModsConfig.ActiveModsInLoadOrder.Any(m => string.Equals(m.PackageId, packageId, System.StringComparison.OrdinalIgnoreCase)),
                $"\"{packageId}\" is in the active load order");
        }

        [Then("Resolve This Instead: the game does not treat the mod named {string} as loaded")]
        public void NameNotTreatedAsLoaded(PickleContext ctx, string name)
        {
            ctx.Assert(!ModLister.HasActiveModWithName(name), $"ModLister.HasActiveModWithName(\"{name}\") is true after loading");
        }

        /// <summary>The companion plays mod C, whose Mod constructor asks for A (absent) while B (its replacement) is loaded.</summary>
        [Then("Resolve This Instead: the consumer mod's constructor saw {string} as absent")]
        public void ConsumerSawAbsent(PickleContext ctx, string packageId)
        {
            ctx.Require(packageId == ResolveThisInstead.TestConsumer.ConsumerMod.OldPackageId, "the consumer mod only asks about " + ResolveThisInstead.TestConsumer.ConsumerMod.OldPackageId);
            ctx.Require(ResolveThisInstead.TestConsumer.ConsumerMod.IsActiveOldInConstructor.HasValue, "the consumer mod's constructor did not run");
            ctx.Assert(ResolveThisInstead.TestConsumer.ConsumerMod.IsActiveOldInConstructor == false, "ModsConfig.IsActive(\"" + packageId + "\") was true inside a Mod constructor: the alias answered too early");
            ctx.Assert(ResolveThisInstead.TestConsumer.ConsumerMod.GetActiveOldInConstructor == false, "ModLister.GetActiveModWithIdentifier(\"" + packageId + "\") returned a mod inside a Mod constructor: the alias answered too early");
            ctx.Assert(ResolveThisInstead.TestConsumer.ConsumerMod.OldModHandleInConstructor == false, "a Mod handle for \"" + packageId + "\" existed inside a Mod constructor");
        }

        /// <summary>The "Mod options" list is built from the Mod classes the game instantiated: an absent mod has none, alias or not.</summary>
        [Then("Resolve This Instead: the mod options list has no entry for {string}")]
        public void NoModOptionsEntry(PickleContext ctx, string packageId)
        {
            var handle = LoadedModManager.ModHandles.FirstOrDefault(h => h.Content != null && string.Equals(h.Content.PackageId, packageId, System.StringComparison.OrdinalIgnoreCase));
            ctx.Assert(handle == null, "the mod options list has an entry for \"" + packageId + "\": " + handle?.SettingsCategory());
        }

        /// <summary>
        /// The generic runtime mechanism (Runtime.MayAnswer's third window), proven from TWO different callers on the SAME pair, to show it
        /// discriminates by caller identity, not by rule: this step's own call (from ResolveThisInstead.PickleSteps.dll, a mod that did NOT
        /// ask for runtime trust) must still see reality, while the separately-staged consumer C (ResolveThisInstead.TestConsumer.dll, whose
        /// own embedded file DID ask for it) must see the alias.
        /// </summary>
        [Then("Resolve This Instead: an untrusted mod's own call to IsActive still sees {string} as absent")]
        public void UntrustedCallSeesReality(PickleContext ctx, string packageId)
        {
            ctx.Assert(!ModsConfig.IsActive(packageId), $"ModsConfig.IsActive(\"{packageId}\") was true from an untrusted caller (this very step): runtime trust leaked to a mod that never asked for it");
        }

        [When("Resolve This Instead: the test consumer C checks {string} from its own runtime code")]
        public void ConsumerChecksAfterLoad(PickleContext ctx, string packageId)
        {
            ctx.Require(packageId == ResolveThisInstead.TestConsumer.ConsumerMod.OldPackageId, "the consumer mod only asks about " + ResolveThisInstead.TestConsumer.ConsumerMod.OldPackageId);
            ResolveThisInstead.TestConsumer.ConsumerMod.CheckAfterLoad();
        }

        [Then("Resolve This Instead: the test consumer C's own runtime code saw {string} as present")]
        public void ConsumerSawPresentAfterLoad(PickleContext ctx, string packageId)
        {
            ctx.Require(ResolveThisInstead.TestConsumer.ConsumerMod.IsActiveOldAfterLoad.HasValue, "the consumer's check did not run");
            ctx.Assert(ResolveThisInstead.TestConsumer.ConsumerMod.IsActiveOldAfterLoad == true,
                $"ModsConfig.IsActive(\"{packageId}\") was false from the trusted consumer's own runtime code: the runtime opt-in did not work");
        }

        /// <summary>The check behind the mod list's "incompatible with" warning: ModsConfig.FindConflicts asks GetActiveModWithIdentifier for each id.</summary>
        [Then("Resolve This Instead: the incompatibleWith of {string} names no loaded mod")]
        public void IncompatibleWithNamesNothing(PickleContext ctx, string packageId)
        {
            ModMetaData mod = ModLister.AllInstalledMods.FirstOrDefault(m => string.Equals(m.PackageId, packageId, System.StringComparison.OrdinalIgnoreCase));
            ctx.Require(mod != null, $"no installed mod with package id '{packageId}'");
            ctx.Require(mod.IncompatibleWith != null && mod.IncompatibleWith.Count > 0, $"'{packageId}' declares no incompatibleWith: nothing to check");
            var flagged = new List<string>();
            foreach (string id in mod.IncompatibleWith)
            {
                ModMetaData conflict = ModLister.GetActiveModWithIdentifier(id, ignorePostfix: true);
                if (conflict != null)
                {
                    flagged.Add(id + " -> " + conflict.Name);
                }
            }
            ctx.Assert(flagged.Count == 0, "the mod list would warn that '" + packageId + "' is incompatible with: " + string.Join(", ", flagged));
        }
    }
}
