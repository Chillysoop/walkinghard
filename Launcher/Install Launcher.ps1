param([string]$InstallRoot=(Join-Path $env:USERPROFILE 'Games\WalkingHard'))
$ErrorActionPreference='Stop'
$launcher=Join-Path $InstallRoot 'Launcher'
New-Item -ItemType Directory -Path $launcher -Force | Out-Null
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'Launch Main.ps1') -Destination (Join-Path $launcher 'Launch Main.ps1') -Force
$desktop=[Environment]::GetFolderPath('Desktop')
$shell=New-Object -ComObject WScript.Shell
$shortcut=$shell.CreateShortcut((Join-Path $desktop 'Walking Hard (Main).lnk'))
$shortcut.TargetPath=Join-Path $env:SystemRoot 'System32\WindowsPowerShell\v1.0\powershell.exe'
$shortcut.Arguments='-NoProfile -ExecutionPolicy Bypass -WindowStyle Hidden -File "'+(Join-Path $launcher 'Launch Main.ps1')+'" -CacheRoot "'+$InstallRoot+'"'
$shortcut.WorkingDirectory=$launcher
$shortcut.Description='Updates from Chillysoop/walkinghard main, then launches. Keeps the last working build if offline.'
$shortcut.WindowStyle=7
$currentPath=Join-Path $InstallRoot 'current.json'
if(Test-Path -LiteralPath $currentPath){
    $current=Get-Content -LiteralPath $currentPath -Raw | ConvertFrom-Json
    $icon=Join-Path $current.folder 'WalkingHard.exe'
    if(Test-Path -LiteralPath $icon){$shortcut.IconLocation=$icon+',0'}
}
$shortcut.Save()
$legacy=Join-Path $desktop 'VECTOR - Play Prototype.lnk'
if(Test-Path -LiteralPath $legacy){Copy-Item -LiteralPath (Join-Path $desktop 'Walking Hard (Main).lnk') -Destination $legacy -Force}
Write-Output (Join-Path $desktop 'Walking Hard (Main).lnk')
