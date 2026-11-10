# Changelog

Format inspired by [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).
This file serves the repository and the writing of Steam patch notes; RimWorld does not display it in game.

## [Unreleased]

First public content, not yet released. The Workshop item exists (`Mod/About/PublishedFileId.txt`) since the 0.1.0 pre-publication below.

### Added

- Alias engine: a mod that asks whether another mod is loaded (patch guard, `MayRequire`, `ModsConfig.IsActive`) gets "yes" when a known replacement is loaded,
  from Use This Instead's data and from a small file a mod can embed to vouch for a pair.
- Settings (Mod options -> Resolve This Instead, plus a hidden MainButtons shortcut for RIMMSQOL): one checkbox per replacement, a chooser when several successors are loaded.
- English and French interface.
- "Debug logs" option and exception logging around every Harmony patch.

### Fixed

- A malformed or absurdly nested Use This Instead data file can no longer hang or crash the game start (stray colon in an array, unbounded nesting).
- The alias caches are now safe against concurrent callers while the game loads.
- No stack walk on every "is mod loaded?" call after loading when no mod asked for the runtime window.
- The exception cap now counts per patched method (it counted per mod id), and the problems list is capped.
- The settings window copies the problems list under a lock before reading it.

## [0.1.0] - 2026-10-01

Creation of a publishIdFile (pre-publication that creates the Workshop item). `Mod/` as at commit 6715612.
