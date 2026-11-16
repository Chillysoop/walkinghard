param([string]$InstallRoot=(Join-Path $env:USERPROFILE 'Games\WalkingHard'),[string]$DesktopDirectory=[Environment]::GetFolderPath('Desktop'))
$ErrorActionPreference='Stop'
$launcher=Join-Path $InstallRoot 'Launcher'
New-Item -ItemType Directory -Path $launcher -Force | Out-Null
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'Launch Main.ps1') -Destination (Join-Path $launcher 'Launch Main.ps1') -Force
$desktop=$DesktopDirectory
$shell=New-Object -ComObject WScript.Shell
$shortcut=$shell.CreateShortcut((Join-Path $desktop 'Walking Hard.lnk'))
$shortcut.TargetPath=Join-Path $env:SystemRoot 'System32\WindowsPowerShell\v1.0\powershell.exe'
$shortcut.Arguments='-NoProfile -ExecutionPolicy Bypass -WindowStyle Hidden -File "'+(Join-Path $launcher 'Launch Main.ps1')+'" -CacheRoot "'+$InstallRoot+'"'
$shortcut.WorkingDirectory=$launcher
$shortcut.Description='Checks for updates, then plays the latest installed Walking Hard. Keeps newer local builds and works offline.'
$shortcut.WindowStyle=7
$currentPath=Join-Path $InstallRoot 'current.json'
if(Test-Path -LiteralPath $currentPath){
    $current=Get-Content -LiteralPath $currentPath -Raw | ConvertFrom-Json
    $icon=Join-Path $current.folder 'WalkingHard.exe'
    if(Test-Path -LiteralPath $icon){$shortcut.IconLocation=$icon+',0'}
}
$shortcut.Save()
foreach($name in @('Walking Hard (Main).lnk','VECTOR - Play Prototype.lnk')) {
    $legacy=Join-Path $desktop $name
    if(Test-Path -LiteralPath $legacy){Remove-Item -LiteralPath $legacy -Force}
}
Write-Output (Join-Path $desktop 'Walking Hard.lnk')
