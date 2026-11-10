using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ResolveThisInstead;

// Rules of the alias engine, proved outside the game. What only a running game shows (that the lookups are really hooked, that
// patches and MayRequire see the alias) is the job of the proof of concept and of the Pickle suites.
internal static class Program
{
    private static int failures;
    private static int checks;

    private static void Check(bool condition, string name)
    {
        checks++;
        Console.WriteLine((condition ? "PASS " : "FAIL ") + name);
        if (!condition) failures++;
    }

    // The reader of Use This Instead's file: tolerant, never throws.
    private static void UtiReader()
    {
        string sample = (char)0xFEFF + "{\"version\":\"2026-09-26T18:35:17Z\",\"rules\":["
            + "{\"oldWorkshopId\":\"1\",\"oldName\":\"Therapy\",\"oldAuthor\":null,\"oldPackageId\":\"Orion.Therapy_steam\",\"newWorkshopId\":\"2\",\"newName\":\"Therapy (Continued)\",\"newPackageId\":\"Zal.Therapy\",\"oldVersions\":[\"1.5\"],\"newVersions\":[\"1.6\"]},"
            + "{\"oldWorkshopId\":\"3\",\"oldName\":\"Apothecary\",\"oldPackageId\":1797397487,\"newWorkshopId\":\"4\",\"newName\":\"Apothecary (Continued)\",\"newPackageId\":\"Mlie.Apothecary\"},"
            + "{\"oldName\":\"No Package Id\",\"oldPackageId\":\"\",\"newPackageId\":\"succ.id\"},"
            + "{\"oldName\":\"Nothing Known\",\"newPackageId\":\"\"},"
            + "42]}";
        UtiData.ParseResult r = UtiData.Parse(sample);
        Check(r.Version == "2026-09-26T18:35:17Z" && r.RulesRead == 5 && r.Rules.Count == 3 && r.RulesSkipped == 2, "UTI file: BOM tolerated, version read, good rules kept, bad ones skipped and counted");
        Check(r.Rules[0].OldPackageId == "orion.therapy" && r.Rules[0].NewPackageId == "zal.therapy" && r.Rules[0].Compat == Compat.Unknown && r.Rules[0].Source == RuleSource.Snapshot, "UTI file: ids are normalized and a data pair is a proposal (compat unknown)");
        Check(r.Rules[1].OldPackageId == "1797397487", "UTI file: a package id written as a number is read as text");
        Check(r.Rules[2].OldPackageId == "" && r.Rules[2].OldName == "No Package Id", "UTI file: a rule with no old package id is kept by name");
        Check(UtiData.Parse("").Errors.Count == 1 && UtiData.Parse("not json").Errors.Count == 1 && UtiData.Parse("{}").Errors.Count == 1 && UtiData.Parse("[1,2]").Errors.Count == 1, "UTI file: empty, non-JSON, no rules and a bare array are reported, not thrown");
        Check(UtiData.Parse("[1 : 2]").Errors.Count == 1, "UTI file: a stray colon in an array is reported and does not loop forever");
        Check(UtiData.Parse(new string('[', 5000)).Errors.Count == 1, "UTI file: absurdly deep nesting is refused, not a stack overflow");
        Check(UtiData.Parse("{\"rules\":[{\"oldPackageId\":\"a\",\"newPackageId\":\"b\"}").Rules.Count == 0, "UTI file: a truncated file gives no rule and does not throw");
        var mods = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("Ludeon.RimWorld", "C:/game"), new KeyValuePair<string, string>("Mlie.UseThisInstead", "C:/w/3396308787/") };
        Check(UtiData.Locate(mods, p => p == "C:/w/3396308787/replacements.json") == "C:/w/3396308787/replacements.json", "UTI file: found at the root of the UTI mod folder, a trailing slash tolerated");
        Check(UtiData.Locate(mods, p => false) == null, "UTI file: UTI loaded but no file yet gives null");
        Check(UtiData.Locate(new List<KeyValuePair<string, string>> { mods[0] }, p => true) == null, "UTI file: UTI not loaded gives null");
    }

    private static AliasRule Rule(string oldId, string newId, Compat compat = Compat.Yes, RuleSource source = RuleSource.Embedded, string oldName = null)
    {
        return new AliasRule { OldPackageId = PackageIds.Normalize(oldId), NewPackageId = PackageIds.Normalize(newId), OldName = oldName, Compat = compat, Source = source };
    }

    private static AliasResolver Resolver(params AliasRule[] rules)
    {
        var catalog = new AliasCatalog();
        catalog.AddRange(rules);
        return new AliasResolver(catalog);
    }

    private static Func<string, bool> Loaded(params string[] ids)
    {
        var set = new HashSet<string>(ids.Select(PackageIds.Normalize));
        return id => set.Contains(id);
    }

    private static int Main(string[] args)
    {
        // Package ids.
        Check(PackageIds.Normalize("  Mlie.NaturesPrettySweet ") == "mlie.naturesprettysweet", "ids are trimmed and lower-cased");
        Check(PackageIds.Normalize("Foo.Bar_steam") == "foo.bar" && PackageIds.Normalize("Foo.Bar_copy") == "foo.bar" && PackageIds.Normalize("Foo.Bar_copy_copy") == "foo.bar", "the Steam and copy postfixes are dropped, even stacked");
        Check(PackageIds.Normalize(null) == "" && PackageIds.Normalize("  ") == "", "a missing id is empty");

        // One hop.
        var one = Resolver(Rule("old.mod", "new.mod"));
        Check(one.ResolvePackageId("Old.Mod", Loaded("new.mod"), null).Target == "new.mod", "a loaded successor stands in for the old id, whatever the case");
        Check(one.ResolvePackageId("old.mod", Loaded("old.mod"), null).Status == ResolutionStatus.NotNeeded, "the old mod, when loaded, is left alone");
        Check(one.ResolvePackageId("old.mod", Loaded(), null).Status == ResolutionStatus.NoSuccessorLoaded, "no successor loaded: no alias");
        Check(one.ResolvePackageId("unrelated", Loaded("new.mod"), null).Status == ResolutionStatus.NoRule, "an unknown id has no rule");

        // Chains A -> B -> C.
        var chain = Resolver(Rule("a", "b"), Rule("b", "c"));
        Check(chain.ResolvePackageId("a", Loaded("c"), null).Target == "c", "A resolves through B to a loaded C");
        Check(chain.ResolvePackageId("a", Loaded("b"), null).Target == "b", "A resolves to B when only B is loaded");
        Check(chain.ResolvePackageId("a", Loaded("b", "c"), null).Target == "c", "when B and C are loaded, the most recent link wins");
        var pick = new PlayerChoices();
        pick.Picks["a"] = "b";
        Check(chain.ResolvePackageId("a", Loaded("b", "c"), pick).Target == "b", "the player's pick outranks the most recent link");
        var chain3 = Resolver(Rule("a", "b"), Rule("b", "c"), Rule("c", "d"));
        Check(chain3.ResolvePackageId("a", Loaded("d"), null).Target == "d", "a three-hop chain resolves (Realistic Planets has one)");

        // Cycles: Use This Instead has seven 2-cycles and one rule pointing at itself.
        var cycle = Resolver(Rule("x", "y"), Rule("y", "x"));
        Check(cycle.ResolvePackageId("x", Loaded(), null).Status == ResolutionStatus.NoSuccessorLoaded, "a cycle with nothing loaded ends instead of looping");
        Check(cycle.ResolvePackageId("x", Loaded("y"), null).Target == "y", "a cycle still resolves to the loaded member");
        Check(cycle.ResolvePackageId("x", Loaded("x"), null).Status == ResolutionStatus.NotNeeded, "a cycle member that is loaded needs no alias");
        var selfCatalog = new AliasCatalog();
        selfCatalog.Add(Rule("s", "s"));
        Check(!selfCatalog.Rules.Any(), "a rule pointing at itself is dropped");

        // Several successors (17 old ids in the data have more than one).
        var fan = Resolver(Rule("werewolves.old", "werewolves.a"), Rule("werewolves.old", "werewolves.b"));
        Resolution amb = fan.ResolvePackageId("werewolves.old", Loaded("werewolves.a", "werewolves.b"), null);
        Check(amb.Status == ResolutionStatus.Ambiguous && amb.Candidates.Count == 2 && amb.Target == null, "two successors at the same distance are ambiguous and give no target");
        Check(fan.ResolvePackageId("werewolves.old", Loaded("werewolves.b"), null).Target == "werewolves.b", "only the installed one of two successors is used");
        var choose = new PlayerChoices();
        choose.Picks["werewolves.old"] = "werewolves.a";
        Check(fan.ResolvePackageId("werewolves.old", Loaded("werewolves.a", "werewolves.b"), choose).Target == "werewolves.a", "the player's pick settles an ambiguity");

        // Switches: the player outranks the file; a bare snapshot pair is only a proposal.
        var snapshot = Resolver(Rule("p", "q", Compat.Unknown, RuleSource.Snapshot));
        Check(snapshot.ResolvePackageId("p", Loaded("q"), null).Status == ResolutionStatus.Disabled, "a snapshot pair nobody vouched for stays off by default");
        var on = new PlayerChoices();
        on.Enabled["p>q"] = true;
        Check(snapshot.ResolvePackageId("p", Loaded("q"), on).Target == "q", "the player can switch a proposed pair on");
        var declaredNo = Resolver(Rule("m", "n", Compat.No));
        Check(declaredNo.ResolvePackageId("m", Loaded("n"), null).Status == ResolutionStatus.Disabled, "a consumer that says 'not compatible' leaves the alias off");
        var forceOn = new PlayerChoices();
        forceOn.Enabled["m>n"] = true;
        Check(declaredNo.ResolvePackageId("m", Loaded("n"), forceOn).Target == "n", "the player can still override a 'not compatible' declaration");
        var off = new PlayerChoices();
        off.Enabled["old.mod>new.mod"] = false;
        Check(one.ResolvePackageId("old.mod", Loaded("new.mod"), off).Status == ResolutionStatus.Disabled, "the player can switch a declared alias off");
        var brokenChain = Resolver(Rule("a", "b"), Rule("b", "c", Compat.Unknown, RuleSource.Snapshot));
        Check(brokenChain.ResolvePackageId("a", Loaded("c"), null).Status == ResolutionStatus.NoSuccessorLoaded, "a chain stops at a link that is off");

        // Merging: the embedded declaration decides, the snapshot fills in the rest, 'no' beats 'yes'.
        var cat = new AliasCatalog();
        cat.Add(new AliasRule { OldPackageId = "o", NewPackageId = "n", NewName = "N (Continued)", NewWorkshopId = "42", Source = RuleSource.Snapshot });
        cat.Add(Rule("o", "n", Compat.Yes));
        AliasRule merged = cat.Rules.Single();
        Check(merged.Compat == Compat.Yes && merged.Source == RuleSource.Embedded && merged.NewName == "N (Continued)" && merged.NewWorkshopId == "42", "embedded declaration and snapshot names merge into one rule");
        cat.Add(Rule("o", "n", Compat.No));
        Check(cat.Rules.Single().Compat == Compat.No, "between two embedded declarations 'no' beats 'yes'");

        // Rules with no package id are reached by display name (16% of Use This Instead's rules).
        var byName = Resolver(Rule("", "successor.id", Compat.Yes, RuleSource.Snapshot, "Old Display Name"));
        Check(byName.ResolveName("Old Display Name", Loaded("successor.id"), null).Target == "successor.id", "a name with no package id resolves by display name");
        Check(byName.ResolveName("old display name", Loaded("successor.id"), null).Status == ResolutionStatus.NoRule, "names are compared exactly, as PatchOperationFindMod does");

        // A rule known only by name and a rule known by package id that describe the same pair are one alias, whichever arrives first.
        var twinA = new AliasCatalog();
        twinA.Add(Rule("", "nps.new", Compat.Yes, RuleSource.Embedded, "Nature's Pretty Sweet"));
        twinA.Add(Rule("nps.old", "nps.new", Compat.Unknown, RuleSource.Snapshot, "Nature's Pretty Sweet"));
        var twinB = new AliasCatalog();
        twinB.Add(Rule("nps.old", "nps.new", Compat.Unknown, RuleSource.Snapshot, "Nature's Pretty Sweet"));
        twinB.Add(Rule("", "nps.new", Compat.Yes, RuleSource.Embedded, "Nature's Pretty Sweet"));
        foreach (AliasCatalog twin in new[] { twinA, twinB })
        {
            Check(twin.Count == 1 && twin.All.Single().OldPackageId == "nps.old" && twin.All.Single().Compat == Compat.Yes, "a name-only declaration and the id rule of the same pair merge into one vouched alias (either order)");
            Check(new AliasResolver(twin).ResolvePackageId("nps.old", Loaded("nps.new"), null).Target == "nps.new" && new AliasResolver(twin).ResolveName("Nature's Pretty Sweet", Loaded("nps.new"), null).Target == "nps.new", "the merged alias resolves by id and by name, both vouched");
            Check(twin.FromPackageId("nps.old").Count() == 1 && twin.FromName("Nature's Pretty Sweet").Count() == 1, "the indexes hold the merged rule once");
        }
        var notTwins = new AliasCatalog();
        notTwins.Add(Rule("a.old", "a.new", Compat.Yes, RuleSource.Embedded, "Same Name"));
        notTwins.Add(Rule("b.old", "b.new", Compat.Yes, RuleSource.Embedded, "Same Name"));
        Check(notTwins.Count == 2 && notTwins.FromName("Same Name").Count() == 2, "two rules with the same old name but different successors stay two aliases");
        Check(notTwins.FromPackageId("A.Old").Count() == 1 && !notTwins.FromPackageId("missing").Any() && !notTwins.FromName("nope").Any(), "index lookups are case-insensitive for ids and empty for unknowns");
        Check(notTwins.Rules.Select(r => r.Id).SequenceEqual(new[] { "a.old>a.new", "b.old>b.new" }), "Rules lists aliases sorted by identity, All in insertion order");

        // The embedded file.
        const string good = "<ResolveThisInstead><alias><old><packageId>Adaptive.Storage.Neolithic</packageId><name>Adaptive Storage Neolithic Module</name></old>"
            + "<successor><packageId>nelim.adaptivestorageneolithic</packageId></successor><compatible>true</compatible><note>Same defNames.</note></alias></ResolveThisInstead>";
        EmbeddedFile.ParseResult parsed = EmbeddedFile.Parse(good);
        Check(parsed.Errors.Count == 0 && parsed.Rules.Count == 1 && parsed.Rules[0].OldPackageId == "adaptive.storage.neolithic"
            && parsed.Rules[0].Compat == Compat.Yes && parsed.Rules[0].Source == RuleSource.Embedded && parsed.Rules[0].Note == "Same defNames.", "a well-formed embedded file is read");
        Check(EmbeddedFile.Parse("<ResolveThisInstead><alias><old><name>x</name></old></alias></ResolveThisInstead>").Errors.Count == 1, "an entry with no successor is reported");
        Check(EmbeddedFile.Parse("<ResolveThisInstead><alias><old><packageId>a</packageId></old><successor><packageId>b</packageId></successor><compatible>maybe</compatible></alias></ResolveThisInstead>").Errors.Count == 1, "a bad compatible value is reported");
        Check(EmbeddedFile.Parse("<oops/>").Errors.Count == 1 && EmbeddedFile.Parse("<broken").Errors.Count == 1, "a wrong root or broken XML is reported, not thrown");
        Check(EmbeddedFile.Parse("<ResolveThisInstead><alias><old><packageId>a</packageId></old><successor><packageId>b</packageId></successor></alias></ResolveThisInstead>").Rules[0].Compat == Compat.Unknown, "compatible defaults to unknown");
        Check(EmbeddedFile.Parse("<ResolveThisInstead><alias><old><packageId>a</packageId></old><successor><packageId>a</packageId></successor></alias><alias><old><packageId>c</packageId></old><successor><packageId>d</packageId></successor></alias></ResolveThisInstead>").Rules.Count == 1, "a bad entry does not stop the good ones");
        Check(parsed.Rules[0].RuntimeOptIn == false, "runtime defaults to false when the tag is absent");
        Check(EmbeddedFile.Parse("<ResolveThisInstead><alias><old><packageId>a</packageId></old><successor><packageId>b</packageId></successor><runtime>true</runtime></alias></ResolveThisInstead>").Rules[0].RuntimeOptIn, "runtime true is read");
        Check(!EmbeddedFile.Parse("<ResolveThisInstead><alias><old><packageId>a</packageId></old><successor><packageId>b</packageId></successor><runtime>false</runtime></alias></ResolveThisInstead>").Rules[0].RuntimeOptIn, "runtime false is read");
        Check(EmbeddedFile.Parse("<ResolveThisInstead><alias><old><packageId>a</packageId></old><successor><packageId>b</packageId></successor><runtime>maybe</runtime></alias></ResolveThisInstead>").Errors.Count == 1, "a bad runtime value is reported");

        UtiReader();
        RealData(args);

        Console.WriteLine();
        Console.WriteLine(failures == 0 ? "ALL " + checks + " CHECKS PASSED" : failures + " of " + checks + " CHECKS FAILED");
        return failures == 0 ? 0 : 1;
    }


    // Optional: the real rules of Use This Instead (path in RTI_UTI_JSON, or as the first argument). The file is not part of this
    // repository: a snapshot is taken on purpose, with the MIT notice.
    private static void RealData(string[] args)
    {
        string path = args.Length > 0 ? args[0] : Environment.GetEnvironmentVariable("RTI_UTI_JSON");
        if (string.IsNullOrEmpty(path) || !File.Exists(path))
        {
            Console.WriteLine("SKIP real data (set RTI_UTI_JSON to a replacements.json); path was: [" + path + "]");
            return;
        }
        UtiData.ParseResult data = UtiData.Parse(File.ReadAllText(path));
        var catalog = new AliasCatalog();
        catalog.AddRange(data.Rules);
        int rules = data.RulesRead;
        // Proposals from the data are off by default (nobody vouched for them); the checks below are about reachability, so the player switches every alias on.
        var allOn = new PlayerChoices();
        foreach (AliasRule r in catalog.Rules) allOn.Enabled[r.Id] = true;
        var resolver = new AliasResolver(catalog);
        Console.WriteLine("real data: " + rules + " rules read (" + data.RulesSkipped + " skipped), " + catalog.Rules.Count() + " distinct aliases kept, version " + data.Version);
        Check(data.Errors.Count == 0 && data.RulesSkipped == 0 && data.Version != null, "the real file reads with no error, no skipped rule and a version stamp");
        Check(catalog.Rules.Count() > 1500, "the real data loads into the catalog");
        Check(resolver.ResolvePackageId("tkkntkkn.nps", Loaded("mlie.naturesprettysweet"), null).Status == ResolutionStatus.Disabled, "real data alone switches nothing on: a proposed pair stays off");

        // Every rule resolves without looping (the seven cycles included) when its successor is the only loaded mod.
        bool allFinish = true;
        var unresolved = new List<string>();
        foreach (AliasRule rule in catalog.Rules)
        {
            Resolution res = rule.OldPackageId.Length > 0
                ? resolver.ResolvePackageId(rule.OldPackageId, Loaded(rule.NewPackageId), allOn)
                : resolver.ResolveName(rule.OldName, Loaded(rule.NewPackageId), allOn);
            if (res.Status != ResolutionStatus.Resolved && res.Status != ResolutionStatus.Ambiguous)
            {
                allFinish = false;
                if (unresolved.Count < 8) unresolved.Add(res.Status + " | " + rule.Id + " (" + rule.OldName + " -> " + rule.NewName + ")");
            }
        }
        foreach (string u in unresolved) Console.WriteLine("  unresolved: " + u);
        Check(allFinish, "every real rule resolves to its successor when that successor is loaded");
        if (catalog.Rules.Any(r => r.OldPackageId == "tkkntkkn.nps" || r.OldName == "tkkntkkn.nps")) Check(resolver.ResolvePackageId("tkkntkkn.nps", Loaded("mlie.naturesprettysweet"), allOn).Target == "mlie.naturesprettysweet", "real case: Nature's Pretty Sweet resolves to its Continued page"); else Console.WriteLine("SKIP real case: Nature's Pretty Sweet resolves to its Continued page (rule not in this version of the data)");
        if (catalog.Rules.Any(r => r.OldPackageId == "nature's pretty sweet" || r.OldName == "Nature's Pretty Sweet")) Check(resolver.ResolveName("Nature's Pretty Sweet", Loaded("mlie.naturesprettysweet"), allOn).Target == "mlie.naturesprettysweet", "real case: the failing name guard of Alpha Mythology Renew resolves by name"); else Console.WriteLine("SKIP real case: the failing name guard of Alpha Mythology Renew resolves by name (rule not in this version of the data)");
        if (catalog.Rules.Any(r => r.NewPackageId == "koth.realisticplanets2")) Check(resolver.ResolvePackageId("windowsxp.realisticplanets", Loaded("koth.realisticplanets2"), allOn).Target == "koth.realisticplanets2", "real case: the three-hop Realistic Planets chain resolves"); else Console.WriteLine("SKIP real case: the three-hop Realistic Planets chain resolves (rule not in this version of the data)");
        if (catalog.Rules.Any(r => r.NewPackageId == "koth.realisticplanets2")) Check(resolver.ResolvePackageId("windowsxp.realisticplanets", Loaded("zvq.realisticplanetscontinued", "koth.realisticplanets2"), allOn).Target == "koth.realisticplanets2", "real case: the newest of two loaded links wins"); else Console.WriteLine("SKIP real case: the newest of two loaded links wins (rule not in this version of the data)");
    }

}
