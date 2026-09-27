using System;
using System.Collections.Generic;
using System.Xml;

namespace ResolveThisInstead
{
    // The file a consuming mod embeds to say which replacements it supports. It lives at About/ResolveThisInstead.xml (About is at the
    // root whatever LoadFolders says), and looks like:
    //
    //   <ResolveThisInstead>
    //     <alias>
    //       <old><packageId>adaptive.storage.neolithic</packageId><name>Adaptive Storage Neolithic Module</name></old>
    //       <successor><packageId>nelim.adaptivestorageneolithic</packageId></successor>
    //       <compatible>true</compatible>      <!-- true | false | unknown -->
    //       <runtime>false</runtime>           <!-- true | false (default false) -->
    //       <note>Same defNames.</note>
    //     </alias>
    //   </ResolveThisInstead>
    //
    // "true" pre-ticks the alias in the settings, "false" leaves it unticked and says so, "unknown" is the default. The player's box
    // outranks the file. A malformed entry is skipped and reported, never fatal: a broken file must not stop a game from loading.
    //
    // <runtime>true</runtime> asks that, after loading, THIS MOD'S OWN CODE (not just its XML) also see the alias when it calls
    // ModsConfig.IsActive/ModLister.GetActiveModWithIdentifier/HasActiveModWithName directly - never the engine's own bookkeeping (the mod
    // list screen, ModMetaData.Active), and never any other mod that did not ask. Runtime.cs enforces this by trusting the assemblies of
    // whichever mod's embedded file set it, not by anything in the rule itself: a mod that only wants the option, without exposing its own
    // runtime C# to the same fiction, leaves this out (the default).
    public static class EmbeddedFile
    {
        public const string RelativePath = "About/ResolveThisInstead.xml";

        public sealed class ParseResult
        {
            public readonly List<AliasRule> Rules = new List<AliasRule>();
            public readonly List<string> Errors = new List<string>();
        }

        public static ParseResult Parse(string xml)
        {
            var result = new ParseResult();
            var doc = new XmlDocument();
            try
            {
                doc.LoadXml(xml);
            }
            catch (XmlException ex)
            {
                result.Errors.Add("not well-formed: " + ex.Message);
                return result;
            }
            XmlElement root = doc.DocumentElement;
            if (root == null || root.Name != "ResolveThisInstead")
            {
                result.Errors.Add("root element must be <ResolveThisInstead>");
                return result;
            }
            int index = 0;
            foreach (XmlNode node in root.ChildNodes)
            {
                if (node.NodeType != XmlNodeType.Element)
                {
                    continue;
                }
                index++;
                if (node.Name != "alias")
                {
                    result.Errors.Add("entry " + index + ": unexpected <" + node.Name + ">");
                    continue;
                }
                string oldId = PackageIds.Normalize(Text(node, "old/packageId"));
                string oldName = Text(node, "old/name");
                string newId = PackageIds.Normalize(Text(node, "successor/packageId"));
                if (newId.Length == 0)
                {
                    result.Errors.Add("entry " + index + ": <successor><packageId> is required");
                    continue;
                }
                if (oldId.Length == 0 && string.IsNullOrWhiteSpace(oldName))
                {
                    result.Errors.Add("entry " + index + ": <old> needs a packageId or a name");
                    continue;
                }
                if (oldId == newId)
                {
                    result.Errors.Add("entry " + index + ": old and successor are the same package id");
                    continue;
                }
                if (!TryCompat(Text(node, "compatible"), out Compat compat))
                {
                    result.Errors.Add("entry " + index + ": <compatible> must be true, false or unknown");
                    continue;
                }
                if (!TryBool(Text(node, "runtime"), out bool runtimeOptIn))
                {
                    result.Errors.Add("entry " + index + ": <runtime> must be true or false");
                    continue;
                }
                result.Rules.Add(new AliasRule
                {
                    OldPackageId = oldId,
                    OldName = oldName,
                    NewPackageId = newId,
                    NewName = Text(node, "successor/name"),
                    Compat = compat,
                    RuntimeOptIn = runtimeOptIn,
                    Source = RuleSource.Embedded,
                    Note = Text(node, "note"),
                });
            }
            return result;
        }

        private static string Text(XmlNode node, string path)
        {
            string value = node.SelectSingleNode(path)?.InnerText;
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static bool TryCompat(string text, out Compat compat)
        {
            compat = Compat.Unknown;
            if (text == null)
            {
                return true;
            }
            switch (text.ToLowerInvariant())
            {
                case "true":
                case "yes":
                    compat = Compat.Yes;
                    return true;
                case "false":
                case "no":
                    compat = Compat.No;
                    return true;
                case "unknown":
                    return true;
                default:
                    return false;
            }
        }

        private static bool TryBool(string text, out bool value)
        {
            value = false;
            if (text == null)
            {
                return true;
            }
            switch (text.ToLowerInvariant())
            {
                case "true":
                case "yes":
                    value = true;
                    return true;
                case "false":
                case "no":
                    return true;
                default:
                    return false;
            }
        }
    }
}
