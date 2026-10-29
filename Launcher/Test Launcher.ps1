$ErrorActionPreference='Stop'
$testRoot=Join-Path ([IO.Path]::GetTempPath()) ('WalkingHardLauncherTest-'+[Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $testRoot -Force | Out-Null
. (Join-Path $PSScriptRoot 'Launch Main.ps1') -FunctionsOnly -CacheRoot $testRoot
$fixture=Join-Path $testRoot 'fixture';New-Item -ItemType Directory -Path (Join-Path $fixture 'WalkingHard_Data') -Force | Out-Null
foreach($f in @('WalkingHard.exe','UnityPlayer.dll','WalkingHard_Data\globalgamemanagers')){Set-Content -LiteralPath (Join-Path $fixture $f) -Value 'Test fixture, never executed.'}
$zip=Join-Path $testRoot 'valid.zip';Compress-Archive -Path (Join-Path $fixture '*') -DestinationPath $zip
$hash=(Get-FileHash -LiteralPath $zip -Algorithm SHA256).Hash
$destination=Join-Path $testRoot 'installed'
Expand-VerifiedBuild $zip $hash $destination
$commit='0123456789012345678901234567890123456789'
$saved=Save-Current $destination $commit
if((Read-Current).commit -ne $commit){throw 'Current build was not saved.'}
$before=Get-Content -LiteralPath (Join-Path $testRoot 'current.json') -Raw
$rejected=$false;try{Expand-VerifiedBuild $zip ('0'*64) (Join-Path $testRoot 'bad')}catch{$rejected=$true}
if(-not $rejected){throw 'Corrupt checksum was accepted.'}
$rejected=$false;try{Save-Current (Join-Path $testRoot 'missing') $commit}catch{$rejected=$true}
if(-not $rejected){throw 'Failed build was promoted.'}
if((Get-Content -LiteralPath (Join-Path $testRoot 'current.json') -Raw) -ne $before){throw 'Failure damaged the working installation.'}
$malicious=Join-Path $testRoot 'unsafe.zip'
$archive=[IO.Compression.ZipFile]::Open($malicious,[IO.Compression.ZipArchiveMode]::Create)
try{$entry=$archive.CreateEntry('../outside.txt');$stream=$entry.Open();$stream.Dispose()}finally{$archive.Dispose()}
$rejected=$false;try{Expand-VerifiedBuild $malicious (Get-FileHash $malicious).Hash (Join-Path $testRoot 'unsafe')}catch{$rejected=$true}
if(-not $rejected -or (Test-Path -LiteralPath (Join-Path $testRoot 'outside.txt'))){throw 'Unsafe archive path was accepted.'}
& (Join-Path $PSScriptRoot 'Launch Main.ps1') -NoLaunch -Offline -CacheRoot $testRoot | Out-Null
Write-Output 'Launcher checks passed: valid install, checksum rejection, failed-build rollback, archive containment, offline fallback.'
