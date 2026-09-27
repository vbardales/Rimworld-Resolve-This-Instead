using System;
using System.Collections.Generic;
using System.Linq;

namespace ResolveThisInstead
{
    public enum ResolutionStatus
    {
        // The asked mod is itself loaded: nothing to resolve.
        NotNeeded,
        // No rule leaves it.
        NoRule,
        // Rules exist but none is switched on.
        Disabled,
        // Rules are on, but no mod of the chain is loaded.
        NoSuccessorLoaded,
        // Several successors at the same distance are loaded and the player has not picked one.
        Ambiguous,
        Resolved,
    }

    public sealed class Resolution
    {
        public ResolutionStatus Status;

        // The loaded package id that stands in for the asked one (or the asked one itself when NotNeeded).
        public string Target;

        // The loaded successors that were in the running (several when Ambiguous).
        public List<string> Candidates = new List<string>();

        public bool IsAlias => Status == ResolutionStatus.Resolved;
    }

    // Follows replacement chains (A -> B -> C) from an old mod to the successor that is really loaded.
    public sealed class AliasResolver
    {
        private readonly AliasCatalog catalog;

        public AliasResolver(AliasCatalog catalog)
        {
            this.catalog = catalog;
        }

        // A lookup by package id (MayRequire, IsActive, GetActiveModWithIdentifier).
        public Resolution ResolvePackageId(string packageId, Func<string, bool> isLoaded, PlayerChoices player)
        {
            string start = PackageIds.Normalize(packageId);
            if (start.Length > 0 && isLoaded(start))
            {
                return new Resolution { Status = ResolutionStatus.NotNeeded, Target = start };
            }
            return Walk(start, catalog.FromPackageId(start), isLoaded, player);
        }

        // A lookup by display name (PatchOperationFindMod). Rules that have no package id are reached this way.
        public Resolution ResolveName(string displayName, Func<string, bool> isLoaded, PlayerChoices player)
        {
            return Walk("name:" + (displayName ?? "").Trim().ToLowerInvariant(), catalog.FromName(displayName), isLoaded, player);
        }

        private Resolution Walk(string startKey, IEnumerable<AliasRule> firstEdges, Func<string, bool> isLoaded, PlayerChoices player)
        {
            var first = firstEdges.ToList();
            if (first.Count == 0)
            {
                return new Resolution { Status = ResolutionStatus.NoRule };
            }
            var enabledFirst = first.Where(r => AliasCatalog.IsEnabled(r, player)).ToList();
            if (enabledFirst.Count == 0)
            {
                return new Resolution { Status = ResolutionStatus.Disabled };
            }

            // Breadth first, one visited set: a cycle (Use This Instead has seven) cannot loop, and each id is met at its shortest distance.
            var depth = new Dictionary<string, int>();
            var queue = new Queue<string>();
            foreach (AliasRule edge in enabledFirst)
            {
                if (!depth.ContainsKey(edge.NewPackageId))
                {
                    depth[edge.NewPackageId] = 1;
                    queue.Enqueue(edge.NewPackageId);
                }
            }
            while (queue.Count > 0)
            {
                string current = queue.Dequeue();
                foreach (AliasRule edge in catalog.FromPackageId(current))
                {
                    if (!AliasCatalog.IsEnabled(edge, player) || depth.ContainsKey(edge.NewPackageId) || edge.NewPackageId == startKey)
                    {
                        continue;
                    }
                    depth[edge.NewPackageId] = depth[current] + 1;
                    queue.Enqueue(edge.NewPackageId);
                }
            }

            var loaded = depth.Where(kv => isLoaded(kv.Key)).ToList();
            if (loaded.Count == 0)
            {
                return new Resolution { Status = ResolutionStatus.NoSuccessorLoaded };
            }

            // The most recent loaded link is the maintained one, unless the player picked another.
            int deepest = loaded.Max(kv => kv.Value);
            var best = loaded.Where(kv => kv.Value == deepest).Select(kv => kv.Key).OrderBy(k => k, StringComparer.Ordinal).ToList();
            var all = loaded.Select(kv => kv.Key).OrderBy(k => k, StringComparer.Ordinal).ToList();
            if (player != null && player.Picks.TryGetValue(startKey, out string picked) && all.Contains(PackageIds.Normalize(picked)))
            {
                return new Resolution { Status = ResolutionStatus.Resolved, Target = PackageIds.Normalize(picked), Candidates = all };
            }
            if (best.Count > 1)
            {
                return new Resolution { Status = ResolutionStatus.Ambiguous, Candidates = best };
            }
            return new Resolution { Status = ResolutionStatus.Resolved, Target = best[0], Candidates = all };
        }
    }
}
