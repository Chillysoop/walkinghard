param(
    [Parameter(Mandatory=$true)][string]$PlayerDirectory,
    [Parameter(Mandatory=$true)][string]$Commit,
    [Parameter(Mandatory=$true)][string]$AssemblySha256,
    [string]$InstallRoot=(Join-Path $env:USERPROFILE 'Games\WalkingHard'),
    [switch]$RemoveSourcePlayer
)
$ErrorActionPreference='Stop'
$source=(Resolve-Path -LiteralPath $PlayerDirectory).Path
$repository=Split-Path -Parent $PSScriptRoot
$buildRoot=[IO.Path]::GetFullPath((Join-Path $repository 'Build')).TrimEnd('\')+'\'
if($RemoveSourcePlayer -and -not $source.StartsWith($buildRoot,[StringComparison]::OrdinalIgnoreCase)){throw 'Source cleanup is limited to player folders inside this repository Build directory.'}
. (Join-Path $PSScriptRoot 'Launch Main.ps1') -FunctionsOnly -CacheRoot $InstallRoot
Assert-Player $source
$assembly=Join-Path $source 'WalkingHard_Data\Managed\Assembly-CSharp.dll'
if($AssemblySha256 -notmatch '^[a-fA-F0-9]{64}$' -or (Get-FileHash -LiteralPath $assembly -Algorithm SHA256).Hash -ne $AssemblySha256){throw 'Player assembly does not match the verified build.'}
New-Item -ItemType Directory -Path $InstallRoot -Force | Out-Null
$lock=[IO.File]::Open((Join-Path $InstallRoot 'launch.lock'),[IO.FileMode]::OpenOrCreate,[IO.FileAccess]::ReadWrite,[IO.FileShare]::None)
try {
    $current=Install-Current $source $Commit
    if((Get-FileHash -LiteralPath (Join-Path $current.folder 'WalkingHard_Data\Managed\Assembly-CSharp.dll') -Algorithm SHA256).Hash -ne $AssemblySha256){throw 'Installed assembly failed verification.'}
    & (Join-Path $PSScriptRoot 'Install Launcher.ps1') -InstallRoot $InstallRoot
    if($RemoveSourcePlayer){
        foreach($name in @('WalkingHard.exe','WalkingHard_Data','UnityPlayer.dll','UnityCrashHandler64.exe','MonoBleedingEdge','D3D12','simulation-capabilities.json','Walking Hard_BurstDebugInformation_DoNotShip')){
            $target=[IO.Path]::GetFullPath((Join-Path $source $name))
            if(-not $target.StartsWith($buildRoot,[StringComparison]::OrdinalIgnoreCase) -or -not $target.StartsWith($source.TrimEnd('\')+'\',[StringComparison]::OrdinalIgnoreCase)){throw 'Unsafe source cleanup path.'}
            if(Test-Path -LiteralPath $target){
                $items=@(Get-Item -LiteralPath $target -Force)+@(Get-ChildItem -LiteralPath $target -Recurse -Force -ErrorAction SilentlyContinue)
                if($items | Where-Object {$_.Attributes -band [IO.FileAttributes]::ReparsePoint}){throw 'Source cleanup cannot follow directory links.'}
                Remove-Item -LiteralPath $target -Recurse -Force
            }
        }
    }
    $current | ConvertTo-Json
} finally { $lock.Dispose() }
