---
mod: Resolve This Instead
packageId: nelim.resolvethisinstead
repo: https://github.com/vbardales/Rimworld-Resolve-This-Instead
visibility: public
local_path: C:/Users/nelim/Documents/rimworld/ResolveThisInstead
git_isolation: standalone; own .git and origin, public; ignored (and never tracked) in the parent monorepo
workflow_stage: declareDependencies[1.0.0]
settings_audit: partial
localization: partial
translation_en: partial
translation_fr: unchecked
audit_at: 2026-10-01
audit_revision: d5223bc plus uncommitted (debug logging in Source/Mod, two new keys EN/FR, rebuilt DLL 79855596cc0e, FRENCH_REVIEW.md, docs)
automated_tests: engine 57/57 without data (dotnet run, 2026-10-01; 61/61 with data on 2026-09-26); static checks 18/18 (Tests/Check-Mod.ps1, 2026-10-01)
functional_tests: Pickle played green on 2026-09-27 (lib-v8, three passes, exit 0, DLL before the debug logging) and the restart pair green (lib-v6); not replayed on the current tree; French not played
licence: MIT (decided by the owner 2026-10-01; LICENSE added; data source Use This Instead is MIT too)
licence_at: 2026-10-01; no upstream licence conflict (emipa606/UseThisInstead MIT, MiniJSON MIT notice kept)
upstream_mod_remotes:
  - https://github.com/emipa606/UseThisInstead
publishedFileId: none (0.1.0 not pre-published; no About/PublishedFileId.txt)
evidence: Tests/Pickle/Evidence (disk only, 1 MB: lib-v8 x3 passes + lib-v6 restart pair); history in docs/runs/history.md
remaining:
  - unverified: the crash reported 2026-10-01 ("RTI plante"): cause not established; debug logging added to find it (Player.log needed)
  - unverified: Pickle not replayed on the tree with the debug logging; settings_audit reset to partial because an option (debug logs) was added
  - unverified: French in game; French review by Virginie (FRENCH_REVIEW.md generated, nothing reviewed)
  - unverified: the aliases do not leak once loading is over beyond what v8 scenarios assert; real Elves ambiguity (needs a download, owner's go)
  - owner only: telling Mlie when tests start; the Preview; the Workshop page before any consumer's 1.0.0; PRs to emipa606/UseThisInstead only with her agreement (none planned yet)
updated: 2026-11-02
protocols_read_sha: 0780ca047aea43f76e2cd7beeb16ae995344952d
---

## 2026-10-01 — Audit (AUDIT.md reapplied)

Retained after the follow-up of the same day: `stage: port`, `workflow_stage: horsMonoRepo` (the first pass of this audit retained `dansMonoRepo`) (was `dansMonoRepo` in the old free-form `stage`; the field now uses the six codes of AUDIT.md). First failing
transition: `dansMonoRepo -> horsMonoRepo`: CHANGELOG.md absent (licence MIT decided and LICENSE added later the same day). Met in that transition: standalone
git with public GitHub origin and a pushed first commit (`420d9ee`, plus `a601223`, `d5223bc`), STATUS.md, packageId/folder/repo naming, README.md and ATTRIBUTION.md, `upstream_mod_remotes` (now set).
Later-stage facts kept as independent validations, not as a state: ModIcon delivered by the owner (provenance confirmed 2026-09-27), 128x128 / 20,792 bytes; no Preview yet; Pickle green on 2026-09-27.
Checks done today: `dotnet run` unit tests 57/57; `Tests/Check-Mod.ps1` 18/18; `dotnet build -c Release` 0 warnings 0 errors. Nothing launched in a game by this session.

Changes today:
- Debug and error logging (`Source/Mod/Runtime.cs`, `RtiMod.cs`) because RTI is reported to crash: `Runtime.Info/Debug/DebugOnce/Failure`, a "Debug logs" checkbox (EN+FR keys `RTI_DebugLogs`, `RTI_DebugLogsTip`,
  saved as `debugLogs`, default on while stabilising; RimWorld's verbose logging also enables it), every Harmony postfix/prefix and the constructor wrapped in try/catch (logged as `FAILED in <site>` with the stack, first 3 per site, game not broken).
  Not run in a game: this is diagnostic code, not a fix. Flip `debugLogs` default to false before publishing.
- Evidence trimmed on disk (68 MB to 1 MB): deleted lib-v6 three passes (red, superseded by green v8) and every `report.html`; kept lib-v8 x3 and the v6 restart pair (sole proof of persistence across a restart). No evidence was tracked in git, no `.dds` is tracked (`.gitignore` already covers `*.dds`). No archive of this mod in `pickle-reports-archive`.
- `docs/runs/history.md` created (one line per run), evidence rule written in `Tests/Pickle/README.md`, `docs/PROTOCOLS-READ.md` rewritten with versions read and useless documents.
- `FRENCH_REVIEW.md` + `Tests/Make-FrenchReview.js` (TRANSLATIONS.md). No pawn-agreeing text, so no gender switch.
- Upstream: the library is original; the only source repository is Use This Instead (`emipa606/UseThisInstead`, MIT), whose data and MiniJSON it reuses. No fix to send back so far.
- No `About/PublishedFileId.txt` found: 0.1.0 not pre-published, so no CHANGELOG entry is written.
- Follow-up, same day (owner: "MIT", then "fais comme les autres mods et lis la doc"): `LICENSE` MIT (Copyright (c) 2026 Nelim, as ArchitectStudio) at the root and in `Mod/` (PUBLISHING.md: the notice must travel with the distribution), `Mod/ATTRIBUTION.md` copy (identical to the root one, compared with `cmp`), `CHANGELOG.md` with `## [Unreleased]` only, `.gitattributes` and `.editorconfig` copied from ArchitectStudio. Gate dansMonoRepo -> horsMonoRepo now met; next gate (horsMonoRepo -> ModIcon) needs finished development (crash open) and the build. PUBLISHING.md read in full up to line 658 (CI, tests and tooling traps below not needed yet). Not done, owner decision: GitHub topics (`gh repo edit ... --add-topic rimworld --add-topic rimworld-mod --add-topic mod`). Later gates not yet met: description lacks AI-generated / THANKS / `Source code on GitHub` lines, no Preview, no PUBLICATION.md.


Superseded (kept, see the sections below; the library, tests and repository exist now). Original note: the README holds the motivation, data findings and open questions. No code, no Mod/
folder, no About.xml, no ModIcon or Preview. First consumer: AlphaMythologyRenew (frozen, not edited from here).
Upstream of the data: `emipa606/UseThisInstead` (public git, MIT); any fix worth returning goes as a PR there,
only with Virginie's agreement. Not affiliated with Mlie.

Session title: `resolvethisinstead / horsMonoRepo`.

## 2026-09-26

Hygiene set up: `.gitignore` (dds, evidence), `docs/PROTOCOLS-READ.md`. No CHANGELOG yet: there is nothing to
release, and a `0.1.0` "creation of a publishIdFile" entry is only written once an item exists.

## Message from the AlphaMythologyRenew session — 2026-09-26

Relayed decision (not yet confirmed to me by Virginie in this session): AlphaMythologyRenew will REQUIRE this library
(`modDependencies`), instead of fixing its 8 name-guarded patches one by one. This settles design question 3
(dependency, not embedded) in the direction I recommended; it stays provisional until Virginie says so here.
Consequences recorded: (1) a stable packageId is needed early: `nelim.resolvethisinstead` proposed; the About.xml of
the library must match the `path:ResolveThisInstead/...` line the pass maps will carry; (2) a public Workshop page is
needed before that mod's 1.0.0 (its 0.1.0 pre-publication may go first, private); (3) first use: the 8 `FindModAny`-style
guards of `Mod/Patches/AlphaMythology/*.xml`, starting with Nature's Pretty Sweet. Questions 1, 2 and 4 stay open.

## Design en cours — 2026-09-26 (Virginie's clarifications, not decisions on the open questions)

- Idea (Virginie): like Use This Instead, but acting: at game launch, identify deprecated external packageIds and let
  the mods that target them follow the maintained replacement, in packageId references and in XML patch guards, instead of
  every patch author chasing successors by hand.
- Recommended shape (mine, to be confirmed): alias resolved at query time ("is mod X active?" also true when a validated
  successor is loaded), not a rewrite of other mods' XML. Reversible; also covers name-based `PatchOperationFindMod`.
- A settings menu lists every alias with a checkbox (origin, successor(s), installed or not); the player can switch one off
  or choose between two successors (two "Continued" Elves pages exist). Real settings: MOD_SETTINGS.md applies
  (Mod options -> Resolve This Instead; MainButtons shortcut hidden by default).
- An alias is active only if its successor is really loaded. Chains A -> B -> C are followed to the last loaded link, with a
  loop guard; rule when several links are loaded: the most recent, unless the player unticked it (to confirm).
- Open: global alias or only for mods that declare the library as a dependency (I recommend dependants only at first).
- To verify, read-only, in the game's code (nothing launched): when `LoadFolders`, `MayRequire`, `PatchOperationFindMod`
  and mod settings are read, relative to when a Harmony patch can be in place; the loading-screen "press Enter for more
  options" pattern is not known to me yet.

## Feasibility read — 2026-09-26 (game code decompiled with ilspycmd, read only, nothing launched)

Source: `Assembly-CSharp.dll` of the installed RimWorld (Verse.LoadedModManager, ModContentPack, ModLister, ModsConfig,
LoadFolder, PatchOperationFindMod, DirectXmlToObject). Decompiled copies are in the session scratchpad, not in the repo.
This is code reading: nothing below has been observed in game.

Order of a launch (`LoadedModManager.LoadAllActiveMods`): `InitializeMods` (creates every ModContentPack; its constructor
runs `InitLoadFolders`, which evaluates `LoadFolders.xml` conditions) -> `LoadModContent` (loads every mod's assemblies) ->
`CreateModClasses` (instantiates every `Verse.Mod` subclass: our constructor and our settings can run here) -> `LoadModXML`
-> `ApplyPatches` -> `ParseAndProcessXML`.

Consequences:
- Patch guards: `PatchOperationFindMod` calls `ModLister.HasActiveModWithName` (exact name equality) inside `ApplyPatches`,
  after our `Mod` constructor. An alias there is feasible.
- `MayRequire` / `MayRequireAnyOf` (Defs and fields): resolved in `ParseAndProcessXML` and `DirectXmlToObject` through
  `ModLister.AllModsActiveNoSuffix` / `AnyModActiveNoSuffix`, which go through `GetActiveModWithIdentifier(id, ignorePostfix)`.
  After our constructor too, so feasible; the natural choke point is `GetActiveModWithIdentifier`.
- `LoadFolders.xml` `IfModActive`: **NOT reachable by a mod DLL on a normal launch.** `LoadFolder.ShouldLoad` uses the same
  `ModLister` calls, but it runs in the `ModContentPack` constructor, before any mod assembly is loaded. Possible partial
  workaround, not verified: recompute `foldersToLoadDescendingOrder` of the running mods from our constructor (reflection
  on the private `InitLoadFolders`): that would fix Defs, Patches and textures of a conditional folder, but not assemblies
  of that folder (already loaded). An alias for `LoadFolders` is therefore at best partial; say so in the description.
- Risk: `ModsConfig.IsActive(string)` is a one-line method (may be inlined, so a Harmony patch may not reach every caller),
  and mods' own C# calling it are out of reach. `GetActiveModWithIdentifier` and `HasActiveModWithName` are loops.
- The mod-list screen (missing dependency, load-order warnings) runs before any of our code: not covered.
- Settings: `GetSettings` in our constructor is read before `ApplyPatches`, so checkboxes can take effect.
- Not checked: how other mods show "press Enter for more options" at loading; Harmony availability at that point (Harmony is
  a hard dependency of most mods and loads first).

## Forcing IsActive — 2026-09-26 (Virginie's idea; code read only)

`ModsConfig.IsActive(string)` only reads the private static `activeModsHashSet`, which is separate from `data.activeMods`
(load order, ModsConfig.xml). Adding an old packageId to that set from our `Mod` constructor would make `IsActive(old)`
true for every caller (other mods' C# included), with no Harmony patch on a possibly inlined one-liner and no change to load
order or saves. `RecacheActiveMods` clears and refills the set: re-add the aliases after it (Harmony postfix on that private
static). Separate mechanism from the `MayRequire`/guard route (`ModLister.GetActiveModWithIdentifier`); `LoadFolders` is
still not covered (evaluated earlier, reads `modsByPackageId`).
Scope: global by nature (no caller identity), so "dependants only" holds for the guard/MayRequire route only.
Risk: a mod that tests `IsActive(old)` then uses the old mod's types fails if the successor changed them. Mitigation:
validated drop-in pairs only, one checkbox per alias (risky ones unticked by default), a log line per forced alias.
Pending Virginie's answer.

## Embedded compatibility file + POC — 2026-09-26

Virginie's addition: each consuming mod may embed a file stating whether a successor is backward compatible with the
old mod, which pre-ticks (or leaves unticked) that alias's checkbox in the menu. My reading: the consuming mod is the one
that knows its own code, so its declaration outranks the library's default; the player's checkbox outranks both; the
snapshot from Use This Instead only proposes pairs. Format and file location to design (not yet): one file per mod,
listing `old packageId (and names) -> successor packageId, compatible: yes/no/unknown`.

`Poc/` (this folder, not a repository): a throwaway mod that tests the theory on a pair we own (Virginie, 2026-09-26): Adaptive Storage Neolithic Module `adaptive.storage.neolithic` (original, Workshop 3033901895, installed, must be disabled for the test) -> our Adaptive Storage Neolithic Renew `nelim.adaptivestorageneolithic` (3806101377, installed). Rebuilt on that pair; the Nature's Pretty Sweet case stays the motivating one for Alpha Mythology Renew. Built (Release, 0
warnings, 0 errors), **not run**: nothing launched by this session. See `Poc/README.md` for what it asserts and how Virginie
runs it; its result (Player.log `[RTI-POC]` lines) is what confirms or refutes the feasibility read above.

## Workshop search: existing packageId guards — 2026-09-26 (scripts/Search-Workshop.sh, default mode, run in the foreground, one after the other)

Search 1 (`-i '*.xml'`, name `rti-patchop-xml`): pattern `Class="…(FindMod|ModActive|ModLoaded|HasMod|IfMod|Alias)…"`; 8987 of 8990 folders read (the
other 3 hold no XML); 29,473 lines. Results under `%LOCALAPPDATA%\Temp\rimworld-search\rti-patchop-xml\hits.txt`.
Besides vanilla `PatchOperationFindMod` (28,525 lines, compares display names), custom guards by packageId already exist:
`XmlExtensions.FindMod` (226; XML Extensions, `Imranfish.XMLExtensions`), `ModCheck.isModLoaded` (153; ModCheck), `JPTools.PatchOperationFindModById`
(113; embedded in the JPT mods), `PatchOperation.FindModByID` (58; CCL PatchOperation Highlander, `ccl.patchoperation.highlander`),
`ModFinder.PatchOperationFindMod` (41), `sd_medicaddons.PatchOperationFindMod` (225), `PatchOperationFindModByPackageId` (2), plus a few one-off
`…ModLoaded`/`…IfModPresent`. None of them was read: whether any is alias-aware is UNVERIFIED (nothing in their names says so).
Search 2 (`-i '*.dll'`, `rti-patchop-dll`): pattern on `PatchOperation…PackageId|ModsConfig.IsActive|AnyModActiveNoSuffix`; 4590 of 8990 folders hold a DLL;
147 lines, 65 mods. 57 call `ModLister.AnyModActiveNoSuffix` (so our `GetActiveModWithIdentifier` route reaches them), 5 reference `ModsConfig.IsActive`
(the hashset route), no DLL defines a packageId `PatchOperation` matching the pattern. Limits (SEARCHING.md): string literals in UTF-16 were not searched,
and an absence is not proof.
Consequence: no library resolves renamed or replaced mods for these guards; several already accept a packageId, so a consuming mod could keep them.
What decides whether our alias reaches them is which ModLister/ModsConfig call each one makes: to read by decompiling XmlExtensions, ModCheck and CCL.

## Existing guards, read — 2026-09-26 (decompiled, read only; not tried in game)

Which game calls each existing packageId guard makes, and so whether our alias reaches it:
- vanilla `PatchOperationFindMod` and CCL `PatchOperationFindAllMods`: `ModLister.HasActiveModWithName` -> reached.
- CCL `FindModByID` (Highlander, Workshop 2552002158): `ModsConfig.ActiveModsInLoadOrder.Any(mod => mod.SamePackageId(id, true))` ->
  not reached by the ModLister route; a postfix on `ModMetaData.SamePackageId` would reach it.
- `XmlExtensions.FindMod` (XML Extensions 2574315206, 1.6 DLL; also its `Boolean.FindMod`): scans `LoadedModManager.RunningMods` comparing
  `ModContentPack.PackageId` or `Name` (option `packageId`) -> not reached; needs a targeted patch of its own method.
- `ModCheck.isModLoaded` (ModCheck 1.0 DLL, the only installed copy; NWNL has no ModCheck DLL in 1.6): scans `ModsConfig.ActiveModsInLoadOrder`
  by `Name` or package id -> not reached.
- 57 mods calling `ModLister.AnyModActiveNoSuffix` and 5 referencing `ModsConfig.IsActive` -> reached.
None of these guards knows replacements: they compare literal names or ids. The library covers game-level lookups by itself; provider
modules (CCL, XML Extensions, ModCheck) would be optional add-ons loaded only when the provider is present. Decision deferred until the POC has run.

Correction (2026-09-26, after Virginie's question): `ModCheck.isModLoaded` compares the display NAME (`m.Name == name`, modName plus a hand-filled
`altModNames` list) over `ModsConfig.ActiveModsInLoadOrder`; it is not a packageId guard and does not go through `ModsConfig.IsActive`. The
earlier lines above ("by `Name` or package id", and ModCheck listed among the packageId guards in the search summary) are wrong for that class.
It is not reached by our alias; it already carries its own manual alias list, which is what the library would replace.

## Provider modules: rule agreed with Virginie — 2026-09-26

Conditional patches per provider (ModCheck, XML Extensions, CCL Highlander) are the plan, but only on real need and only if the provider's own
options do not already solve it (XML Extensions has a `packageId` option; ModCheck has hand-filled `altModNames`). Each is a small Harmony
module applied only when the provider is loaded, targeting its method by name (no hard reference). CCL is the cheapest: one postfix on the game's
`ModMetaData.SamePackageId`. Alpha Mythology Renew needs none (its 8 guards are vanilla `PatchOperationFindMod`). Nothing is written before the POC result.

## First steps agreed — 2026-09-26

Virginie: the first steps are to test the existing methods and see whether they meet the need. POC extended accordingly (built, not run): the
game-level doors (name guard, MayRequire, IsActive, AnyModActiveNoSuffix), then the providers' own guards on the same pair: XML Extensions on the old
id only (expected not applied), XML Extensions listing both ids (expected applied: the consumer alone solves it), CCL `FindModByID` on the old id
(expected applied through a postfix on `ModMetaData.SamePackageId`, which affects every caller of that method). ModCheck is not tested: only a 1.0 DLL is installed.
Test list: Harmony, Adaptive Storage Framework, Adaptive Storage Neolithic Renew (original disabled), XML Extensions, CCL Highlander, the POC.

## POC run requested — 2026-09-26

Pickle request `20260926-222716-133-fd04` filed with `Submit-PickleRun.ps1` (owner `local_e96de4d2-bc54-4029-af3f-79eabc3c7859`, registered with TicketDispatcher):
`-Mod ResolveThisInstead/Poc -DepMap wsl-deps.avec-fournisseurs.map -EvidenceDir ResolveThisInstead/Poc/Tests/Pickle/Evidence/poc-v1`, English, one pass.
Tested tree: `Poc/Mod/Assemblies/RtiPoc.dll` sha256 `12879dc137fd...`, built 2026-09-26 22:20; the tree is not a git repository, so the request carries no SHA:
**do not edit `Poc/` until `RUN_DONE`**. The C# lookups (IsActive, AnyModActiveNoSuffix, name, hashset) are read from `Player.log` (`[RTI-POC]` lines) in the
evidence folder; the suite asserts only the guarded defs. Nothing launched by this session; the worker plays it when the machine is free.

## packageId confirmed — 2026-09-26

Virginie confirmed the packageId `nelim.resolvethisinstead`. The future library's `About.xml` must carry exactly that id (Alpha Mythology Renew's pass maps will use
`nelim.resolvethisinstead path:ResolveThisInstead/Mod` until a Workshop id exists). No `Mod/` folder or `About.xml` exists yet; the POC keeps its own id `nelim.resolvethisinstead.poc`.

Commitment to the AlphaMythologyRenew session (its reply, 2026-09-26): it will declare the library in `About.xml`, in its Check-Mod list and in its pass maps in ONE commit,
only after I tell it that `ResolveThisInstead/Mod` exists, carries `nelim.resolvethisinstead` in its `About.xml` and loads (staging stops a whole queue on a hard dependency it cannot mount).
So: do not tell it before the folder is real; tell it once, when it is.

## Engine, data, README — 2026-09-26 (Virginie: options 1, 3, 4 go)

- **Engine** (`Source/Engine`, pure C#, no game dependency): `AliasModel` (rule, compat yes/no/unknown, source snapshot/embedded, id normalisation incl. stacked `_steam`/`_copy`),
  `AliasCatalog` (merge, the player > the consumer's declaration > a bare snapshot pair stays off), `AliasResolver` (breadth-first over enabled links with a visited set; most recent
  loaded link wins; ambiguity when several successors sit at the same distance; the player's pick settles it; by packageId and by exact display name), `EmbeddedFile`
  (`About/ResolveThisInstead.xml`, tolerant parser). Not yet hooked into the game (that is what the POC tests).
- **Tests**: `Tests/UnitTests` (`dotnet run -c Release [-- <replacements.json>]`, net8): 35 checks, 41 with the real rules; all passed on 2026-09-26. Found on the way and fixed:
  stacked postfix `_copy_copy` in the data.
- **Data**: `replacements.json.gz` downloaded to the session scratchpad (184,754 bytes, `emipa606/UseThisInstead` commit `4454f15`, MIT), not into this repository; analysed
  (numbers in README.md). Not in the data: our Adaptive Storage Neolithic pair.
- **README.md** rewritten (was partly out of date): idea, mechanism table with what the alias does and does not reach, embedded file format, data findings, open questions.
- Still true: nothing hooked in a running game, POC request `20260926-222716-133-fd04` not played, no `Mod/`, no repository, nothing published.
- Not done from the list: option 2 (control pass with the alias off, provider patches behind `LoadFolders`, then refile the ticket): waiting for the first result.

## Where Use This Instead gets its data — 2026-09-26 (source read: `emipa606/UseThisInstead` `Source/UseThisInstead/UseThisInstead.cs`; installed copy 3396308787 listed)

- At startup (a `[StaticConstructorOnStartup]`, on a thread pool thread, after every `Verse.Mod` constructor has run) UTI downloads `https://data.litet.net/replacements.json.gz`
  (Mlie's server), falling back to `https://raw.githubusercontent.com/Mlie/UseThisInstead/main/replacements.json.gz`, with an `If-None-Match` ETag it keeps in its own settings
  (`LastVersion`, `LastAlternateVersion`; 304 = up to date). It decompresses and writes **`<UTI mod root>/replacements.json`** (`Content.RootDir`), i.e. inside its Workshop folder.
- Installed here: `.../workshop/content/294100/3396308787/replacements.json`, 1,152,589 bytes, version stamp **2026-02-09T07:43:38Z**: seven months older than the repository's
  2026-09-26T18:35:17Z, so UTI's own copy can be stale on a machine (not refreshed, or offline).
- Consequence for us (to avoid a duplicate downloader on Mlie's server): the library does NOT download anything. It reads UTI's `replacements.json` when UTI is loaded (find the mod by
  packageId, read `RootDir/replacements.json`; UTI's write happens after our constructor, so no race, one session of lag), and otherwise falls back to a snapshot shipped with the library.
  When both exist, the higher `version` stamp wins. Reading UTI's file is preferable to reflection into its private `modReplacements` (keyed by Workshop id only, pruned).
- Needed: a JSON reader for that file (the game has no ready one that I have checked; UTI ships its own `Json.cs`, MIT, to reuse with the notice), or convert the snapshot to XML at build time and use the reader only for UTI's file.
- Open: UTI's packageId to look up (read from its About.xml: see output above), and whether reading another mod's folder needs the player's consent line in the description.

## Use This Instead as a dependency of this library — 2026-09-26 (Virginie's proposal; my recommendation, awaiting her confirmation)

Recommended: hard dependency on `Mlie.UseThisInstead` (Workshop 3396308787; it requires Harmony, already common), and NO snapshot shipped. Removes open questions 1 and 4 (generation and refresh of a
snapshot), adds no downloader, and credits Mlie naturally. Costs: one more mod for players, network use by UTI at startup, coupling to a file location and format that are not a public interface,
and a possibly stale file (2026-02-09 here). Safeguards: (1) missing or unreadable file = log it and continue with the consuming mods' embedded files only (they remain what vouches for an alias; UTI's
pairs are proposals, off by default); (2) show the data's `version` stamp in the menu. Owner-only: telling Mlie (public, outward-facing). Pickle passes that read UTI's data must stage
3396308787; whether UTI disturbs a headless run (it downloads at startup) is unknown. Alternative if she prefers: keep a shipped snapshot as a fallback (then questions 1 and 4 stay open).

## Decision — 2026-09-26: hard dependency on Use This Instead, no snapshot (Virginie: "dep dure")

The recommendation above is adopted. Consequences to carry:
- The library's `About.xml` will declare `Mlie.UseThisInstead` (steam://url/CommunityFilePage/3396308787) as `modDependencies`, and `loadAfter` it.
- Staging is not recursive: a consuming mod's Pickle pass map must list UTI (`Mlie.UseThisInstead 3396308787`) BEFORE `nelim.resolvethisinstead`, and this library's own suite needs it in its `wsl-ids.map`. Tell the AlphaMythologyRenew session when `ResolveThisInstead/Mod` exists (with the UTI line).
- No snapshot generator, no refresh script: questions 1 and 4 are closed. Open: JSON reader (reuse UTI's MIT `Json.cs` with its notice, or write one), UTI's effect on a headless Pickle run, telling Mlie (owner only).

## JSON reader — 2026-09-26 (Virginie: "go lecteur JSON, celui d'UTI s'il est accessible")

- Reused UTI's MiniJSON (`Source/ThirdParty/MiniJSON.cs`, MIT, Calvin Rien 2013, copied from UTI's `Json.cs`), header kept, namespace `ResolveThisInstead.ThirdParty`, class made `internal`. `ATTRIBUTION.md` created.
- `Source/Engine/UtiData.cs`: `Parse` (tolerant: BOM, numeric package ids, null authors, bad rules skipped and counted, never throws), `Locate` (finds `Mlie.UseThisInstead`'s `replacements.json` among the running mods' root dirs).
  Pairs read from the data are `Compat.Unknown` (proposals, off by default). Found while testing: MiniJSON throws (`OverflowException`) on a truncated file, contrary to its header: the call is wrapped.
- Tests: 44 checks without data; 52 with the current file (2,710 rules, 0 skipped, 2,298 aliases); 50 with the stale copy installed here (2,253 rules, version 2026-02-09, 1,934 aliases: 364 fewer than the current
  file, and the Realistic Planets three-hop chain does not exist in it yet, so those two real-case checks are skipped, not failed). Not in a running game.

Telling Mlie: Virginie will do it herself when the tests start (2026-09-26). Not done by a session; nothing has been sent or posted to him. The queued POC (`20260926-222716-133-fd04`) does not read UTI's data or use its mod; the first run that stages UTI is the first of the library's own suite.

## The library mod exists — 2026-09-26 (Virginie: "fais-le" on touching Mod/Source/Tests while the ticket waits)

- `Mod/` (packageId `nelim.resolvethisinstead`, hard deps Harmony and Use This Instead, hidden `RTI_Settings` MainButton, EN and FR Keyed, French DefInjected) and `Source/Mod` (`Runtime.cs`,
  `RtiMod.cs`) on top of `Source/Engine`. Built: Release, 0 warnings, 0 errors; DLL sha256 `d8d6351600bb...`, 31,744 bytes. **Never loaded in a game.**
- What it does at startup (Mod constructor): reads `About/ResolveThisInstead.xml` of every running mod, reads Use This Instead's `replacements.json` (found by packageId), builds the catalog and
  resolver, patches `ModLister.GetActiveModWithIdentifier` (MayRequire, AnyModActiveNoSuffix), `ModLister.HasActiveModWithName` (PatchOperationFindMod) and `ModsConfig.RecacheActiveMods`
  (re-injects old ids into the private set `IsActive` reads). Settings window (Mod options -> Resolve This Instead): data status, problems, restore defaults, one checkbox per replacement that
  applies in this game (old mod absent, successor loaded), a chooser when several successors are loaded. Changes apply at the next start. Not done: the `SamePackageId` postfix for CCL (the POC decides).
- Static checks: `Tests/Check-Mod.ps1` (Windows PowerShell 5.1 or 7): 18 of 18 passed (XML well-formed, About identity and dependencies, hidden shortcut and worker class, EN and FR keys identical and all used, French DefInjected).
  Unit tests of the engine: unchanged (44/52).
- Pickle suite `Tests/Pickle` (companion `nelim.resolvethisinstead.pickletests`, which also embeds an alias file as a consuming mod): features 01 loading, 02 declared alias, 03 proposed pair stays off;
  `wsl-ids.map` (Use This Instead 3396308787) and `wsl-deps.avec-neolithic-nps.map`. Two requests filed, not played: `20260926-231647-943-d21e` (sans facultatifs) and `20260926-231648-509-11db`
  (avec neolithic et nps). Do not edit `Mod/`, `Source/` or `Tests/Pickle` until both are `RUN_DONE`. The POC request `20260926-222716-133-fd04` is ahead of them.
- Watch item: the postfix on `GetActiveModWithIdentifier` also feeds the game's mod-list screen (dependency and load-order checks), where an alias could hide a "missing dependency" warning. Unobserved.
- Still not told to the AlphaMythologyRenew session: it waits for a folder that loads. Tell it once, after the first green run, with the Use This Instead line for its pass maps.
- `settings_audit`: partial (window, persistence, RIMMSQOL route not yet played); translations `unchecked` for the game (keys and French DefInjected written, static check green).

## Code review fixes — 2026-09-26 (Virginie: "fix tout")

Nine review findings, all addressed (ReportFindings re-reported with outcomes):
- Aliases now answer only while the game loads data: `Runtime.Loading` gates the `GetActiveModWithIdentifier` and `HasActiveModWithName` postfixes and the `IsActive` injection; a prefix on
  `StaticConstructorOnStartupUtility.CallAll` (called after `ApplyPatches`, Defs and both cross-reference passes, read in the decompiled `PlayDataLoader`) calls `Runtime.EndLoading`, which takes the injected ids back out.
  This fixes the two confirmed defects: the Renew flagged incompatible with itself in the mod list, and an installed original looking active. **README/STATUS earlier said the IsActive route changed nothing listed: wrong, corrected.**
- The settings window edits a pending copy only (read by the next launch); lists are computed once; `AliasCatalog` indexes rules by old id and old name (`All` cheap, `Rules` sorted); a name-only rule and an id rule of the same pair merge.
- Tests: unit 61/61 with the real data, 53/53 without. New local Pickle steps (`Tests/Pickle/Source/Steps.cs`, compiled, DLL `e62a0a10b2c2...`) assert after loading that IsActive, GetActiveModWithIdentifier
  (both forms), the load order and HasActiveModWithName do not treat the original as loaded, that the Renew's incompatibleWith names nothing, and that `Runtime.Loading` is false. **Not played.**
- Library DLL `92b48d9984a6...`. Requests cancelled and refiled on this code: `20260926-232614-246-6ec4` (sans facultatifs), `20260926-232615-254-d8ed` (avec neolithic et nps), behind the POC `20260926-222716-133-fd04`.
  The POC still tests the OLD mechanism (global injection, no end of phase): its result validates the hooks, not the side effects.
- A residual doubt, unverified: Harmony's postfix on `StaticConstructorOnStartupUtility.CallAll` is installed from a Mod constructor, which runs before that method is first called: it should hold, but only a run shows it (the new step "the load is over" checks it).
- Do not edit `Mod/`, `Source/` or `Tests/Pickle` until both requests are `RUN_DONE`.

## Options and the "C asks for A, A -> B" case — 2026-09-26 (Virginie's question)

Answer to "did you look at the options": not until asked. Read in the game's code: the "Mod options" list is `LoadedModManager.ModHandles`, the `Mod` classes actually instantiated from loaded assemblies, so an absent mod has no entry
and the alias cannot create one. The real hole was different: the alias became active in OUR constructor, so the constructors of mods created after it (`CreateModClasses`) saw A as loaded and could go on to use A's settings.
Fix: the phase now starts at `LoadedModManager.LoadModXML` (prefix, `Runtime.BeginLoading`) and ends at `StaticConstructorOnStartupUtility.CallAll` (`Runtime.EndLoading`); nothing answers in between the two.
Residual, by design: C# that runs INSIDE that window (a Def's `ResolveReferences`, a custom PatchOperation that calls `IsActive`) still sees A as loaded; only the XML doors are fully safe. Hot reload (dev mode) calls LoadModXML again and does not reach CallAll: the alias stays on there.
The chain test: the companion plays C (`Tests/Pickle/Source/Consumer`, `ConsumerMod`, a real Mod constructor that asks IsActive, GetActiveModWithIdentifier and the Mod handles for A), with A absent and B (our Renew) loaded, plus C's XML guards on A:
new steps assert that the constructor saw A as absent and that the options list has no entry for A; the existing scenarios show C's guarded defs and patches apply through B. Built, not played. Library DLL `e6e803d3c399...`, steps `4cb55a101a10...`, consumer `73c8eb834001...`.

## Open question — C's settings while A is absent and replaced by B (Virginie, 2026-09-26)

Today the alias is a load-time answer, not a pointer: XML guards of C on A (vanilla FindMod, MayRequire) see B while data loads; after loading, C's C# (a settings window testing IsActive(A), Harmony patches on A's types) sees A absent, so
A-related options may be hidden or greyed while C's XML patches were applied (half-consistent, no crash from us); C's saved values are untouched; A's settings are not routed to B's. XML Extensions `FindMod`/`OptionalPatch`
guards are not reached at all (provider module needed): those options never appear. Candidate, undecided and unwritten: an opt-in runtime alias declared in the consumer's file (`<runtime>true</runtime>`), answered only to
the declared consumer assemblies (caller inspection), never to the mod manager; risks: dependence on the call stack, and `IsActive` may be inlined so a patch on it may not intercept. Waiting for Virginie's choice: reflect reality (current) or add it.

## POC request abandoned, not refiled — 2026-09-27

The queued POC request (`20260926-222716-133-fd04`) was abandoned overnight: its staging worker (pid 8296) died without releasing the lock, and TicketDispatcher does not
auto-replay a request already staged by a dead worker. A new worker (pid 20716) is processing the queue since this morning. Virginie confirmed (2026-09-27) not to refile it:
the mechanism it tested (global alias, no load-phase boundary) was superseded by the corrected library the same evening. The two live requests are the library's own suite,
`20260926-233551-213-c41f` (sans facultatifs) and `20260926-233552-743-c284` (avec neolithic et nps), still pending in the queue.

## More Pickle scenarios written while waiting — 2026-09-27 (Virginie: "on relancera juste les tickets")

Added, on the model of Alpha Mythology Renew's own suite (opens/sees/edits-same-instance, hidden-shortcut, RIMMSQOL pass, restart pair): `04-settings-window.feature`,
`05-shortcut.feature`, `06-rimmsqol.feature` (new `wsl-deps.avec-rimmsqol.map`), `07-restart-write.feature`/`08-restart-read.feature` (disables the declared Neolithic alias, restarts,
checks it stayed off and its guarded defs are gone). New steps `Tests/Pickle/Source/SettingsSteps.cs` (`SettingsSteps`, `ShortcutSteps`), same conventions as AMR's (direct settings
manipulation, not pixel clicks; `AfterScenario` teardown; a restart guard that refuses if the writer ran in the same process). The persistence check reads the DESERIALIZED
`Settings.enabled` dictionary rather than guessing Scribe's raw XML tag encoding for a `Dictionary<string, bool>` key containing `.` and `>`, which is not established here.
Built: engine 61/61 (with data), static Check-Mod 18/18. Library DLL unchanged (`e6e803d3c399...`); steps DLL `8f85fb52137d...`; consumer DLL unchanged (`73c8eb834001...`).
Requests refiled (cancelled c41f and c284, split into four so the restart pair does not run inside a plain full pass, matching Alpha Mythology Renew's own convention of excluding a
restart pair from an unfiltered run): `20260927-102207-385-2dfe` (sans facultatifs), `20260927-102209-183-3bfa` (avec RIMMSQOL), `20260927-102237-473-4982` (avec neolithic et nps,
full suite minus 07/08), `20260927-102210-372-a606` (avec neolithic et nps, `-Filter 07-restart-write -Then 08-restart-read` only). Nothing played.

## Generic runtime mechanism, corrected after finding a real flaw while testing it — 2026-09-27 (Virginie: "on peut tester ça ?")

First attempt (any mod caller gets the alias post-load, only the engine doesn't) was WRONG, found while writing the very test meant to confirm it:
this suite's OWN steps (`ResolveThisInstead.PickleSteps.dll`, a mod assembly) call `IsActive`/`GetActiveModWithIdentifier` directly to check that
loading has really ended (`NotTreatedAsLoaded`, `IncompatibleWithNamesNothing`); under "any mod = alias" those checks would have flipped from correct
to wrong, recreating the exact class of bug fixed the day before (an installed original looking active), just for a diagnostic/test caller instead of
the engine. Same risk for RIMMSQOL, ModCheck, or any compatibility-checking mod: they want the truth, not the convenience.

**Corrected design**: an explicit per-consumer opt-in, `<runtime>true</runtime>` on an embedded `<alias>` (`AliasRule.RuntimeOptIn`, parsed by
`EmbeddedFile`, never merged into a shared catalog rule since trust is per calling ASSEMBLY, not per pair). `Runtime.ReadEmbeddedFiles` adds a mod's
`assemblies.loadedAssemblies` to a `trustedForRuntime` set only when that mod's file asked for it. `Runtime.MayAnswer` is now: `Loading` (window 2,
unconditional) `|| (PastConstructionPhase && CallerIsTrustedForRuntime())` (window 3, opt-in only); `PastConstructionPhase` (set once, at the first
`BeginLoading`, never cleared) keeps window 1 (Mod construction) from ever answering even to a trusted caller. `CallerIsTrustedForRuntime` walks the
stack (`FindRealCaller`, skipping our own frames and Harmony's dynamic trampoline) and checks HashSet membership - no more `ClassifyCaller`/engine-vs-mod
enum, which turned out to be the wrong question entirely (see below).

**Simplification found along the way**: the old hashset-injection hack (`InjectIntoActiveSet`, reflecting into `ModsConfig.activeModsHashSet`) is
GONE, replaced by a direct Harmony postfix on `ModsConfig.IsActive(string)` itself, gated by the same `MayAnswer`. Simpler, no private-field reflection,
and removes yesterday's residual doubt about whether the `CallAll` prefix reliably strips injected ids (moot now, nothing is injected).

**Tested by**: a new, separately-staged third mod `Tests/Pickle/Consumer` (`nelim.resolvethisinstead.testconsumer`, own About.xml, own embedded file
WITH `<runtime>true</runtime>` on the Neolithic pair) versus the Pickle-tests companion's own embedded file (same pair, WITHOUT `<runtime>`). New
scenario in `02-aliases.feature`: the companion's own step still sees `adaptive.storage.neolithic` absent; the separately-staged consumer's own
runtime code (`ConsumerMod.CheckAfterLoad`, compiled into ITS OWN assembly) sees it present. Same rule, two callers, two different outcomes -
the property the whole design rests on, proven by construction rather than argued.

Engine build: 65/65 unit tests (incl. new `<runtime>` parsing checks); static Check-Mod 18/18. DLLs: library `8ce7d43df993...`, companion steps
`5a936330521c...`, test consumer `c1d58a69f7c6...` (new, separate mod folder `Tests/Pickle/Consumer/Mod`). Not played in a game.
Residual, unverified: whether the stack walk survives a Mono JIT that might inline a frame away (flagged in README).

Requests refiled on the v5 build (cancelled 2dfe, 3bfa, a606, 4982): `20260927-104657-941-8abc` (sans facultatifs), `20260927-104659-739-479f`
(avec RIMMSQOL), `20260927-104701-806-f478` (avec neolithic et nps, full minus restart pair), `20260927-104704-040-0c1d` (restart pair only).

## Ambiguity scenario added while waiting — 2026-09-27

The real ambiguity case (Lord of the Rims - Elves, two "Continued" pages) has only one successor (`zal.lotrelves`) installed on this machine; the
other (`LordOfTheRims.Elves.Continued`, 3383096916) is not, and downloading it needs Virginie's go. Used a SYNTHETIC pair instead, declared in the
companion's own embedded file: a fictitious old id `nelim.resolvethisinstead.test.ambiguous` with two successors that are ALWAYS staged (Harmony,
Use This Instead), so the ambiguity is available in every pass without downloading anything. New: `09-ambiguity.feature` (resolves to neither
successor on its own); extended the restart pair to also pick a successor in the write half and check it resolved after restart (a settings-window
pick, like the toggle, only takes effect at the NEXT launch: `Runtime.Choices` is fixed for a load). New steps in `SettingsSteps.cs` (`IsAmbiguous`,
`Pick`, `ResolvedToPickAfterRestart`). Steps DLL `b031e03db950...`. Static checks 18/18. Requests refiled: `20260927-105320-171-dcd7` (sans
facultatifs), `20260927-105321-292-263b` (avec RIMMSQOL), `20260927-105322-332-7a54` (avec neolithic et nps), `20260927-105323-363-b0c6` (restart pair).

## First real Pickle run: found and fixed — 2026-09-27

First runs played (v5/v6 builds): `dcd7`, `263b`, `7a54` exit 1; `b0c6` (restart pair) exit 0, **fully green** (both the on/off toggle and the
ambiguity pick survived a real restart, confirming the core "Choices fixed at load, read at next launch" design in a real game). Result across the
three red runs: **every alias-mechanism scenario passed** (18 of 20 in the neolithic pass: name guard, MayRequire, IsActive/GetActiveModWithIdentifier
no longer treating the original as loaded, the mod list's incompatibleWith fixed, the caller-discrimination test, the ambiguity test). Only the
shortcut-activation path failed, and RIMMSQOL's own steps failed too in the rimmsqol pass with an explicit message: `there is no main bar: no game is
running. Load the fixture ('the save "test-colony" is loaded') first`.

**Root cause, confirmed by the game/RIMMSQOL's own error text, not guessed**: `05-shortcut.feature` and `06-rimmsqol.feature` ran with no save
loaded (an assumption of mine, "Dialog_ModSettings works from main menu", was never actually verified and turned out incomplete: opening the dialog
directly works with no save - `04-settings-window.feature` proved that, passing every time - but the MAIN BAR / MainButtonWorker route needs a
running game). Alpha Mythology Renew's own equivalent suite always loads `test-colony` first; mine did not.

**Fixed**: added `Given the save "test-colony" is loaded` to both features' Background, matching AMR's proven pattern. Also wrapped
`Shortcut(ctx).Worker.Activate()` in a try/catch that rethrows with the real exception type and stack (the first run's failure message was a bare
"Object reference not set to an instance of an object" with zero attribution - diagnosed only by comparing against the passing `04-settings-window`
scenario and reading RIMMSQOL's own clearer error in a sibling run). Steps DLL `cbcbdd414063...`. Static checks 18/18.

Requests refiled on the v7 build (f478 and dcd7/263b superseded): `20260927-121052-951-e6b7` (sans facultatifs), `20260927-121054-178-6370`
(avec RIMMSQOL), `20260927-121055-185-53a2` (avec neolithic et nps). The restart pair (`b0c6`) was already fully green and is not replayed.

## AlphaMythologyRenew notified — 2026-09-27

Its own criterion (folder exists, carries the packageId, loads) is met and verified IN GAME, not just built: "the library loads after its dependencies
without startup errors" passed green in three real Pickle runs today, before the shortcut fix even. Sent it what to write in its one commit
(modDependencies, Check-Mod list, pass maps' `nelim.resolvethisinstead path:ResolveThisInstead/Mod` line, its own wsl-ids.map entry for
Mlie.UseThisInstead 3396308787 since staging is not recursive). Flagged as still open: no repository of its own yet (fine for `path:` staging), French
untested, and its own 8 name guards not yet proven through the library in a real game (only synthetic/other-mod pairs tested so far).

## MayRequire-ambiguous proof added while waiting — 2026-09-27

`RTI_TestMayRequireAmbiguous` (MayRequire on the synthetic ambiguous id) added to `RtiTestDefs.xml`; new scenario in `09-ambiguity.feature` asserting
it does not exist, proving the ambiguity result through the real XML door, not just `Runtime.TargetForId` directly. No code changed (XML/feature only).
Static checks 18/18. Requests refiled (e6b7/6370/53a2 cancelled before they ran): `20260927-173317-006-8b5a` (sans facultatifs),
`20260927-173318-369-0a20` (avec RIMMSQOL), `20260927-173319-769-4b89` (avec neolithic et nps).

## AlphaMythologyRenew wired the dependency — 2026-09-27

Its reply: commit `03d1a9d` adds the library to `About.xml` (modDependencies + loadAfter, no Workshop id yet so declared as a hard dependency, always
staged), its `Check-Mod.ps1` dependency list, and a new `Tests/Pickle/wsl-ids.map` resolving `nelim.resolvethisinstead path:ResolveThisInstead/Mod`
and `Mlie.UseThisInstead 3396308787`. Check-Mod green, 335 assertions. **Not done**: rewriting its 8 name guards to actually consume the library —
that's Virginie's call on shape (custom PatchOperation vs live lookup vs both, per its BACKLOG.md), still open on both sides. No joint Pickle pass
needed yet since nothing in AMR's patches calls into the library. French and a live check of the 8 guards wait on the same decision.

## Preview prompt written, ModIcon noted as still missing — 2026-09-27

`PROMPT_RESOLVETHISINSTEAD.md` written at the monorepo root (gitignored, `STYLE_RIMWORLD.md`'s single-block convention): a waystone niche, a
cracked old marker beside a freshly placed identical one, same silhouette, cold ambient vs warm lamp pool. No ModIcon exists yet
(`Mod/About/`, `Art/` do not exist): its generation is the owner's alone (`STYLE_RIMWORLD.md`, "ModIcon: contrôle, pas génération"), never a session's.
Queued for when she delivers it: convert `Art/ModIcon-source.png` (or the delivered PNG) to a `.ico` alongside it in `Art/`, a straightforward format
conversion once the source exists, not a generation step.

## ModIcon delivered, resized and .ico generated — 2026-09-27 (Virginie confirmed: resize yes, .ico from the resized file)

Delivered `Mod/About/ModIcon.png`: the winking mascot with a checkmark badge, "Resolve this instead" below. Full-resolution source (1254x1254,
1,212,318 bytes) preserved as `Art/Icons/ModIcon-source.png`. Control at 32px (before resizing): head and accompanying object (the checkmark) both
distinguishable, passes `STYLE_RIMWORLD.md`'s legibility test.
Resized to spec with Virginie's explicit go-ahead (ffmpeg, lanczos, only tool available - no ImageMagick/pngquant on this machine):
`Mod/About/ModIcon.png` now 128x128, 20,792 bytes, inside the 20-30 KB range. `Art/ModIcon.ico` generated from that resized file (her instruction):
single-size 128x128, 24bpp (no alpha in the source, background is solid black), 51,374 bytes. A proper multi-resolution .ico (16/32/48/256) would need
ImageMagick, not available here; ask if that's wanted later.
This is the only file-format step taken on the icon; the artwork itself (the mascot) is entirely hers, not generated or altered by any session.

## All three passes green — 2026-09-27

`20260927-173317-006-8b5a` (sans facultatifs), `20260927-173318-369-0a20` (avec RIMMSQOL), `20260927-173319-769-4b89` (avec neolithic et nps):
all exit 0, first attempt. Confirms the shortcut fix (loading a save before the MainButtons route) and the MayRequire-ambiguous addition, both in
one real run. Combined with the earlier green restart pair (`b0c6`): every scenario written so far has now passed in a real game at least once.
Not yet played: French, the real Elves ambiguity (needs a download), a joint pass with AlphaMythologyRenew's own patches (nothing in them consumes
the library yet).

## Standalone repository created — 2026-09-27 (Virginie: public, "Rimworld-Resolve-This-Instead")

`git init` inside `ResolveThisInstead/` (was untracked in the monorepo); parent `.gitignore` updated with the standard isolation entry and
comment, matching AdaptiveStorageNeolithicRenew's/ArchitectStudio's pattern (no `git rm --cached` needed: never tracked there). One `desktop.ini`
(Windows Explorer noise) caught before the first commit and added to this repo's own `.gitignore` too.
Initial commit `420d9ee`, 68 files, pushed to `main` on a fresh public GitHub repository (`gh repo create --public --source=. --remote=origin --push`),
account `vbardales`, same naming template as the other public repos (`Rimworld-<Name>`, no `Nelim's` prefix reserved for private mods, no `Renew`
suffix reserved for ports). Local HEAD and `origin/main` verified identical (`420d9ee9033d2325f96bbd1f764d49679c13dc20`).
**Open, by her explicit choice**: made public despite the licence of this library's own code (not the data, already MIT via Use This Instead/
ATTRIBUTION.md) still being undecided (design question 2). No LICENSE file was added: inventing one was not asked, and AGENTS.md's
dansMonoRepo -> horsMonoRepo checklist ("licence definie, justifiee") is not yet fully met, flagged rather than silently resolved by a default choice.

## ModIcon provenance confirmed by the owner — 2026-09-27

Virginie confirmed she made `Mod/About/ModIcon.png` herself ("je t'ai créé un modIcon normalement"), consistent with `STYLE_RIMWORLD.md`'s
"ModIcon : contrôle, pas génération" rule. No new file appeared since the resize/`.ico` step earlier today; this is the same delivered icon
already checked at 32px, resized to 128x128/20,792 bytes with her explicit go-ahead, and converted to `Art/ModIcon.ico`.
