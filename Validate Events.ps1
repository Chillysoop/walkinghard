param(
    [string]$Player=(Join-Path $PSScriptRoot 'Build\Windows\WalkingHard.exe'),
    [ValidateSet('Mechanics','Rendered','Economy','Resume')][string]$Mode='Mechanics',
    [string]$ProfileDirectory='',
    [string]$OutputDirectory=(Join-Path $PSScriptRoot ('QA\events-'+(Get-Date -Format 'yyyyMMdd-HHmmss')))
)
$ErrorActionPreference='Stop'
$Player=(Resolve-Path -LiteralPath $Player).Path
$OutputDirectory=[IO.Path]::GetFullPath($OutputDirectory)
if(Test-Path -LiteralPath $OutputDirectory){throw 'Use a fresh output directory; retain prior results.'}
New-Item -ItemType Directory -Path $OutputDirectory | Out-Null
if(-not $ProfileDirectory){$ProfileDirectory=Join-Path $OutputDirectory 'profile'}
$ProfileDirectory=[IO.Path]::GetFullPath($ProfileDirectory)
$assembly=Join-Path (Split-Path -Parent $Player) 'WalkingHard_Data\Managed\Assembly-CSharp.dll'
@{assemblySha256=(Get-FileHash -LiteralPath $assembly -Algorithm SHA256).Hash;mode=$Mode} | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $OutputDirectory 'manifest.json')
$arguments='-districtEventReview -saveDirectory "'+$ProfileDirectory+'" -reviewOutput "'+$OutputDirectory+'" -logFile "'+$OutputDirectory+'\player.log"'
if($Mode -eq 'Economy'){$arguments+=' -eventEconomy'}
if($Mode -eq 'Resume'){$arguments+=' -eventResumeOnly'}
if($Mode -eq 'Rendered'){
    $arguments+=' -force-d3d11 -screen-fullscreen 0'
    $run=Start-Process -FilePath $Player -ArgumentList $arguments -WindowStyle Normal -PassThru
}else{
    $arguments+=' -batchmode -nographics'
    $run=Start-Process -FilePath $Player -ArgumentList $arguments -WindowStyle Hidden -PassThru
}
if(-not $run.WaitForExit(1200000)){$run.Kill();throw 'Event review timed out; retained report and log.'}
if($run.ExitCode -ne 0){throw "Event review failed with exit $($run.ExitCode). See $OutputDirectory\player.log"}
$report=Get-Content -LiteralPath (Join-Path $OutputDirectory 'report.json') -Raw | ConvertFrom-Json
if(-not $report.passed){throw 'Event review did not pass.'}
Write-Output "$Mode passed: $($report.assertions) assertions; $($report.economy.Count) economy cohorts. $OutputDirectory"
