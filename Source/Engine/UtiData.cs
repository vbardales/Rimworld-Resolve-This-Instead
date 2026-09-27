using System;
using System.Collections.Generic;
using System.Globalization;
using ResolveThisInstead.ThirdParty;

namespace ResolveThisInstead
{
    // Reads the rules that Use This Instead (Mlie, MIT) keeps in its own mod folder. This library downloads nothing: UTI fetches
    // replacements.json.gz at startup and writes replacements.json at the root of its mod folder, and we read that file.
    // Its location and format are not a public interface of UTI, so everything here is tolerant: a bad rule is skipped and counted,
    // a bad file gives an empty result and a message, and nothing throws.
    public static class UtiData
    {
        public const string PackageId = "mlie.usethisinstead";
        public const string FileName = "replacements.json";
        private const int MaxReportedErrors = 5;

        public sealed class ParseResult
        {
            // The "version" stamp of the data (an ISO time), shown in the settings menu because the file can be stale.
            public string Version;
            public int RulesRead;
            public int RulesSkipped;
            public readonly List<AliasRule> Rules = new List<AliasRule>();
            public readonly List<string> Errors = new List<string>();
        }

        // Finds UTI's data file among the running mods (package id -> root directory). Null when UTI is not loaded or has no file yet.
        public static string Locate(IEnumerable<KeyValuePair<string, string>> modRoots, Func<string, bool> fileExists)
        {
            foreach (KeyValuePair<string, string> mod in modRoots)
            {
                if (PackageIds.Normalize(mod.Key) != PackageId || string.IsNullOrEmpty(mod.Value))
                {
                    continue;
                }
                string path = mod.Value.TrimEnd('/', '\\') + "/" + FileName;
                return fileExists(path) ? path : null;
            }
            return null;
        }

        public static ParseResult Parse(string json)
        {
            var result = new ParseResult();
            if (string.IsNullOrWhiteSpace(json))
            {
                result.Errors.Add("the file is empty");
                return result;
            }
            // MiniJSON does not accept a byte order mark; the file from Use This Instead has one. It also claims not to throw, but it does on a
            // truncated file (a download cut short): the tests found it, so the call is wrapped.
            Dictionary<string, object> root;
            try
            {
                root = Json.Deserialize(json.TrimStart((char)0xFEFF)) as Dictionary<string, object>;
            }
            catch (Exception ex)
            {
                result.Errors.Add("the file could not be read as JSON (" + ex.GetType().Name + "), probably truncated");
                return result;
            }
            if (root == null)
            {
                result.Errors.Add("the file is not a JSON object");
                return result;
            }
            result.Version = Text(root, "version");
            var rules = root.TryGetValue("rules", out object raw) ? raw as List<object> : null;
            if (rules == null)
            {
                result.Errors.Add("no \"rules\" array");
                return result;
            }
            foreach (object item in rules)
            {
                result.RulesRead++;
                var rule = item as Dictionary<string, object>;
                string newId = rule == null ? "" : PackageIds.Normalize(Text(rule, "newPackageId"));
                string oldId = rule == null ? "" : PackageIds.Normalize(Text(rule, "oldPackageId"));
                string oldName = rule == null ? null : Text(rule, "oldName");
                if (newId.Length == 0 || (oldId.Length == 0 && string.IsNullOrWhiteSpace(oldName)))
                {
                    result.RulesSkipped++;
                    if (result.Errors.Count < MaxReportedErrors)
                    {
                        result.Errors.Add("rule " + result.RulesRead + " skipped: no successor id, or nothing to find the old mod by");
                    }
                    continue;
                }
                result.Rules.Add(new AliasRule
                {
                    OldPackageId = oldId,
                    OldName = oldName,
                    OldWorkshopId = Text(rule, "oldWorkshopId"),
                    NewPackageId = newId,
                    NewName = Text(rule, "newName"),
                    NewWorkshopId = Text(rule, "newWorkshopId"),
                    // A pair of the data is a proposal: nobody has vouched that the successor is a drop-in.
                    Compat = Compat.Unknown,
                    Source = RuleSource.Snapshot,
                });
            }
            return result;
        }

        // A value as text whatever its JSON type: some old package ids are written as numbers (a Workshop id used as a package id).
        private static string Text(Dictionary<string, object> map, string key)
        {
            if (!map.TryGetValue(key, out object value) || value == null)
            {
                return null;
            }
            switch (value)
            {
                case string s:
                    return string.IsNullOrWhiteSpace(s) ? null : s.Trim();
                case long l:
                    return l.ToString(CultureInfo.InvariantCulture);
                case double d:
                    return d.ToString("R", CultureInfo.InvariantCulture);
                default:
                    return null;
            }
        }
    }
}
