param([string]$Player=(Join-Path $PSScriptRoot 'Build\Windows\WalkingHard.exe'),[switch]$Rendered)
$ErrorActionPreference='Stop'
$playerPath=(Resolve-Path -LiteralPath $Player).Path
$log=Join-Path $PSScriptRoot ('Logs\tutorial-'+$(if($Rendered){'rendered'}else{'checks'})+'.log')
New-Item -ItemType Directory -Path (Split-Path $log) -Force | Out-Null
$arguments=if($Rendered){'-tutorialReview -force-d3d11 -screen-fullscreen 0 -screen-width 1280 -screen-height 720'}else{'-batchmode -nographics -tutorialChecks'}
$arguments+=' -logFile "'+$log+'"'
if($Rendered){$process=Start-Process -FilePath $playerPath -ArgumentList $arguments -PassThru}
else{$process=Start-Process -FilePath $playerPath -ArgumentList $arguments -WindowStyle Hidden -PassThru}
if(-not $process.WaitForExit(120000)){$process.Kill();throw "Tutorial review timed out. See $log"}
if($process.ExitCode -ne 0){throw "Tutorial review failed. See $log"}
$success=Select-String -LiteralPath $log -Pattern 'TUTORIAL_REVIEW_SUCCESS'
if(-not $success){throw "Tutorial review did not report success. See $log"}
$success.Line
Write-Output (Join-Path (Split-Path $playerPath) 'TutorialReview')
