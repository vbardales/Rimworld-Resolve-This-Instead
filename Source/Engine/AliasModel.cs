using System;
using System.Collections.Generic;

namespace ResolveThisInstead
{
    // Pure model of "mod X is replaced by mod Y". No dependency on the game, so that every rule can be checked on its own
    // (Tests/UnitTests). The in-game hooks (Harmony) and the settings window sit on top of this and add nothing to the rules.

    // Does the successor keep working for the mods that were written against the old one?
    public enum Compat
    {
        Unknown,
        Yes,
        No,
    }

    // Where a rule comes from. A consuming mod knows its own code: its declaration outranks the snapshot of Use This Instead,
    // which only proposes pairs.
    public enum RuleSource
    {
        Snapshot,
        Embedded,
    }

    public sealed class AliasRule
    {
        // Lower case, without a Steam/copy postfix. Empty when the source rule has no packageId (16% of Use This Instead's rules).
        public string OldPackageId = "";
        public string OldName;
        public string OldWorkshopId;
        public string NewPackageId = "";
        public string NewName;
        public string NewWorkshopId;
        public Compat Compat = Compat.Unknown;
        public RuleSource Source = RuleSource.Snapshot;
        public string Note;

        // Parsed from an embedded file's <runtime>true</runtime>: this consumer asks that ITS OWN code, called after loading, also see the
        // alias (Mod construction still never does, whoever asks - see Runtime.MayAnswer). Never set from the snapshot. Not merged into a
        // catalog rule: Runtime tracks trust per calling ASSEMBLY (the declaring mod's), not per rule, since the same pair can be trusted by
        // one consumer's code and not by another's.
        public bool RuntimeOptIn;

        // Identity of an alias, and the key of its checkbox in the settings: stable across snapshot refreshes.
        public string Id
        {
            get
            {
                string from = OldPackageId.Length > 0 ? OldPackageId : "name:" + (OldName ?? "").Trim().ToLowerInvariant();
                return from + ">" + NewPackageId;
            }
        }

        public bool IsIdentity => OldPackageId.Length > 0 && OldPackageId == NewPackageId;
    }

    public static class PackageIds
    {
        // The game compares package ids case-insensitively and ignores the "_steam" / "_copy" postfix it adds to duplicates.
        public static string Normalize(string packageId)
        {
            if (string.IsNullOrWhiteSpace(packageId))
            {
                return "";
            }
            string id = packageId.Trim().ToLowerInvariant();
            // Postfixes can stack ("x_copy_copy" exists in Use This Instead's data): strip until stable.
            bool changed = true;
            while (changed)
            {
                changed = false;
                foreach (string postfix in new[] { "_steam", "_copy" })
                {
                    if (id.EndsWith(postfix, StringComparison.Ordinal))
                    {
                        id = id.Substring(0, id.Length - postfix.Length);
                        changed = true;
                    }
                }
            }
            return id;
        }
    }

    // What the player decided in the settings menu. A missing entry means "no opinion".
    public sealed class PlayerChoices
    {
        public readonly Dictionary<string, bool> Enabled = new Dictionary<string, bool>();

        // old key (packageId or "name:...") -> the successor packageId the player picked when several are loaded.
        public readonly Dictionary<string, string> Picks = new Dictionary<string, string>();
    }
}
