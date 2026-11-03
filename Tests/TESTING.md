# Testing Resolve This Instead

One scenario per behaviour the mod promises (About.xml description, README). Expected results are written before any run; observed results live in `Docs/runs/history.md` (one line per run) and the evidence on disk (`Tests/Pickle/Evidence`, never in git). Everything below is `unverified` in game until a run of the current revision is read.

## Automated, off game

| Check | Command | Expected |
| --- | --- | --- |
| Engine unit tests (alias model, catalog, resolver, embedded file, Use This Instead data parser) | `dotnet run -c Release` in `Tests/UnitTests` | `ALL 57 CHECKS PASSED` (real-data checks are skipped unless `RTI_UTI_JSON` points at a `replacements.json`: 61 then) |
| Mod-specific static checks (identity, dependencies, hidden shortcut, worker class) | `powershell -NoProfile -ExecutionPolicy Bypass -File Tests\Check-Mod.ps1` | `ALL 7 CHECKS PASSED` |
| Shared checks | `Check-ModBasics.ps1`, `Check-Translations.ps1`, `Check-Steps.ps1` (protocols `scripts/`) | PASS |

## Functional scenarios (Pickle, `Tests/Pickle/Mod/Pickle/Features`)

Staging: `wsl-ids.map` (Use This Instead), `wsl-deps.avec-neolithic-nps.map`, `wsl-deps.avec-rimmsqol.map`. Pass map and filters: `Tests/Pickle/README.md`.

| Promise | Scenario (feature file) | Precondition | Action | Expected |
| --- | --- | --- | --- | --- |
| The library loads after its dependencies, and finds Use This Instead's data | 01-loading | Harmony, Use This Instead, the library | start the game | no startup error, hidden shortcut def exists, `replacements.json` found |
| Aliases answer only while data loads | 01-loading, 02-aliases | declared pair, original absent | read IsActive, mod list, name lookup after loading | original not treated as loaded |
| A declared replacement stands in for a missing mod | 02-aliases | Adaptive Storage Neolithic Renew loaded, original absent | guarded defs and patches | MayRequire and name guards on the original apply; controls stay absent |
| A proposed pair stays off | 03-proposals-off | Nature's Pretty Sweet (Continued) loaded | check its old page | not stood in for |
| A consuming mod opts in at runtime | 02-aliases | second staged mod with `<runtime>true</runtime>` | both mods call IsActive after loading | the opted-in assembly sees the alias, the other sees reality |
| Ambiguity does not guess | 09-ambiguity | two successors loaded for one old id | MayRequire and lookup | neither successor applies until the player picks |
| Settings open from Mod options | 04-settings-window | clean configuration | open Mod options, Resolve This Instead | same instance the hooks read |
| The shortcut is hidden, never greyed | 05-shortcut | clean configuration | read the MainButtons list, then reveal it | hidden; once revealed it draws, enables and opens the same settings |
| RIMMSQOL reveals the shortcut | 06-rimmsqol | RIMMSQOL staged | list, reveal, open | listed hidden; revealed it opens the same settings |
| Choices persist across a restart | 07-restart-write then 08-restart-read | declared pair | disable an alias, pick a successor, restart | both survived, guarded content and ambiguity follow |

## Not covered yet (stated, not hidden)

- "Debug logs" toggle, "Restore defaults", and the exception logging added 2026-10-01 never ran in a game.
- English and French interface in game (raw keys, fallback text, clipping): played at `playTests`.
- The real Lord of the Rims Elves ambiguity (needs a download and the owner's go); the synthetic pair in 09-ambiguity stands in.
- The crash reported 2026-10-01 has no scenario: the cause is not established.
- Settings migration from a file without `debugLogs`.
