# Resolve This Instead: proof of concept (test only)

Throwaway mod that checks the theory in `../STATUS.md` ("Feasibility read" and "Forcing IsActive") on a pair we own:
Adaptive Storage Neolithic Module (`adaptive.storage.neolithic`, Workshop 3033901895, the original, must be DISABLED) and
our update Adaptive Storage Neolithic Renew (`nelim.adaptivestorageneolithic`, Workshop 3806101377, must be enabled). It is not the library: aliases are hardcoded, there is no menu, no data file, no settings.
Not affiliated with or endorsed by Mlie (Use This Instead).

## What it does

From a `Verse.Mod` constructor (which runs after every DLL is loaded and before patches and Defs are processed):
- Harmony postfix on `ModLister.GetActiveModWithIdentifier`: old packageId -> successor if it is active
  (the path of `MayRequire`, `AnyModActiveNoSuffix`, `AllModsActiveNoSuffix`).
- Harmony postfix on `ModLister.HasActiveModWithName`: old display name -> successor (the path of `PatchOperationFindMod`).
- Adds the old packageId to the private `ModsConfig.activeModsHashSet` (what `ModsConfig.IsActive` reads), and again
  after every `RecacheActiveMods`.

## What it checks (one `[RTI-POC]` line each in Player.log, PASS or FAIL, then a RESULT line)

| Check | Expected with the successor active |
|---|---|
| `HasActiveModWithName("Adaptive Storage Neolithic Module")` | True |
| `ModsConfig.IsActive("adaptive.storage.neolithic")` | True |
| `ModLister.AnyModActiveNoSuffix` / `AllModsActiveNoSuffix` on `adaptive.storage.neolithic` | True |
| XML patch guarded by the old NAME added the def `RTI_PocPatchGuard` | True |
| Def with `MayRequire="adaptive.storage.neolithic"` loaded (`RTI_PocMayRequire`) | True |
| Controls (nonexistent name / id, three of them) | False |

Not covered on purpose: `LoadFolders.xml` `IfModActive` (evaluated before any mod DLL loads).

## How to run it (by Virginie: a session does not launch RimWorld)

1. Junction the mod folder into the game's Mods folder:
   `powershell.exe -Command "New-Item -ItemType Junction -Path 'C:\Program Files (x86)\Steam\steamapps\common\RimWorld\Mods\RtiPoc' -Target 'C:\Users\nelim\Documents\rimworld\ResolveThisInstead\Poc\Mod'"`
2. In a test mod list (Harmony, Adaptive Storage Framework, Adaptive Storage Neolithic Renew, XML Extensions, CCL PatchOperation Highlander, this POC, and NOT the original Adaptive Storage Neolithic Module; both are installed, only one may be enabled),
   start the game to the main menu; no colony needed.
3. Send the `[RTI-POC]` lines of Player.log. Remove the junction afterwards (delete the `RtiPoc` link, not its target).

`RtiPoc.dll` is built from `Source/` (net48, Krafs.Rimworld.Ref 1.6, Lib.Harmony 2.3.3 excluded from the output).

## Second block: the providers' own guards (same launch)

Tests whether the existing provider methods already meet the need, on the same pair. Needs XML Extensions (2574315206) and
CCL PatchOperation Highlander (2552002158) enabled too; the log line `providers active` says whether each block is valid.

| Check | Expected | Why it matters |
|---|---|---|
| `XmlExtensions.FindMod` (packageId option) on the OLD id only (`RTI_PocXmlExtOldId`) | False | its own method does not know replacements |
| `XmlExtensions.FindMod` listing OLD and NEW ids (`RTI_PocXmlExtBoth`) | True | the consumer alone can already solve it, no library |
| CCL `FindModByID` on the OLD id (`RTI_PocCclOldId`) | True, through the POC's postfix on `ModMetaData.SamePackageId` | one game-level patch reaches CCL |

Side effect to watch: the `SamePackageId` postfix makes the successor answer to the old id for EVERY caller of that game method (not only CCL).
