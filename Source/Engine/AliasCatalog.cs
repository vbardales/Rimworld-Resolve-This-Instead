using System;
using System.Collections.Generic;
using System.Linq;

namespace ResolveThisInstead
{
    // The set of known replacements, from Use This Instead's data and from the files that consuming mods embed.
    // Rules are indexed by old package id and by old display name when they are added: the resolver asks for them once per step of a
    // chain and the game asks about thousands of ids while it loads, so a scan of every rule per lookup would be paid at startup.
    public sealed class AliasCatalog
    {
        private static readonly IEnumerable<AliasRule> None = new AliasRule[0];

        private readonly Dictionary<string, AliasRule> byId = new Dictionary<string, AliasRule>();
        private readonly Dictionary<string, List<AliasRule>> byOldId = new Dictionary<string, List<AliasRule>>();
        private readonly Dictionary<string, List<AliasRule>> byOldName = new Dictionary<string, List<AliasRule>>(StringComparer.Ordinal);

        public int Count => byId.Count;

        // Every rule, in insertion order. Cheap: use this in loops.
        public IEnumerable<AliasRule> All => byId.Values;

        // Every rule, sorted by identity. Allocates and sorts: for tests and for one-off listings, not for hot paths.
        public IEnumerable<AliasRule> Rules => byId.Values.OrderBy(r => r.Id, StringComparer.Ordinal);

        public void AddRange(IEnumerable<AliasRule> rules)
        {
            foreach (AliasRule rule in rules)
            {
                Add(rule);
            }
        }

        // Rules with the same identity are merged: the embedded declaration decides compatibility, the snapshot fills in names and
        // Workshop ids. Between two embedded declarations "no" beats "yes": one mod that knows it breaks is enough to warn.
        // A rule found only by display name and a rule found by package id that describe the same pair (same successor, same old name)
        // are one alias too: otherwise a mod that vouches by name would leave the id lookup of the same pair unvouched.
        public void Add(AliasRule rule)
        {
            if (rule == null || rule.NewPackageId.Length == 0 || rule.IsIdentity)
            {
                return;
            }
            if (rule.OldPackageId.Length == 0 && string.IsNullOrWhiteSpace(rule.OldName))
            {
                return;
            }
            if (byId.TryGetValue(rule.Id, out AliasRule existing))
            {
                MergeInto(existing, rule);
                return;
            }
            AliasRule twin = FindTwin(rule);
            if (twin != null && rule.OldPackageId.Length == 0)
            {
                MergeInto(twin, rule);
                return;
            }
            AliasRule stored = Store(Copy(rule));
            if (twin != null)
            {
                // The incoming rule has the package id, the twin only the name: the twin joins it.
                MergeInto(stored, twin);
                Remove(twin);
            }
        }

        // Rules leaving a package id.
        public IEnumerable<AliasRule> FromPackageId(string packageId)
        {
            string id = PackageIds.Normalize(packageId);
            return id.Length > 0 && byOldId.TryGetValue(id, out List<AliasRule> list) ? list : None;
        }

        // Rules leaving a display name (what PatchOperationFindMod compares, exactly). Used for rules with no packageId too.
        public IEnumerable<AliasRule> FromName(string name)
        {
            return !string.IsNullOrEmpty(name) && byOldName.TryGetValue(name, out List<AliasRule> list) ? list : None;
        }

        // Effective state of a checkbox. The player outranks everything; then the consuming mod's declaration; a bare snapshot
        // pair is only a proposal and stays off ("unknown" is not "compatible").
        public static bool IsEnabled(AliasRule rule, PlayerChoices player)
        {
            if (player != null && player.Enabled.TryGetValue(rule.Id, out bool chosen))
            {
                return chosen;
            }
            return rule.Compat == Compat.Yes;
        }

        private AliasRule FindTwin(AliasRule rule)
        {
            if (string.IsNullOrWhiteSpace(rule.OldName) || !byOldName.TryGetValue(rule.OldName, out List<AliasRule> sameName))
            {
                return null;
            }
            bool incomingHasId = rule.OldPackageId.Length > 0;
            return sameName.FirstOrDefault(r => r.NewPackageId == rule.NewPackageId && (r.OldPackageId.Length > 0) != incomingHasId);
        }

        private static void MergeInto(AliasRule existing, AliasRule rule)
        {
            existing.OldName = existing.OldName ?? rule.OldName;
            existing.NewName = existing.NewName ?? rule.NewName;
            existing.OldWorkshopId = existing.OldWorkshopId ?? rule.OldWorkshopId;
            existing.NewWorkshopId = existing.NewWorkshopId ?? rule.NewWorkshopId;
            existing.Note = existing.Note ?? rule.Note;
            if (rule.Source == RuleSource.Embedded)
            {
                if (existing.Source == RuleSource.Snapshot)
                {
                    existing.Source = RuleSource.Embedded;
                    existing.Compat = rule.Compat;
                }
                else if (rule.Compat == Compat.No || (rule.Compat == Compat.Yes && existing.Compat == Compat.Unknown))
                {
                    existing.Compat = rule.Compat;
                }
            }
        }

        private AliasRule Store(AliasRule rule)
        {
            byId[rule.Id] = rule;
            if (rule.OldPackageId.Length > 0)
            {
                Index(byOldId, rule.OldPackageId, rule);
            }
            if (!string.IsNullOrWhiteSpace(rule.OldName))
            {
                Index(byOldName, rule.OldName, rule);
            }
            return rule;
        }

        private void Remove(AliasRule rule)
        {
            byId.Remove(rule.Id);
            if (rule.OldPackageId.Length > 0 && byOldId.TryGetValue(rule.OldPackageId, out List<AliasRule> ids))
            {
                ids.Remove(rule);
            }
            if (!string.IsNullOrWhiteSpace(rule.OldName) && byOldName.TryGetValue(rule.OldName, out List<AliasRule> names))
            {
                names.Remove(rule);
            }
        }

        private static void Index(Dictionary<string, List<AliasRule>> index, string key, AliasRule rule)
        {
            if (!index.TryGetValue(key, out List<AliasRule> list))
            {
                index[key] = list = new List<AliasRule>();
            }
            list.Add(rule);
        }

        private static AliasRule Copy(AliasRule r)
        {
            return new AliasRule
            {
                OldPackageId = r.OldPackageId,
                OldName = r.OldName,
                OldWorkshopId = r.OldWorkshopId,
                NewPackageId = r.NewPackageId,
                NewName = r.NewName,
                NewWorkshopId = r.NewWorkshopId,
                Compat = r.Compat,
                Source = r.Source,
                Note = r.Note,
            };
        }
    }
}
