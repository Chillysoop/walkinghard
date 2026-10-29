param([string]$InstallRoot=(Join-Path $env:LOCALAPPDATA 'WalkingHard'))
$ErrorActionPreference='Stop'
$launcher=Join-Path $InstallRoot 'Launcher'
New-Item -ItemType Directory -Path $launcher -Force | Out-Null
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'Launch Main.ps1') -Destination (Join-Path $launcher 'Launch Main.ps1') -Force
$desktop=[Environment]::GetFolderPath('Desktop')
$shell=New-Object -ComObject WScript.Shell
$shortcut=$shell.CreateShortcut((Join-Path $desktop 'Walking Hard (Main).lnk'))
$shortcut.TargetPath=Join-Path $env:SystemRoot 'System32\WindowsPowerShell\v1.0\powershell.exe'
$shortcut.Arguments='-NoProfile -ExecutionPolicy Bypass -WindowStyle Hidden -File "'+(Join-Path $launcher 'Launch Main.ps1')+'"'
$shortcut.WorkingDirectory=$launcher
$shortcut.Description='Updates from Chillysoop/walkinghard main, then launches. Keeps the last working build if offline.'
$shortcut.WindowStyle=7
$shortcut.Save()
Write-Output (Join-Path $desktop 'Walking Hard (Main).lnk')
