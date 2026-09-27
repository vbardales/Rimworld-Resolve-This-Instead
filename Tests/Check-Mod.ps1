# Static checks of the Resolve This Instead mod: well-formed XML, About identity, hidden shortcut, translation keys used in code exist in
# English and French (and are not duplicated), French Def fields exist. Runs on Windows PowerShell 5.1 or PowerShell 7. No game needed.
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$fail = 0; $n = 0
function Check($ok, $name) { $script:n++; if ($ok) { "PASS $name" } else { "FAIL $name"; $script:fail++ } }

$xmlFiles = Get-ChildItem "$root/Mod" -Recurse -Filter *.xml
foreach ($f in $xmlFiles) {
    $ok = $true; try { [xml](Get-Content $f.FullName -Raw -Encoding UTF8) | Out-Null } catch { $ok = $false }
    Check $ok "well-formed: $($f.FullName.Substring($root.Length + 1))"
}

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

function Keys($path) { $x = [xml](Get-Content $path -Raw -Encoding UTF8); @($x.LanguageData.ChildNodes | Where-Object { $_.NodeType -eq 'Element' } | ForEach-Object { $_.Name }) }
$en = Keys "$root/Mod/Languages/English/Keyed/RTI.xml"; $fr = Keys "$root/Mod/Languages/French/Keyed/RTI.xml"
Check (($en | Sort-Object -Unique).Count -eq $en.Count -and ($fr | Sort-Object -Unique).Count -eq $fr.Count) 'keys: no duplicate key in English or French'
Check (-not (Compare-Object ($en | Sort-Object) ($fr | Sort-Object))) 'keys: English and French define the same keys'
$used = @(Select-String -Path "$root/Source/Mod/*.cs" -Pattern '"(RTI_[A-Za-z]+)"\.Translate' -AllMatches | ForEach-Object { $_.Matches } | ForEach-Object { $_.Groups[1].Value } | Sort-Object -Unique)
$missing = @($used | Where-Object { $en -notcontains $_ -or $fr -notcontains $_ })
Check ($missing.Count -eq 0) ("keys: every key used in code resolves in both languages" + $(if ($missing) { " (missing: $($missing -join ', '))" } else { '' }))
$unused = @($en | Where-Object { $used -notcontains $_ })
Check ($unused.Count -eq 0) ("keys: every defined key is used" + $(if ($unused) { " (unused: $($unused -join ', '))" } else { '' }))
$fi = [xml](Get-Content "$root/Mod/Languages/French/DefInjected/MainButtonDef/RTI.xml" -Raw -Encoding UTF8)
$fiNames = @($fi.LanguageData.ChildNodes | Where-Object { $_.NodeType -eq 'Element' } | ForEach-Object { $_.Name })
Check (($fiNames -contains 'RTI_Settings.label') -and ($fiNames -contains 'RTI_Settings.description')) 'French DefInjected covers the shortcut label and description'

Check (Test-Path "$root/Mod/Assemblies/ResolveThisInstead.dll") 'the built assembly is in Mod/Assemblies'
''
if ($fail -eq 0) { "ALL $n CHECKS PASSED" } else { "$fail of $n CHECKS FAILED"; exit 1 }
