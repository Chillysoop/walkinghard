$ErrorActionPreference='Stop'
$testRoot=Join-Path ([IO.Path]::GetTempPath()) ('WalkingHardLauncherTest-'+[Guid]::NewGuid().ToString('N'))
$cache=Join-Path $testRoot 'Cache'
New-Item -ItemType Directory -Path $cache -Force | Out-Null
$checks=0
function Require($Condition,[string]$Message){if(-not $Condition){throw $Message};$script:checks++}
try {
    . (Join-Path $PSScriptRoot 'Launch Main.ps1') -FunctionsOnly -CacheRoot $cache
    $fixture=Join-Path $testRoot 'fixture'
    New-Item -ItemType Directory -Path (Join-Path $fixture 'WalkingHard_Data\Managed') -Force | Out-Null
    foreach($f in @('WalkingHard.exe','UnityPlayer.dll','WalkingHard_Data\globalgamemanagers','WalkingHard_Data\Managed\Assembly-CSharp.dll')){Set-Content -LiteralPath (Join-Path $fixture $f) -Value 'Original test fixture, never executed.'}
    $zip=Join-Path $testRoot 'valid.zip';Compress-Archive -Path (Join-Path $fixture '*') -DestinationPath $zip
    $hash=(Get-FileHash -LiteralPath $zip -Algorithm SHA256).Hash
    $destination=Join-Path $cache 'Versions\old'
    Expand-VerifiedBuild $zip $hash $destination
    $first='0123456789012345678901234567890123456789';$second='1123456789012345678901234567890123456789'
    $saved=Save-Current $destination $first
    Require ((Read-Current).commit -eq $first) 'Current build was not saved.'
    $saved=Install-Current $destination $first
    Require ($saved.folder -eq (Join-Path $cache 'Current')) 'Player did not move to the stable Current location.'
    Remove-UpdateCandidate $destination
    Require (-not (Test-Path -LiteralPath $destination)) 'Old candidate was retained.'
    $sourceCandidate=Join-Path $cache 'Sources\temporary-source\Build\Windows'
    New-Item -ItemType Directory -Path $sourceCandidate -Force | Out-Null
    Copy-Item -LiteralPath (Join-Path $fixture 'WalkingHard.exe') -Destination $sourceCandidate
    Remove-UpdateCandidate $sourceCandidate
    Require (-not (Test-Path -LiteralPath (Join-Path $cache 'Sources\temporary-source'))) 'Local Unity build source was retained after promotion.'
    $before=Get-Content -LiteralPath (Join-Path $cache 'current.json') -Raw
    $rejected=$false;try{Expand-VerifiedBuild $zip ('0'*64) (Join-Path $cache 'bad')}catch{$rejected=$true}
    Require $rejected 'Corrupt checksum was accepted.'
    $rejected=$false;try{Install-Current (Join-Path $cache 'missing') $first}catch{$rejected=$true}
    Require $rejected 'Incomplete build was promoted.'
    Require ((Get-Content -LiteralPath (Join-Path $cache 'current.json') -Raw) -eq $before) 'Failed install changed the working record.'
    $malicious=Join-Path $testRoot 'unsafe.zip'
    $archive=[IO.Compression.ZipFile]::Open($malicious,[IO.Compression.ZipArchiveMode]::Create)
    try{$entry=$archive.CreateEntry('../outside.txt');$stream=$entry.Open();$stream.Dispose()}finally{$archive.Dispose()}
    $rejected=$false;try{Expand-VerifiedBuild $malicious (Get-FileHash $malicious).Hash (Join-Path $cache 'unsafe')}catch{$rejected=$true}
    Require ($rejected -and -not (Test-Path -LiteralPath (Join-Path $cache 'outside.txt'))) 'Archive containment failed.'
    $rejected=$false;try{Remove-CacheTree $testRoot}catch{$rejected=$true}
    Require ($rejected -and (Test-Path -LiteralPath $fixture)) 'Cleanup escaped its cache.'
    $originalSave=(Get-Item -Path function:Save-Current).ScriptBlock
    function Save-Current {throw 'Injected manifest write failure'}
    $rejected=$false;try{Install-Current $fixture $second}catch{$rejected=$true}
    Set-Item -Path function:Save-Current -Value $originalSave
    Require ($rejected -and (Read-Current).commit -eq $first) 'Promotion failure did not roll back.'
    Require ((Get-Content -LiteralPath (Join-Path $cache 'current.json') -Raw) -eq $before) 'Rollback changed the old manifest.'
    Require (@(Get-ChildItem -LiteralPath $cache -Directory -Force | Where-Object {$_.Name -match '^\.(install|previous)-'}).Count -eq 0) 'Rollback left an extra runtime.'
    & (Join-Path $PSScriptRoot 'Launch Main.ps1') -NoLaunch -Offline -CacheRoot $cache | Out-Null
    Require ((Read-Current).commit -eq $first) 'Offline launch replaced the working build.'
    Set-Content -LiteralPath (Join-Path $fixture 'WalkingHard_Data\Managed\Assembly-CSharp.dll') -Value 'Updated fixture, never executed.'
    $networkState=@{}
    $networkState.zip=Join-Path $testRoot 'update.zip';Compress-Archive -Path (Join-Path $fixture '*') -DestinationPath $networkState.zip
    $networkState.hash=(Get-FileHash -LiteralPath $networkState.zip -Algorithm SHA256).Hash
    $networkState.head=$second;$networkState.relation='behind';$networkState.downloads=0
    function Invoke-RestMethod($Uri,$Headers,$TimeoutSec){
        if($Uri -like '*/commits/main'){return @{sha=$networkState.head}}
        if($Uri -like '*/compare/*'){return @{status=$networkState.relation}}
        if($Uri -like '*/releases/latest'){return @{assets=@(@{name='walkinghard-main.json';browser_download_url='https://fixture/manifest'},@{name='WalkingHard-Windows.zip';browser_download_url='https://github.com/Chillysoop/walkinghard/releases/download/test/WalkingHard-Windows.zip'})}}
        if($Uri -eq 'https://fixture/manifest'){return @{commit=$networkState.head;sha256=$networkState.hash}}
        throw 'Unexpected test request.'
    }
    function Invoke-WebRequest($Uri,$OutFile,$TimeoutSec,[switch]$UseBasicParsing){$networkState.downloads++;Copy-Item -LiteralPath $networkState.zip -Destination $OutFile}
    foreach($relation in @('behind','diverged')){
        $networkState.relation=$relation
        & (Join-Path $PSScriptRoot 'Launch Main.ps1') -NoLaunch -CacheRoot $cache | Out-Null
        Require ((Read-Current).commit -eq $first -and $networkState.downloads -eq 0) 'An older or divergent main replaced the newer local build.'
    }
    $networkState.relation='ahead'
    & (Join-Path $PSScriptRoot 'Launch Main.ps1') -NoLaunch -CacheRoot $cache | Out-Null
    Require ((Read-Current).commit -eq $second -and $networkState.downloads -eq 1) 'A forward update was not installed.'
    Require ((Get-Content -LiteralPath (Join-Path $cache 'Current\WalkingHard_Data\Managed\Assembly-CSharp.dll') -Raw) -match 'Updated fixture') 'Updated runtime bytes are missing.'
    Require (@(Get-ChildItem -LiteralPath $cache -Filter WalkingHard.exe -File -Recurse).Count -eq 1) 'Successful update retained extra player executables.'
    $networkState.head='2123456789012345678901234567890123456789';$networkState.hash='0'*64
    & (Join-Path $PSScriptRoot 'Launch Main.ps1') -NoLaunch -CacheRoot $cache | Out-Null
    Require ((Read-Current).commit -eq $second) 'Corrupt network update damaged Current.'
    Require (@(Get-ChildItem -LiteralPath $cache -Filter WalkingHard.exe -File -Recurse).Count -eq 1) 'Failed update left an extra player executable.'
    $desktop=Join-Path $testRoot 'Desktop';New-Item -ItemType Directory -Path $desktop | Out-Null
    foreach($name in @('Walking Hard (Main).lnk','VECTOR - Play Prototype.lnk')){Set-Content -LiteralPath (Join-Path $desktop $name) -Value 'Legacy shortcut'}
    & (Join-Path $PSScriptRoot 'Install Launcher.ps1') -InstallRoot $cache -DesktopDirectory $desktop | Out-Null
    Require (@(Get-ChildItem -LiteralPath $desktop -Filter '*.lnk').Count -eq 1) 'Installer left duplicate play shortcuts.'
    $shell=New-Object -ComObject WScript.Shell;$shortcut=$shell.CreateShortcut((Join-Path $desktop 'Walking Hard.lnk'))
    Require ($shortcut.Arguments -like '*Launch Main.ps1*' -and $shortcut.Arguments -like '*-CacheRoot*') 'Desktop shortcut bypasses the updater.'
    Write-Output ('Launcher checks passed: '+$checks+' assertions covering updates, downgrade protection, rollback, containment, cleanup, offline play and the single shortcut.')
} finally {
    $resolved=[IO.Path]::GetFullPath($testRoot);$temp=[IO.Path]::GetFullPath([IO.Path]::GetTempPath()).TrimEnd('\')+'\'
    if(-not $resolved.StartsWith($temp,[StringComparison]::OrdinalIgnoreCase) -or (Split-Path -Leaf $resolved) -notlike 'WalkingHardLauncherTest-*'){throw 'Unsafe test cleanup path.'}
    if(Test-Path -LiteralPath $resolved){Remove-Item -LiteralPath $resolved -Recurse -Force}
}
