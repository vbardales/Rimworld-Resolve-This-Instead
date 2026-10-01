# Changelog

Format inspired by [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).
This file serves the repository and the writing of Steam patch notes; RimWorld does not display it in game.

## [Unreleased]

First public content, not yet published to the Workshop (no `About/PublishedFileId.txt`: the 0.1.0 pre-publication that creates the item has not happened).
When it does, a `## [0.1.0]` section ("creation of a publishIdFile") goes below this one with `Mod/` as it was at the sent commit.

### Added

- Alias engine: a mod that asks whether another mod is loaded (patch guard, `MayRequire`, `ModsConfig.IsActive`) gets "yes" when a known replacement is loaded,
  from Use This Instead's data and from a small file a mod can embed to vouch for a pair.
- Settings (Mod options -> Resolve This Instead, plus a hidden MainButtons shortcut for RIMMSQOL): one checkbox per replacement, a chooser when several successors are loaded.
- English and French interface.
- "Debug logs" option and exception logging around every Harmony patch.
