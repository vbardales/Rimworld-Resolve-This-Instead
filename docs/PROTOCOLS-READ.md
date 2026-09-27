# Protocols read (local note)

Compare `git log -1 --format=%h -- <file>` (monorepo) with the version below before rereading.
Read 2026-09-26 by the Resolve This Instead session.

| Document | Version | Read | Useful now |
|---|---|---|---|
| AGENTS.md | 90d51374 | full | yes: gate order, evidence rules, CI publishing |
| AUDIT.md | 90d51374 | full | yes: stage chain and criteria; nothing beyond dansMonoRepo applies yet |
| scripts/SEARCHING.md | 90d51374 | full, twice (2026-09-26 and 2026-09-27, both after an owner reminder: no grep, not even Bash `grep`/`rg`, on any file, not only the corpus) | yes: use `scripts/Search-Workshop.sh` for the corpus (default mode ~5 min, foreground, ask first; `-i '*.xml'` for patches, `-i '*.dll'` for assemblies, UTF-16 pattern for string literals); the session's own Grep tool times out at 20s on the corpus and can report a partial result as complete; a negative result is not proof |
| STYLE_RIMWORLD.md | edited 2026-09-27, uncommitted | full (2026-09-27): ModIcon control section, Preview prompt convention (single triple-backtick block) | yes: added the resize/`.ico` paragraph myself; reread before the next preTest/publication pass of this mod in case someone else edits it further |
| MOD_SETTINGS.md, PUBLISHING.md, TRANSLATIONS.md, WORKSHOP_COMMENTS.md | - | not read | reread when the matching stage comes up |
| PickleTools/*, Rimworld-Release-Admin/docs/OPERATIONS.md, Rimworld-Ticket-Dispatcher/docs/* | - | not read | no tests or publication yet |
