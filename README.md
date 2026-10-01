# Resolve This Instead

A RimWorld 1.6 library that lets mods keep working when a mod they support has been replaced by a maintained continuation, without
every patch author chasing successors by hand.

- packageId: `nelim.resolvethisinstead` (confirmed 2026-09-26). The display name may still change; the packageId does not.
- Status: very start of development. Nothing is published, there is no repository yet, and the library itself (`Mod/`) does not exist.
  What exists: a pure-C# resolution engine with unit tests (`Source/Engine`, `Tests/UnitTests`) and a throwaway proof of concept (`Poc/`,
  queued as a Pickle request, not yet played). `STATUS.md` is the working record.
- **Not affiliated with or endorsed by Mlie.** The name echoes Use This Instead on purpose, because this library builds on that mod's open data.
  Its description, when there is one, must say so, credit Use This Instead (MIT) with a link, and ask Mlie's consent before any mention on his page.
  Check that no Workshop mod already uses this name.

## The problem

Patches that add compatibility with another mod are usually guarded by that mod's **display name** (`PatchOperationFindMod`), and names drift:
forks rename themselves, a "(Continued)" page replaces the original. Alpha Mythology Renew hit this on 2026-09-26:
`NatureIsPrettySweetPatch.xml` looks for "Nature's Pretty Sweet" while the installed page is "Nature's Pretty Sweet (Continued)", so the patch
does not apply. Seven other patches of that mod are guarded the same way.

## The idea (Virginie, 2026-09-26)

Like Use This Instead, which tells a player that a mod they run has been replaced, but acting: at game launch, when a mod asks "is X loaded?"
and X is not, answer yes if a known successor of X is. No patch has to be rewritten, and the mods that ask do not have to know.

## How it would work (from reading the game's code; not yet seen in a running game)

Order of a launch: every mod content pack is created (this is where `LoadFolders.xml` conditions are evaluated), then every mod DLL loads, then every
`Verse.Mod` class is instantiated, then XML patches are applied, then Defs are parsed. Our `Mod` constructor therefore runs before any patch guard or
`MayRequire` is read.

| The mod asks by... | Through | Reached by the alias |
|---|---|---|
| display name (`PatchOperationFindMod`, CCL `FindAllMods`) | `ModLister.HasActiveModWithName` | yes: Harmony postfix |
| `MayRequire`, `MayRequireAnyOf`, 57 mods' `AnyModActiveNoSuffix` | `ModLister.GetActiveModWithIdentifier` | yes: Harmony postfix |
| `ModsConfig.IsActive(id)` (5 mods' DLLs, engine bookkeeping, and any mod's own C#) | `ModsConfig.IsActive(string)` | yes, three windows below |
| `LoadFolders.xml` `IfModActive` | evaluated before any DLL loads | **no** |
| CCL `FindModByID`, XML Extensions `FindMod`, ModCheck `isModLoaded` | their own scans of the mod lists | **no** (provider modules, see below) |
| the mod list screen's missing-dependency warnings | before any of our code | **no** |

**Three windows decide whether an alias answers**, not one:
1. **Mod construction** (`LoadedModManager.CreateModClasses`, every `Mod` constructor): **never**, whoever asks. A mod C's own constructor must see reality, or it would go on to use another mod's types or settings that are not really there.
2. **Loading** (`LoadedModManager.LoadModXML` to `StaticConstructorOnStartupUtility.CallAll`, patches/`MayRequire`/Defs/cross-references): **always**, whoever asks, because every caller here is the engine itself processing mod-authored XML on everyone's behalf.
3. **Afterward, forever** (menus reopened later, hot reload, a mod's own runtime code): **only for a mod that explicitly asked for it** (see below) — never the engine's own bookkeeping (the mod list screen, `ModMetaData.Active`), which runs in this same window and must see reality.

Window 3 was not "any mod gets it, only the engine doesn't": that was tried and found unsafe while building the test for it (2026-09-27) — a diagnostic or compatibility-checking mod (even this library's own Pickle steps) wants the truth from `IsActive`, exactly like the engine's own bookkeeping does, and "the caller is some mod" cannot distinguish that from "a consumer that wants the convenience." The fix is an explicit per-consumer opt-in: a mod's embedded file (below) can add `<runtime>true</runtime>` to a rule, and the library then trusts **that mod's own assemblies** (not any mod, not the engine) to see the alias in window 3. One generic mechanism, scoped by explicit declaration rather than by guessing intent from caller category. Unverified without a real run: whether walking the call stack to find the immediate caller survives Mono's JIT (a frame could in principle be inlined away).

Rules: a successor stands in only if it is really loaded; chains A -> B -> C are followed to the most recent loaded link (a breadth-first walk with a
visited set: Use This Instead has seven 2-cycles); several successors at the same distance are ambiguous and the player picks. A settings menu
(Mod options -> Resolve This Instead) lists every alias with a checkbox. Default: an alias is **on** only if a consuming mod's embedded file vouches
for it (`compatible: true`); a bare snapshot pair is only a proposal and stays **off**; the player outranks everything.

Provider modules (CCL Highlander, XML Extensions, ModCheck) would be optional add-ons, written only on a real need and only if the provider's own
options do not already solve it (XML Extensions has a `packageId` option; ModCheck has hand-filled `altModNames`). Alpha Mythology Renew needs none:
its guards are vanilla `PatchOperationFindMod`.

## The embedded file

A consuming mod may embed `About/ResolveThisInstead.xml` to say which replacements it supports (parsed by `Source/Engine/EmbeddedFile.cs`):

```xml
<ResolveThisInstead>
  <alias>
    <old><packageId>adaptive.storage.neolithic</packageId><name>Adaptive Storage Neolithic Module</name></old>
    <successor><packageId>nelim.adaptivestorageneolithic</packageId></successor>
    <compatible>true</compatible>   <!-- true | false | unknown; pre-ticks the box -->
    <runtime>false</runtime>        <!-- true | false, default false; see window 3 above -->
    <note>Same defNames.</note>
  </alias>
</ResolveThisInstead>
```

A malformed entry is reported and skipped: a broken file must never stop a game from loading. Two files disagreeing: "no" beats "yes". `<runtime>true</runtime>` is separate from `<compatible>`: it does not change whether the checkbox is pre-ticked, only whether THIS mod's own C# (not the engine's) also sees the alias after loading is over. A mod that only wants its XML patches to apply through the alias leaves it out.

## Data: Use This Instead's rules (MIT)

Source: `emipa606/UseThisInstead`, `replacements.json.gz` (184,754 bytes, commit `4454f15`, version stamp 2026-09-26T18:35:17Z), MIT licence,
Copyright (c) 2020 Mlie. Read with `gh api`; the file is kept outside this repository. Fields: `oldWorkshopId`, `oldName`, `oldAuthor`, `oldPackageId`,
`newWorkshopId`, `newName`, `newAuthor`, `newPackageId`, `oldVersions`, `newVersions`. Measured on that file:

- 2,710 rules (1.38 MB uncompressed), 2,298 distinct aliases once duplicates and "same id" rules are dropped.
- **435 rules (16%) have no usable `oldPackageId`** (empty, null, or, for 38, a Workshop id written as a number): they can only be found by name.
- **392 rules (14%) keep the same packageId** across the continuation (Therapy -> Therapy (Continued)): no id alias is needed, the name still differs.
- 1,883 rules (69%) change packageId: the case an id alias serves.
- Chains: 2,589 one-hop, 112 two-hop, 1 three-hop (`windowsxp.realisticplanets` -> `zvq.realisticplanetscontinued` -> `koth.realisticplanets1.6` -> `koth.realisticplanets2`). Fifty packageId chains are at least two hops.
- **Seven cycles** (Tribal Instruments <-> TribalInstruments (Royalty), Aztec Factions and Culture Patch, Expanded Artillery Heavy Shells, ...) and one rule that points at itself: the loop guard is real.
- 17 old packageIds have more than one distinct successor (Rim of Madness Werewolves and Vampires, Raise the Roof, ...): ambiguity is real.
- Package ids can carry stacked postfixes (`salmontoastie.lizardfolk_copy_copy`).
- Our own pair, Adaptive Storage Neolithic Module -> Adaptive Storage Neolithic Renew, is **not** in the data: the consuming mod's embedded file is the source for pairs the data lacks.
- Checked cases: Nature's Pretty Sweet (`tkkntkkn.nps` -> `Mlie.NaturesPrettySweet`), [XND] Nocturnal Animals, Lord of the Rims - Elves (two "Continued" pages exist, one per old id).

## What exists and what is tested

- `Source/Engine` (`AliasModel`, `AliasCatalog`, `AliasResolver`, `EmbeddedFile`, `UtiData`): pure C#, no game dependency. `Tests/UnitTests`
  (`dotnet run -c Release -- <replacements.json>`): 65 checks on 2026-09-27, all passed.
- `Mod/` + `Source/Mod` (`Runtime.cs`, `RtiMod.cs`): the real library, built (0 warnings, 0 errors), declaring Harmony and Use This Instead as hard
  dependencies, with a settings window and a hidden MainButtons shortcut. **Never loaded in a real game.**
- `Tests/Pickle`: a companion mod (also a consumer, via its own embedded file) plus a separately-staged third mod "C"
  (`Tests/Pickle/Consumer`, opted into the runtime window) whose scenarios cover the doors above, including window 3's caller discrimination.
  Requests filed 2026-09-27, not played. `Poc/` (the earlier, cruder proof of concept, superseded by this real suite) was abandoned in the Pickle
  queue and not refiled. Until a run is read, every claim above is a reading of the code, not an observation.

## Open design questions

1. ~~Data source~~ **Decided 2026-09-26 (Virginie): hard dependency on Use This Instead, no shipped snapshot** (see "Where the data comes from"). No snapshot to generate or refresh.
2. Credit and licence: MIT allows using the data with its notice; the notice and a link to Use This Instead go in the description, `THANKS`, and the Workshop
   comment register (page 3396308787, Mlie). This library is MIT licensed (decided 2026-10-01, see `LICENSE`).
3. Dependency (Virginie's decision, relayed 2026-09-26: Alpha Mythology Renew will require it) versus embedded: dependency. Consequence: a public Workshop page before
   that mod's 1.0.0.
4. ~~Scope of the alias~~ **Settled 2026-09-27**: never during Mod construction; unconditional while loading; afterward, only for a mod that
   explicitly opted in via `<runtime>true</runtime>` (see "How it would work", window 3). Not the same question as it looked at first ("global
   vs per-consumer for `IsActive`"): the real split turned out to be per-window, and window 3 needed a caller-based, opt-in mechanism, not a
   blanket rule either way. Unverified without a real run.
5. The loading-screen "press Enter for more options" pattern for reaching the menu: how other mods do it is not known.

## First consumer

Alpha Mythology Renew (`../AlphaMythologyRenew`), whose eight name guards in `Mod/Patches/AlphaMythology/*.xml` start with Nature's Pretty Sweet. Its session
declares this library in one commit once `ResolveThisInstead/Mod` exists, carries `nelim.resolvethisinstead` in its `About.xml` and loads.

## Other prior art seen

- ModCheck (Continued), Workshop 2035333096: patch operations testing whether a mod is loaded, by name; 1.0 to 1.2 only, its page says the game now covers it.
- A def-based guard (`PatchOperationConditional` on an xpath for a def the other mod defines): survives renames, already used in Alpha Mythology Renew's Giddy-Up patch.
- Use This Instead, Workshop 3396308787 (Mlie, MIT, 236,000 subscribers, requires Harmony): the source of the rules.
- Bonne Compatibilite, Workshop 3675397267 (GOS): a per-mod compatibility pack guarded by packageId; a pattern only, not a resolver.
- Existing packageId guards found by searching the installed Workshop (2026-09-26, `Search-Workshop.sh`): XML Extensions `FindMod` (226 lines), ModCheck `isModLoaded` (153),
  JPTools `FindModById` (113), CCL `FindModByID` (58), and others. None knows replacements.

## Where the data comes from at runtime (decided 2026-09-26)

Use This Instead is a **hard dependency** of this library (`Mlie.UseThisInstead`, Workshop 3396308787, itself requires Harmony). It downloads `replacements.json.gz` at startup (from `data.litet.net`, falling back to the GitHub raw file, with an ETag) and writes `replacements.json` in its own mod folder. This library downloads nothing and ships no snapshot: it reads that file (find the mod by packageId, read `RootDir/replacements.json`). Safeguards: a missing or unreadable file is logged and the library continues with the consuming mods' embedded files only (they are what vouches for an alias; UTI's pairs are proposals, off by default); the menu shows the data's `version` stamp, because the copy on the author's machine is stamped 2026-02-09, seven months behind the repository. The file's location and format are not a public interface of UTI: parse tolerantly. Telling Mlie is the owner's call (outward-facing).

