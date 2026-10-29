param([string]$UnityEditor = 'C:\Program Files\Unity\Hub\Editor\6000.3.27f1\Editor\Unity.exe', [string]$OutputDirectory = (Join-Path $PSScriptRoot 'Build\Windows'))
$ErrorActionPreference = 'Stop'
if (-not (Test-Path -LiteralPath $UnityEditor)) { throw 'Set -UnityEditor to the path of Unity 6000.3.27f1 or a compatible Unity 6.3 editor.' }
New-Item -ItemType Directory -Path (Join-Path $PSScriptRoot 'Logs') -Force | Out-Null
$arguments = '-batchmode -nographics -quit -projectPath "' + $PSScriptRoot + '" -executeMethod VectorGame.Editor.PrototypeBuilder.BuildWindows -walkingHardOutput "' + $OutputDirectory + '" -buildTarget StandaloneWindows64 -logFile "' + (Join-Path $PSScriptRoot 'Logs\build.log') + '"'
$buildProcess = Start-Process -FilePath $UnityEditor -ArgumentList $arguments -WindowStyle Hidden -PassThru -Wait
if ($buildProcess.ExitCode -ne 0) { throw ('Unity build failed. See ' + (Join-Path $PSScriptRoot 'Logs\build.log')) }
Write-Output (Join-Path $OutputDirectory 'WalkingHard.exe')
