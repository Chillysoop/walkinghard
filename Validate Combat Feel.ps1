param(
    [string]$Player=(Join-Path $PSScriptRoot 'Build\Windows\WalkingHard.exe'),
    [Parameter(Mandatory=$true)][string]$OutputDirectory,
    [ValidateSet('Fixtures','Journey')][string]$Mode='Fixtures',
    [int]$Seed=4319,
    [ValidateSet('pinball','storm','sever','gun','cutcarry','close','siphon')][string]$Policy='pinball',
    [switch]$OriginalBodyScan,
    [switch]$VerifyWalkingSnapshot,
    [switch]$Unpooled
)
$ErrorActionPreference='Stop'
$Player=(Resolve-Path -LiteralPath $Player).Path
$OutputDirectory=[IO.Path]::GetFullPath($OutputDirectory)
if(Test-Path -LiteralPath $OutputDirectory){throw 'Choose a fresh output directory.'}
New-Item -ItemType Directory -Path $OutputDirectory | Out-Null
$assembly=Join-Path (Split-Path -Parent $Player) 'WalkingHard_Data\Managed\Assembly-CSharp.dll'
@{mode=$Mode;seed=$(if($Mode -eq 'Fixtures'){1741}else{$Seed});policy=$Policy;unpooled=[bool]$Unpooled;originalBodyScan=[bool]$OriginalBodyScan;verifyWalkingSnapshot=[bool]$VerifyWalkingSnapshot;assemblySha256=(Get-FileHash -LiteralPath $assembly -Algorithm SHA256).Hash} | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $OutputDirectory 'manifest.json')
$arguments='-combatFeelReview -seed '+$Seed+' -journeyPolicy '+$Policy+' -saveDirectory "'+$OutputDirectory+'\profile" -reviewOutput "'+$OutputDirectory+'" -force-d3d11 -screen-fullscreen 0 -logFile "'+$OutputDirectory+'\player.log"'
if($Mode -eq 'Journey'){$arguments+=' -feelJourney'}
if($Unpooled){$arguments+=' -unpooledEffects'}
if($OriginalBodyScan){$arguments+=' -originalBodyScan'}
if($VerifyWalkingSnapshot){$arguments+=' -verifyWalkingSnapshot'}
# Native review windows are intentional: visual acceptance is part of this runner.
$run=Start-Process -FilePath $Player -ArgumentList $arguments -PassThru
try {
    if(-not $run.WaitForExit(1200000)){throw 'Native combat review timed out.'}
    if($run.ExitCode -ne 0){throw "Native combat review failed ($($run.ExitCode)); see $OutputDirectory\player.log"}
    $report=Get-Content -LiteralPath (Join-Path $OutputDirectory 'report.json') -Raw | ConvertFrom-Json
    if(-not $report.passed){throw 'Native combat review did not pass.'}
    $report | Select-Object mode,result,assertions,wave,kills,health,passed | Format-List
} finally {if(-not $run.HasExited){$run.Kill()}}
