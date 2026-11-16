param(
    [string]$Player=(Join-Path $PSScriptRoot 'Build\Windows\WalkingHard.exe'),
    [Parameter(Mandatory=$true)][string]$OutputDirectory,
    [switch]$Rendered
)
$ErrorActionPreference='Stop'
$Player=(Resolve-Path -LiteralPath $Player).Path
$OutputDirectory=[IO.Path]::GetFullPath($OutputDirectory)
if(Test-Path -LiteralPath $OutputDirectory){throw 'Choose a fresh review directory.'}
New-Item -ItemType Directory -Path $OutputDirectory | Out-Null
$assembly=Join-Path (Split-Path -Parent $Player) 'WalkingHard_Data\Managed\Assembly-CSharp.dll'
@{rendered=[bool]$Rendered;assemblySha256=(Get-FileHash -LiteralPath $assembly -Algorithm SHA256).Hash} | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $OutputDirectory 'manifest.json')
$arguments='-constellationReview -saveDirectory "'+$OutputDirectory+'\profile" -reviewOutput "'+$OutputDirectory+'" -logFile "'+$OutputDirectory+'\player.log"'
if($Rendered){$arguments+=' -force-d3d11 -screen-fullscreen 0';$run=Start-Process -FilePath $Player -ArgumentList $arguments -PassThru}
else{$arguments+=' -batchmode -nographics';$run=Start-Process -FilePath $Player -ArgumentList $arguments -WindowStyle Hidden -PassThru}
try {
    if(-not $run.WaitForExit(600000)){throw 'Constellation review timed out.'}
    if($run.ExitCode -ne 0){throw "Constellation review failed ($($run.ExitCode)); see $OutputDirectory\player.log"}
    $report=Get-Content -LiteralPath (Join-Path $OutputDirectory 'report.json') -Raw | ConvertFrom-Json
    if(-not $report.passed){throw 'Constellation review did not pass.'}
    $report | Select-Object version,assertions,passed | Format-List
} finally {if(-not $run.HasExited){$run.Kill()}}
