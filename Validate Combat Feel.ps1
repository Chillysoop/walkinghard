param(
    [string]$Player=(Join-Path $PSScriptRoot 'Build\Windows\WalkingHard.exe'),
    [Parameter(Mandatory=$true)][string]$OutputDirectory,
    [ValidateSet('Fixtures','Journey')][string]$Mode='Fixtures',
    [int]$Seed=4319,
    [ValidateSet('pinball','storm','sever','gun','cutcarry','close','siphon')][string]$Policy='pinball',
    [switch]$OriginalBodyScan,
    [switch]$OriginalBodyFilters,
    [switch]$VerifyWalkingSnapshot,
    [switch]$Unpooled,
    [string]$CheckpointProfile
)
$ErrorActionPreference='Stop'
$Player=(Resolve-Path -LiteralPath $Player).Path
$OutputDirectory=[IO.Path]::GetFullPath($OutputDirectory)
if(Test-Path -LiteralPath $OutputDirectory){throw 'Choose a fresh output directory.'}
New-Item -ItemType Directory -Path $OutputDirectory | Out-Null
$checkpointHash=''
if($CheckpointProfile){
    if($Mode -ne 'Journey'){throw 'CheckpointProfile requires Journey mode.'}
    $CheckpointProfile=(Resolve-Path -LiteralPath $CheckpointProfile).Path
    $checkpointHash=(Get-FileHash -LiteralPath (Join-Path $CheckpointProfile 'checkpoint.json') -Algorithm SHA256).Hash
    $profile=Join-Path $OutputDirectory 'profile'
    New-Item -ItemType Directory -Path $profile | Out-Null
    foreach($name in @('checkpoint.json','office.json','archive.json')){
        $file=Join-Path $CheckpointProfile $name
        if(Test-Path -LiteralPath $file){Copy-Item -LiteralPath $file -Destination (Join-Path $profile $name)}
    }
}
$assembly=Join-Path (Split-Path -Parent $Player) 'WalkingHard_Data\Managed\Assembly-CSharp.dll'
@{mode=$Mode;checkpointSha256=$checkpointHash;checkpointProfile=$CheckpointProfile;seed=$(if($Mode -eq 'Fixtures'){1741}else{$Seed});policy=$Policy;unpooled=[bool]$Unpooled;originalBodyScan=[bool]$OriginalBodyScan;originalBodyFilters=[bool]$OriginalBodyFilters;verifyWalkingSnapshot=[bool]$VerifyWalkingSnapshot;physicsSettingsSha256=(Get-FileHash -LiteralPath (Join-Path $PSScriptRoot 'ProjectSettings\DynamicsManager.asset') -Algorithm SHA256).Hash;assemblySha256=(Get-FileHash -LiteralPath $assembly -Algorithm SHA256).Hash} | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $OutputDirectory 'manifest.json')
$arguments='-combatFeelReview -seed '+$Seed+' -journeyPolicy '+$Policy+' -saveDirectory "'+$OutputDirectory+'\profile" -reviewOutput "'+$OutputDirectory+'" -force-d3d11 -screen-fullscreen 0 -logFile "'+$OutputDirectory+'\player.log"'
if($Mode -eq 'Journey'){$arguments+=' -feelJourney'}
if($CheckpointProfile){$arguments+=' -feelResume'}
if($Unpooled){$arguments+=' -unpooledEffects'}
if($OriginalBodyScan){$arguments+=' -originalBodyScan'}
if($OriginalBodyFilters){$arguments+=' -originalBodyFilters'}
if($VerifyWalkingSnapshot){$arguments+=' -verifyWalkingSnapshot'}
# Native review windows are intentional: visual acceptance is part of this runner.
$run=Start-Process -FilePath $Player -ArgumentList $arguments -PassThru
try {
    $timer=[Diagnostics.Stopwatch]::StartNew();$competing=@{};$samples=0
    while(-not $run.WaitForExit(1000)){
        if($timer.Elapsed.TotalSeconds -gt 1200){throw 'Native combat review timed out.'}
        $samples++
        foreach($other in @(Get-Process Unity,WalkingHard -ErrorAction SilentlyContinue | Where-Object {$_.Id -ne $run.Id})){
            $competing[$other.Id]=@{id=$other.Id;path=$other.Path;seenUtc=[DateTime]::UtcNow.ToString('o')}
        }
    }
    @{samples=$samples;seconds=$timer.Elapsed.TotalSeconds;otherProcesses=@($competing.Values)} | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $OutputDirectory 'isolation.json')
    if($Mode -eq 'Journey' -and $competing.Count){throw 'Competing engine workers invalidated performance timing; retain this output and retry when idle.'}
    if($run.ExitCode -ne 0){throw "Native combat review failed ($($run.ExitCode)); see $OutputDirectory\player.log"}
    $report=Get-Content -LiteralPath (Join-Path $OutputDirectory 'report.json') -Raw | ConvertFrom-Json
    if(-not $report.passed){throw 'Native combat review did not pass.'}
    $report | Select-Object mode,result,assertions,wave,kills,health,passed | Format-List
} finally {if(-not $run.HasExited){$run.Kill()}}
