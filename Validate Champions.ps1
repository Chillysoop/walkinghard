param(
    [string]$Player=(Join-Path $PSScriptRoot 'Build\Windows\WalkingHard.exe'),
    [ValidateSet('Mechanics','Rendered','Balance')][string]$Mode='Mechanics',
    [int]$Seed=1741,
    [switch]$OriginalChampions,
    [string]$OutputDirectory=(Join-Path $PSScriptRoot ('QA\champions-'+(Get-Date -Format 'yyyyMMdd-HHmmss')))
)
$ErrorActionPreference='Stop'
$Player=(Resolve-Path -LiteralPath $Player).Path
$OutputDirectory=[IO.Path]::GetFullPath($OutputDirectory)
if(Test-Path -LiteralPath $OutputDirectory){throw 'Use a fresh output directory; previous results must be retained.'}
New-Item -ItemType Directory -Path $OutputDirectory | Out-Null
$assembly=Join-Path (Split-Path -Parent $Player) 'WalkingHard_Data\Managed\Assembly-CSharp.dll'
@{assemblySha256=(Get-FileHash -LiteralPath $assembly -Algorithm SHA256).Hash;mode=$Mode;seed=$Seed;originalChampions=[bool]$OriginalChampions} | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $OutputDirectory 'manifest.json')
$arguments='-championReview -seed '+$Seed+' -saveDirectory "'+$OutputDirectory+'\profile" -reviewOutput "'+$OutputDirectory+'" -logFile "'+$OutputDirectory+'\player.log"'
if($OriginalChampions){$arguments+=' -originalChampions'}
if($Mode -eq 'Balance'){$arguments+=' -championBalance'}
if($Mode -eq 'Rendered'){
    $arguments+=' -force-d3d11 -screen-fullscreen 0'
    $run=Start-Process -FilePath $Player -ArgumentList $arguments -PassThru
}else{
    $arguments+=' -batchmode -nographics'
    $run=Start-Process -FilePath $Player -ArgumentList $arguments -WindowStyle Hidden -PassThru
}
if(-not $run.WaitForExit(1200000)){$run.Kill();throw 'Champion review timed out; retained report and log.'}
if($run.ExitCode -ne 0){throw "Champion review failed with exit $($run.ExitCode). See $OutputDirectory\player.log"}
$report=Get-Content -LiteralPath (Join-Path $OutputDirectory 'report.json') -Raw | ConvertFrom-Json
if(-not $report.passed){throw 'Champion review did not pass.'}
Write-Output "$Mode passed: $($report.assertions) assertions; $($report.cases.Count) fixed-kit encounters. $OutputDirectory"
