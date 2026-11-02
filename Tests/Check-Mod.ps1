# Mod-specific static checks of Resolve This Instead: exact About identity and dependencies, the hidden MainButtons shortcut and its worker class.
# Generic checks (XML well-formed, translation coverage and duplicates, French DefInjected, DLL present) belong to the shared scripts
# (Check-ModBasics.ps1, Check-Translations.ps1). Run: powershell -NoProfile -ExecutionPolicy Bypass -File Tests\Check-Mod.ps1 (PowerShell 5.1 or 7). No game needed.
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$fail = 0; $n = 0
function Check($ok, $name) { $script:n++; if ($ok) { "PASS $name" } else { "FAIL $name"; $script:fail++ } }

$about = [xml](Get-Content "$root/Mod/About/About.xml" -Raw -Encoding UTF8)
Check ($about.ModMetaData.packageId -eq 'nelim.resolvethisinstead') 'About: packageId is nelim.resolvethisinstead'
$deps = @($about.ModMetaData.modDependencies.li | ForEach-Object { $_.packageId.ToLower() })
Check (($deps -contains 'brrainz.harmony') -and ($deps -contains 'mlie.usethisinstead')) 'About: Harmony and Use This Instead are hard dependencies'
$after = @($about.ModMetaData.loadAfter.li | ForEach-Object { $_.ToLower() })
Check (($after -contains 'brrainz.harmony') -and ($after -contains 'mlie.usethisinstead')) 'About: loadAfter Harmony and Use This Instead'
Check ($about.ModMetaData.description -match 'Not affiliated with or endorsed by Mlie') 'About: says it is not affiliated with Mlie'

$btn = [xml](Get-Content "$root/Mod/Defs/MainButtonDefs.xml" -Raw -Encoding UTF8)
$def = $btn.Defs.MainButtonDef
Check ($def.defName -eq 'RTI_Settings' -and $def.buttonVisible -eq 'false') 'shortcut: RTI_Settings exists and is hidden by default (buttonVisible false)'
Check ($def.workerClass -eq 'ResolveThisInstead.MainButtonWorker_ResolveThisInsteadSettings') 'shortcut: worker class name matches the source'
Check ((Get-Content "$root/Source/Mod/RtiMod.cs" -Raw) -match 'class MainButtonWorker_ResolveThisInsteadSettings') 'shortcut: the worker class exists in the source'
''
if ($fail -eq 0) { "ALL $n CHECKS PASSED" } else { "$fail of $n CHECKS FAILED"; exit 1 }
