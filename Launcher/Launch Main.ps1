param([switch]$NoLaunch, [switch]$Offline, [string]$CacheRoot = (Join-Path $env:LOCALAPPDATA 'WalkingHard'), [string]$UnityEditor, [switch]$FunctionsOnly)
$ErrorActionPreference = 'Stop'
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
$repository = 'Chillysoop/walkinghard'
$api = 'https://api.github.com/repos/' + $repository
$headers = @{ 'User-Agent'='WalkingHard-Main-Launcher'; Accept='application/vnd.github+json' }
function Set-Status([string]$Message) {
    Write-Host $Message
    if ($script:statusLabel) { $script:statusLabel.Text=$Message; [Windows.Forms.Application]::DoEvents() }
    if (Test-Path -LiteralPath $CacheRoot) { Add-Content -LiteralPath (Join-Path $CacheRoot 'launcher.log') -Value ((Get-Date -Format s) + ' ' + $Message) }
}
function Assert-Player([string]$Folder) {
    foreach ($file in @('WalkingHard.exe','UnityPlayer.dll','WalkingHard_Data\globalgamemanagers')) {
        if (-not (Test-Path -LiteralPath (Join-Path $Folder $file) -PathType Leaf)) { throw ('Incomplete game build: ' + $file) }
    }
}
function Expand-VerifiedBuild([string]$Zip, [string]$Hash, [string]$Destination) {
    if ($Hash -notmatch '^[a-fA-F0-9]{64}$' -or (Get-FileHash -LiteralPath $Zip -Algorithm SHA256).Hash -ne $Hash) { throw 'Download checksum does not match the release manifest.' }
    if (Test-Path -LiteralPath $Destination) { throw 'Build staging directory already exists.' }
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $archive = [IO.Compression.ZipFile]::OpenRead($Zip)
    try {
        $prefix = [IO.Path]::GetFullPath($Destination).TrimEnd('\') + '\'
        foreach ($entry in $archive.Entries) {
            $resolved = [IO.Path]::GetFullPath((Join-Path $Destination $entry.FullName))
            if (-not $resolved.StartsWith($prefix,[StringComparison]::OrdinalIgnoreCase)) { throw 'Archive contains a path outside the build directory.' }
        }
    } finally { $archive.Dispose() }
    [IO.Compression.ZipFile]::ExtractToDirectory($Zip,$Destination)
    Assert-Player $Destination
}
function Save-Current([string]$Folder,[string]$Commit) {
    Assert-Player $Folder
    if ($Commit -notmatch '^[a-f0-9]{40}$') { throw 'Invalid source commit.' }
    $record = @{commit=$Commit;folder=[IO.Path]::GetFullPath($Folder);installedAt=(Get-Date -Format s)}
    $temporary = Join-Path $CacheRoot ('current-' + [Guid]::NewGuid().ToString('N') + '.tmp')
    $record | ConvertTo-Json | Set-Content -LiteralPath $temporary -Encoding UTF8
    Move-Item -LiteralPath $temporary -Destination (Join-Path $CacheRoot 'current.json') -Force
    return [pscustomobject]$record
}
function Read-Current {
    $path=Join-Path $CacheRoot 'current.json'
    if (-not (Test-Path -LiteralPath $path)) { return $null }
    $record=Get-Content -LiteralPath $path -Raw | ConvertFrom-Json
    $root=[IO.Path]::GetFullPath($CacheRoot).TrimEnd('\')+'\'
    if (-not ([IO.Path]::GetFullPath($record.folder)).StartsWith($root,[StringComparison]::OrdinalIgnoreCase)) { throw 'Cached build is outside the launcher directory.' }
    Assert-Player $record.folder
    return $record
}
function Get-Unity([string]$Source) {
    $version=(Get-Content -LiteralPath (Join-Path $Source 'ProjectSettings\ProjectVersion.txt') | Select-String '^m_EditorVersion: (.+)$').Matches.Groups[1].Value.Trim()
    if ($UnityEditor -and (Test-Path -LiteralPath $UnityEditor)) { return $UnityEditor }
    $candidate=Join-Path $env:ProgramFiles ('Unity\Hub\Editor\'+$version+'\Editor\Unity.exe')
    if (Test-Path -LiteralPath $candidate) { return $candidate }
    throw ('Main has newer source but no matching Windows release yet. Unity '+$version+' is required to build it locally.')
}
function Build-Commit([string]$Commit) {
    if (-not (Get-Command git -ErrorAction SilentlyContinue)) { throw 'Main has newer source; install Git and Unity to build before a Windows release is published.' }
    $source=Join-Path $CacheRoot ('Sources\'+$Commit+'-'+[Guid]::NewGuid().ToString('N').Substring(0,8))
    Set-Status 'Main has new source. Downloading an isolated copy to build locally...'
    $gitOutput=& git -c credential.interactive=never clone --quiet --depth 1 --branch main ('https://github.com/'+$repository+'.git') $source 2>&1
    if ($LASTEXITCODE -ne 0) { throw 'Could not download main source.' }
    $actual=(& git -C $source rev-parse HEAD).Trim()
    if ($actual -ne $Commit) {
        $gitOutput=& git -C $source -c credential.interactive=never fetch --depth 1 origin $Commit 2>&1
        if ($LASTEXITCODE -ne 0) { throw 'Could not fetch the requested main commit.' }
        $gitOutput=& git -C $source checkout --detach $Commit 2>&1
        if ($LASTEXITCODE -ne 0) { throw 'Could not check out the requested main commit.' }
    }
    $editor=Get-Unity $source
    Set-Status 'Building the latest main in Unity. The first build can take several minutes...'
    & (Join-Path $source 'Build Windows.ps1') -UnityEditor $editor | Out-Null
    $folder=Join-Path $source 'Build\Windows'
    Assert-Player $folder
    return $folder
}
if ($FunctionsOnly) { return }
$script:statusLabel=$null;$form=$null;$lock=$null;$current=$null;$notice=$null
try {
    New-Item -ItemType Directory -Path $CacheRoot -Force | Out-Null
    $lock=[IO.File]::Open((Join-Path $CacheRoot 'launch.lock'),[IO.FileMode]::OpenOrCreate,[IO.FileAccess]::ReadWrite,[IO.FileShare]::None)
    if (-not $NoLaunch) {
        Add-Type -AssemblyName System.Windows.Forms
        $form=New-Object Windows.Forms.Form
        $form.Text='Walking Hard — Main';$form.Size=New-Object Drawing.Size(490,160);$form.StartPosition='CenterScreen';$form.FormBorderStyle='FixedDialog';$form.MaximizeBox=$false;$form.ControlBox=$false
        $script:statusLabel=New-Object Windows.Forms.Label
        $script:statusLabel.Dock='Fill';$script:statusLabel.Padding=New-Object Windows.Forms.Padding(20);$script:statusLabel.Font=New-Object Drawing.Font('Segoe UI',11)
        $form.Controls.Add($script:statusLabel);$form.Show()
    }
    $current=Read-Current
    try {
        if ($Offline) { throw 'Offline mode requested.' }
        Set-Status 'Checking the main branch...'
        $head=(Invoke-RestMethod ($api+'/commits/main') -Headers $headers -TimeoutSec 20).sha
        if ($head -notmatch '^[a-f0-9]{40}$') { throw 'GitHub returned an invalid main commit.' }
        if (-not $current -or $current.commit -ne $head) {
            $release=$null;$manifest=$null
            try {
                $release=Invoke-RestMethod ($api+'/releases/latest') -Headers $headers -TimeoutSec 20
                $asset=$release.assets | Where-Object name -eq 'walkinghard-main.json' | Select-Object -First 1
                if ($asset) { $manifest=Invoke-RestMethod $asset.browser_download_url -Headers @{'User-Agent'='WalkingHard-Main-Launcher'} -TimeoutSec 20 }
            } catch { Set-Status 'No matching published build found; checking local build support...' }
            if ($manifest -and $manifest.commit -eq $head) {
                $zipAsset=$release.assets | Where-Object name -eq 'WalkingHard-Windows.zip' | Select-Object -First 1
                if (-not $zipAsset -or -not $zipAsset.browser_download_url.StartsWith('https://github.com/'+$repository+'/releases/download/')) { throw 'Release is missing its Windows archive.' }
                Set-Status 'Downloading the latest main build...'
                $downloads=Join-Path $CacheRoot 'Downloads';New-Item -ItemType Directory -Path $downloads -Force | Out-Null
                $zip=Join-Path $downloads ($head+'-'+[Guid]::NewGuid().ToString('N')+'.zip')
                Invoke-WebRequest $zipAsset.browser_download_url -OutFile $zip -UseBasicParsing -TimeoutSec 180
                $folder=Join-Path $CacheRoot ('Versions\'+$head+'-'+[Guid]::NewGuid().ToString('N').Substring(0,8))
                Expand-VerifiedBuild $zip $manifest.sha256 $folder
            } else { $folder=Build-Commit $head }
            $current=Save-Current $folder $head
            Set-Status ('Updated to main '+$head.Substring(0,8)+'.')
        } else { Set-Status ('Already on main '+$head.Substring(0,8)+'.') }
    } catch {
        if (-not $current) { throw }
        $notice=$_.Exception.Message
        Set-Status ('Update unavailable. Keeping the last working build. '+$notice)
    }
    if ($NoLaunch) { $current | ConvertTo-Json -Compress }
    else {
        $form.Close();$form=$null;$script:statusLabel=$null
        if ($notice -and -not $Offline) { [Windows.Forms.MessageBox]::Show(('Launching your last working build. '+$notice),'Walking Hard update') | Out-Null }
        Start-Process -FilePath (Join-Path $current.folder 'WalkingHard.exe') -WorkingDirectory $current.folder
    }
} catch {
    if ($NoLaunch) { throw }
    Add-Type -AssemblyName System.Windows.Forms
    [Windows.Forms.MessageBox]::Show($_.Exception.Message,'Walking Hard could not launch') | Out-Null
} finally {
    if ($form) { $form.Close() }
    if ($lock) { $lock.Dispose() }
}
