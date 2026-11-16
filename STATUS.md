---
mod: Resolve This Instead
packageId: nelim.resolvethisinstead
repo: https://github.com/vbardales/Rimworld-Resolve-This-Instead
visibility: public
local_path: C:/Users/nelim/Documents/rimworld/ResolveThisInstead
git_isolation: standalone; own .git and origin, public; ignored (and never tracked) in the parent monorepo
workflow_stage: playTests[1.0.0]
settings_audit: partial
localization: complete
translation_en: complete
translation_fr: complete
audit_at: 2026-10-01
audit_revision: d5223bc plus uncommitted (debug logging in Source/Mod, two new keys EN/FR, rebuilt DLL 79855596cc0e, FRENCH_REVIEW.md, docs)
automated_tests: engine 57/57 without data (dotnet run, 2026-10-01; 61/61 with data on 2026-09-26); static checks 18/18 (Tests/Rule-OwnValues.ps1, 2026-10-01)
functional_tests: Pickle played green on 2026-09-27 (lib-v8, three passes, exit 0, DLL before the debug logging) and the restart pair green (lib-v6); not replayed on the current tree; French not played
licence: MIT (decided by the owner 2026-10-01; LICENSE added; data source Use This Instead is MIT too)
licence_at: 2026-10-01; no upstream licence conflict (emipa606/UseThisInstead MIT, MiniJSON MIT notice kept)
upstream_mod_remotes:
  - https://github.com/emipa606/UseThisInstead
upstream_synced: 2026-11-16 emipa606/UseThisInstead@956fb42
workshop_required_items: n/a, not released yet (pre-publication 0.1.0 only; owner decision 2026-11-10 "on verra en 1.0.0"; manual pose at prepareRelease/publish)
publishedFileId: 3811295032 (0.1.0 pre-publication, 2026-10-01; Mod/About/PublishedFileId.txt)
tested_on: 2026-11-16 pickle 2f35ccd (DLL 61e65ee28264): settings defaults 4/4 (v16), English 7/7 (v13) and French 7/7 (v12), no raw key in Player.log; v11 avec neolithic et nps 133/133, v10 restart pair green on the previous DLL, v9 avec RIMMSQOL green on the previous DLL; clipping not checked (no capture)
code_review_sha: d92e5f9f1307eb2ba0c8ac3941a2b6dec4a8f3b7
evidence: Tests/Pickle/Evidence (disk only, 1 MB: lib-v8 x3 passes + lib-v6 restart pair); history in docs/runs/history.md
remaining:
  - ecosystem (2.h) done 2026-11-16, see the Ecosystem section; upstream_synced written 2026-11-16 (3.0: upstream Source change since 2026-09-26 is d24d2ec, removes example comments from Json.cs, nothing to integrate)
  - playTests: 8.c 8.g 8.i 8.k open (8.h done 2026-11-16: debug switch, restore defaults, old settings file green in game v16; English 7/7, French 7/7; 8.d n/a no @review scenario)
  - test shape: convert to Tests/Rule-OwnValues.ps1 + generated Test-Mod.ps1 (AGENTS.md "Mod tests shape"; LOCAL-SCRIPTS-REVIEW-2026-11-04.md), then tell Audit
  - French pass 20261110-203439-273-9cde: green 7/7, exit 0, found in Inbox/v12-french on 2026-11-12 (Player.log: no raw RTI_ key, no missing-key line); still to do: look at the screenshots/report for accented fallback and clipping (PICKLE.md "Lecture d une interface traduite"), then 8.h/8.i, 8.n; /review-translation before Virginie's next French review (AUDIT 6.g)
  - defect (code review 2026-11-10, logged not fixed): #4 caller walk may read Assembly-CSharp if Harmony maps replacement frames to originals (runtime opt-in would silently fail; confirm with an opted-in caller); #7 a proposal A->B with only C loaded never listed, name-only rules skip ambiguity check; #8 UTI data read in the constructor can be stale or half-written (move to BeginLoading, FileShare.ReadWrite); #9 ActiveMod scans the mod list per alias hit (index it); #10 Loading cleared only by one prefix (add a backstop postfix); UtiPath logged in full with debug on
  - unverified: the crash reported 2026-10-01 ("RTI plante"): cause not established; debug logging added to find it (Player.log needed)
  - unverified: Pickle not replayed on the tree with the debug logging; settings_audit reset to partial because an option (debug logs) was added
  - unverified: French and English in game (raw keys, fallback text, clipping), at playTests
  - unverified: the aliases do not leak once loading is over beyond what v8 scenarios assert; real Elves ambiguity (needs a download, owner's go)
  - owner only: telling Mlie when tests start; the Preview; the Workshop page before any consumer's 1.0.0; PRs to emipa606/UseThisInstead only with her agreement (none planned yet)
updated: 2026-11-16
protocols_read_sha: 1a0f52eba37ebe89343961296aaf24f346c91d73
---

Art/Icons/ModIcon-source.png (1,212,318 bytes, untrimmed icon original) removed 2026-11-12 on Audit's order: Art/ModIcon-source.png is the source; the original stays in history at 420d9ee. Mod/desktop.ini restored (git-ignored, AUDIT 10.c bis).

## Ecosystem (AUDIT 1.a and 2.h): PiI report 2026-11-16, docs/inventory/ecosystem-steam/ResolveThisInstead.md

No Steam mod aliases a replaced packageId to its successor (12 search terms, about 60 hits). Use This Instead 3396308787 (alive, updated 2026-11-12, 1.4 to 1.7) is our data source and hard dependency, not a parent (no original mod is replaced): it warns and links, it does not alias; no patch or integration of it. Verdict: the niche is empty, no duplicate, nothing to integrate at localize (the Russian translations of Use This Instead found by PiI translate its texts, not ours, and are not used). No subscription made.

## writeTests record (AUDIT.md 7): 2026-11-03, revision 0ed0ebf plus the Tests/ changes of this commit

- Scenarios (7.a): `Tests/TESTING.md`, one row per promised behaviour (library load, aliases only while data loads, declared pair, proposed pair stays off, runtime opt-in by caller, ambiguity, settings window, hidden shortcut, RIMMSQOL, restart persistence), plus the list of what is not covered. `PUBLICATION.md` does not exist yet (written at `writeDocs`); the rows follow the About.xml description and README.
- Automated, rerun 2026-11-03 on library DLL sha256 `79855596cc0e` (unchanged since 2026-10-01, newer than every file under `Source/`): engine unit tests `dotnet run -c Release` in `Tests/UnitTests` 57 of 57 passed (real-data checks skipped, no `RTI_UTI_JSON`); `Tests/Rule-OwnValues.ps1` 7 of 7; `Check-ModBasics` PASS; `Check-Translations` PASS; `Check-Steps` PASS (26 patterns, 72 step lines, none unused) (7.b, 7.f).
- XML tests (7.c): `Check-ModBasics` (5 XML files, 1 def) and `Check-Translations`; no XML patches in `Mod/`.
- Pickle suites (7.d): 9 feature files, 24 scenarios, not played on this tree. New scenario "Use This Instead's data was found" in `01-loading`; the three unused step patterns removed (two with no scenario to serve). Steps DLL rebuilt, sha256 `81c48c0335b9`. Local steps declared to NPT through TM on 2026-11-03 (no NPT session in the agent list). `@requires`: `nelim.adaptivestorageneolithic`, `mlie.naturesprettysweet`, `MalteSchulze.RIMMSqol` + `nelim.pickletools.rimmsqol`, `nelim.resolvethisinstead.testconsumer` (staged from `Tests/Pickle/Consumer`), each with its map (`Tests/Pickle/README.md`).
- Not applicable (7.e): nothing declared not applicable.
- Translations (7.g): complete, French reviewed by Virginie 2026-11-03.
- Local scripts: `Tests/Rule-OwnValues.ps1` justified in `scripts/LOCAL_SCRIPTS.md`, awaiting Audit's Validated.

## Translation audit (AUDIT.md 6, TRANSLATIONS.md): 2026-11-03, revision b3c11d7 plus the two French fixes below

Where the text lives (6.a): English is the source language: 19 `RTI_*` Keyed keys (`Mod/Languages/English/Keyed/RTI.xml`, all resolved by `.Translate()` in `Source/Mod/RtiMod.cs`, no hard-coded displayed text found; log lines stay English), the `RTI_Settings` MainButtonDef label and description (Def text is the English; French in `Languages/French/DefInjected/MainButtonDef/RTI.xml`), and the About.xml description (English only, Steam and mod list). French: `Languages/French/Keyed/RTI.xml` (same 19 keys) and the DefInjected file above. No other language, no LoadFolders, no optional integration text.
Checks (6.b, 6.c, 2026-11-03): `Check-Translations.ps1` PASS (2 source texts, 19 Keyed calls, coverage, stale, duplicates, tokens); `Check-ModBasics.ps1` PASS. `Check-DefInjected.ps1` not applicable (the Def is this mod's own, covered by Check-Translations).
French reading (6.d), every sentence read: no tu, no vous, no nous. Two imperatives addressed to the player were found and fixed: `RTI_Problems` "survolez pour le détail" now "détail au survol"; `RTI_Ambiguous` "choisissez-en un" now "il faut en choisir un". Gender (6.e): no text agrees with a pawn, so no gender switch and no player choice. "Mod" and the packageName/mod names stay as in the game.
`FRENCH_REVIEW.md` (6.f): regenerated with `scripts/Make-FrenchReview.ps1 -RepoType original`; the Original column repeats the English (no other source language).
Review by Virginie (6.g): done 2026-11-03 on revision 0ed0ebf (she validated FRENCH_REVIEW.md as it stands). Corrections made before her review, each accepted by her after ChatGPT reviews (six pages, last one without finding): RTI_Settings.description FR and EN, RTI_DebugLogsTip, RTI_CompatNo, RTI_ListHeader. The session did not mark its own French as reviewed.
Review by Virginie 2026-11-12: TRANSLATION_REVIEW.md validated after two ChatGPT passes (page 1 of 1, 0 applied, 1 refused: Docs/review-refusals.md), revision 5b39256, no correction. Preview title: no "Nelim's" (owner, 2026-11-12).
Fields (2026-11-03): `localization`, `translation_en`, `translation_fr` all `complete`. Raw keys, fallback text and clipping in game stay `unverified` until `playTests`.

## Settings audit (AUDIT.md 5, MOD_SETTINGS.md): 2026-11-02, revision c65db21 (`Source/Mod/RtiMod.cs`, `Runtime.cs`, `Mod/Defs/MainButtonDefs.xml`, `Mod/Languages/*/Keyed/RTI.xml`)

Status: `settings_audit: partial` (code and definition analysis and automated checks done; the in-game checks of the current DLL are `unverified`, made at `playTests`).

Inventory and decision (5.a, 5.b): settings are useful, since the library cannot know which replacement a player accepts. Scope is global (mod settings, not per save). Every option applies at the next game start (the window says so: `RTI_RestartNote`; the choices are fixed for a load).
- One checkbox per replacement that applies in this game (old mod absent, successor loaded). Default: on only where a consuming mod vouched for the pair (`compatible: yes`); a bare proposal from Use This Instead stays off; a pair declared not compatible stays off. Tooltip: replaces, by, source, compatibility.
- A chooser when several successors are loaded (ambiguity: nothing resolves until the player picks).
- "Debug logs" (`debugLogs`, tooltip `RTI_DebugLogsTip`): extra log lines. Added 2026-10-01 to find the reported crash; default off since 2026-11-03 (commit follows; the player or RimWorld verbose logging turns it on).
- "Restore defaults" button; data status line (Use This Instead version, rule count, declared pairs) and a problems line.
- Not exposed on purpose: provider modules, load-phase boundaries, trusted-caller opt-in (declared by the consuming mod, never by the player).

Access (5.c, 5.d): Mod options -> Resolve This Instead (`SettingsCategory`), no customization mod needed. MainButtons shortcut `RTI_Settings`: `buttonVisible` false (checked by `Tests/Rule-OwnValues.ps1`, 7 of 7 on 2026-11-02), worker class `MainButtonWorker_ResolveThisInsteadSettings` opens the same window and the same `Settings` instance.

Usage review (5.e): checkboxes and one chooser only, so no free input and no boundary validation (the "Input validation" row is `n/a`). Labels, tooltips and the restart notice use stable `RTI_*` keys in English and French. Empty list shows `RTI_ListEmpty`, the list scrolls. `debugLogs` defaulted to true while the crash was being chased: fixed 2026-11-03, default false (code change, DLL `bf62ca694b24`).

Expected and observed, per MOD_SETTINGS.md section 4 (observed = last played run, lib-v8 of 2026-09-27, DLL before the debug logging; evidence `Tests/Pickle/Evidence`, one line per run in `Docs/runs/history.md`):

| Area | Observed |
| --- | --- |
| First use | Green: 01-loading, library loads after its dependencies, no startup error (lib-v8 x3). |
| Primary access | Green: 04-settings-window opens, sees and edits the same instance. |
| Actual effect | Green: 02-aliases (name guard, MayRequire, IsActive, mod list), 09-ambiguity. Debug logs toggle: not played. |
| Persistence | Green: 07/08 restart pair (lib-v6): disabled alias and chosen successor survived a real restart. Not replayed on the current DLL. |
| Input validation | n/a (checkboxes and a chooser). |
| Defaults and upgrades | Restore defaults: not played. Older stored settings without `debugLogs`: not played (the field default applies). |
| Optional dependencies | Green: avec neolithic et nps pass (4b89) and the sans facultatifs pass. |
| Shortcut default | Green: 05-shortcut (hidden on a clean configuration); static check 7 of 7. |
| Shortcut integration | Green: 06-rimmsqol (RIMMSQOL only, no other customization mod claimed). |
| Runtime robustness | Not played on the current DLL: the exception logging added 2026-10-01 has never run in a game. |

Commands: `powershell -NoProfile -ExecutionPolicy Bypass -File Tests\Check-Mod.ps1`; `dotnet run -c Release` in `Tests/UnitTests` (57 of 57 without data, 2026-10-01); Pickle tickets at `playTests`.

## 2026-11-02: Dependencies (AUDIT.md 4.g), revision b3c11d7

- Hard dependencies (`modDependencies` and `loadAfter`, both in `Mod/About/About.xml`, Workshop URLs set):
  - Harmony `brrainz.harmony` (steam 2009463077): the engine hooks are Harmony patches (`Source/Mod/Runtime.cs`, `RtiMod.cs`).
  - Use This Instead `Mlie.UseThisInstead` (steam 3396308787): its `replacements.json` is the data source, no snapshot shipped (owner decision "dep dure", 2026-09-26).
- Optional integrations: none. No `MayRequire`, no `PatchOperationFindMod`, no `LoadFolders.xml`, no `incompatibleWith`.
- DLC: none referenced in `About.xml`, `Mod/Defs`, `Source/Mod` or `Source/Engine`. `supportedVersions`: 1.6 only (1.7 not declared before a green 1.7 run).
- Load order: no cycle (this mod after Harmony and Use This Instead; Use This Instead itself requires Harmony). `scripts/sort-load-order.py` needs a staged Mods folder: it ran inside the Pickle stagings, whose three passes and restart pair were green on 2026-09-27 (library loads after its dependencies without startup errors).
- 4.f animal-adding mods: not applicable (library, no animal).


Older journal (2026-09-26 to 2026-10-01: design, feasibility, decisions, audit): Docs/runs/status-journal-2026-09-10.md
