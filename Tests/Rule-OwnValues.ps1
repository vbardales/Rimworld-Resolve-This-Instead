# What only this mod asserts: About identity and dependencies, the not-affiliated line, the hidden MainButtons shortcut and its worker class.
# The generic checks (XML, translation coverage, DefInjected, DLL present) are the shared scripts', run by the generated Test-Mod.ps1.
# Takes -ModRoot (repository root) and -SharedDir (unused, kept for the Test-Mod.ps1 contract); prints findings, exits 1 on any.
param([string]$ModRoot = (Join-Path $PSScriptRoot '..'), [string]$SharedDir)
$ErrorActionPreference = 'Stop'
$ModRoot = (Resolve-Path -LiteralPath $ModRoot).Path
$script:findings = New-Object System.Collections.Generic.List[string]
function Assert-True($Condition, [string]$Message) { if (-not $Condition) { $script:findings.Add("FAIL: $Message") } }

$about = [xml](Get-Content (Join-Path $ModRoot 'Mod/About/About.xml') -Raw -Encoding UTF8)
Assert-True ($about.ModMetaData.packageId -ceq 'nelim.resolvethisinstead') 'About: packageId is nelim.resolvethisinstead'
$deps = @($about.ModMetaData.modDependencies.li | ForEach-Object { $_.packageId.ToLower() })
Assert-True (($deps -contains 'brrainz.harmony') -and ($deps -contains 'mlie.usethisinstead')) 'About: Harmony and Use This Instead are hard dependencies'
$after = @($about.ModMetaData.loadAfter.li | ForEach-Object { $_.ToLower() })
Assert-True (($after -contains 'brrainz.harmony') -and ($after -contains 'mlie.usethisinstead')) 'About: loadAfter Harmony and Use This Instead'
Assert-True ($about.ModMetaData.description -match 'Not affiliated with or endorsed by Mlie') 'About: says it is not affiliated with Mlie'

$btn = [xml](Get-Content (Join-Path $ModRoot 'Mod/Defs/MainButtonDefs.xml') -Raw -Encoding UTF8)
$def = $btn.Defs.MainButtonDef
Assert-True ($def.defName -eq 'RTI_Settings' -and $def.buttonVisible -eq 'false') 'shortcut: RTI_Settings exists and is hidden by default (buttonVisible false)'
Assert-True ($def.workerClass -eq 'ResolveThisInstead.MainButtonWorker_ResolveThisInsteadSettings') 'shortcut: worker class name matches the source'
Assert-True ((Get-Content (Join-Path $ModRoot 'Source/Mod/RtiMod.cs') -Raw) -match 'class MainButtonWorker_ResolveThisInsteadSettings') 'shortcut: the worker class exists in the source'

if ($script:findings.Count -gt 0) { $script:findings | ForEach-Object { $_ }; exit 1 }
'Rule-OwnValues: 7 checks passed'
