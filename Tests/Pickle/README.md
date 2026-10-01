# Pickle suite of Resolve This Instead

The companion mod `Resolve This Instead - Pickle tests` also plays a consuming mod: it embeds `About/ResolveThisInstead.xml` vouching for
Adaptive Storage Neolithic Module -> our Adaptive Storage Neolithic Renew, WITHOUT `<runtime>true</runtime>`. `Consumer/Mod` (assembly
`ResolveThisInstead.TestConsumer`, its own local mod with its own About.xml and its own embedded file, WITH `<runtime>true</runtime>` on the same
pair) plays a third mod "C": its constructor tests the chain-of-trust case (C asks for A, A -> B, so C and B must work without A), and its own
runtime code (called after loading) tests that the opt-in mechanism trusts its assembly specifically, not the companion's. Nothing here is published.

| Pass | Map | What it proves |
|---|---|---|
| sans facultatifs | none (`wsl-ids.map` only resolves Use This Instead) | the library loads after Harmony and Use This Instead with no error; the hidden shortcut def exists; the settings window belongs to this mod; the shortcut behaves; a declared alias with no successor loaded does nothing |
| avec neolithic et nps | `wsl-deps.avec-neolithic-nps.map` | a declared pair satisfies a `MayRequire` and a name guard on the missing original; a pair that Use This Instead only proposes (Nature's Pretty Sweet) stays off; once loading ends the original is not treated as loaded any more (IsActive, GetActiveModWithIdentifier, the mod list's incompatibleWith); a consumer mod's own constructor sees the original as absent and gets no "Mod options" entry for it; **the same pair is seen differently by two callers**: the companion's own untrusted call still sees reality, the separately-staged consumer's own trusted runtime code sees the alias; disabling a declared alias in the settings and restarting turns its guarded content off (`-Filter 07-restart-write -Then 08-restart-read`) |
| avec RIMMSQOL | `wsl-deps.avec-rimmsqol.map` | RIMMSQOL finds, reveals and hides the shortcut, and it opens the same settings |

Not covered yet: French, ambiguity (two successors loaded at once).
Everything here is `unverified` until a run is read.

Requests: `Submit-PickleRun.ps1 -Mod ResolveThisInstead [-DepMap wsl-deps.avec-neolithic-nps.map|wsl-deps.avec-rimmsqol.map] -EvidenceDir ResolveThisInstead/Tests/Pickle/Evidence/<run>`;
for the restart pair add `-Filter 07-restart-write -Then 08-restart-read` to the avec-neolithic-nps request.

## Evidence to keep (rule of AGENTS.md, "Test evidence")

`Tests/Pickle/Evidence/` is on disk only (gitignored). Per pass keep the latest run of the revision now in the repository: `junit.xml`, `summary.json`, `summary.md`,
`evidence-complete.txt`, `Player.log` and `messages.ndjson` when small. Delete `report.html` (1.5 MB, a rendering of the same data) and every superseded pass once a newer
one replaces it. Keep an older run only when it is the sole proof of a check the newest did not repeat: today the restart pair (`lib-v6-avec-neolithic-nps-restart`).
Screenshots (`@review`) are kept only when a scenario produces some and they were opened. One text line per run goes into `docs/runs/history.md`, never a folder.
Read `exitReason` and the discovered/played count before citing any report. Trimmed on 2026-10-01: 68 MB to 1 MB.
