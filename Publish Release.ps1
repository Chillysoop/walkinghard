param([string]$Tag='v0.3.1',[switch]$SkipBuild)
$ErrorActionPreference='Stop'
$repo='Chillysoop/walkinghard';$api='https://api.github.com/repos/'+$repo
Push-Location $PSScriptRoot
try {
    if (-not $SkipBuild) { & '.\Build Windows.ps1'; & '.\Validate Prototype.ps1' }
    $commit=(& git rev-parse HEAD).Trim()
    if ((& git status --porcelain).Length -gt 0) { throw 'Commit the source and validation reports before publishing.' }
    $remote=(& git ls-remote origin refs/heads/main) -split '\s+'
    if ($remote[0] -ne $commit) { throw 'Push this exact commit to main before publishing.' }
    $releases=Join-Path $PSScriptRoot 'Releases';New-Item -ItemType Directory -Path $releases -Force | Out-Null
    $stage=Join-Path $releases ('Stage-'+[Guid]::NewGuid().ToString('N'));New-Item -ItemType Directory -Path $stage -Force | Out-Null
    foreach($f in @('WalkingHard.exe','WalkingHard_Data','UnityPlayer.dll','UnityCrashHandler64.exe','MonoBleedingEdge','D3D12')) {
        $source=Join-Path $PSScriptRoot ('Build\Windows\'+$f)
        if(Test-Path -LiteralPath $source){Copy-Item -LiteralPath $source -Destination $stage -Recurse}
    }
    Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'Launcher') -Destination $stage -Recurse
    Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'README.md') -Destination $stage
    $zip=Join-Path $releases 'WalkingHard-Windows.zip'
    Compress-Archive -Path (Join-Path $stage '*') -DestinationPath $zip -Force
    $manifest=Join-Path $releases 'walkinghard-main.json'
    @{version=$Tag.TrimStart('v');commit=$commit;sha256=(Get-FileHash $zip -Algorithm SHA256).Hash.ToLowerInvariant();asset='WalkingHard-Windows.zip'} | ConvertTo-Json | Set-Content -LiteralPath $manifest -Encoding UTF8
    # Credential Manager supplies the existing GitHub login. Never print or persist its token.
    $credentialText="protocol=https`nhost=github.com`n`n" | & git -c credential.interactive=never credential fill
    if($LASTEXITCODE -ne 0){throw 'GitHub credentials unavailable.'}
    $token=($credentialText | Where-Object {$_ -like 'password=*'} | Select-Object -First 1).Substring(9)
    $headers=@{Authorization='Bearer '+$token;'User-Agent'='WalkingHard-Release';Accept='application/vnd.github+json';'X-GitHub-Api-Version'='2022-11-28'}
    $release=$null
    try{$release=Invoke-RestMethod ($api+'/releases/tags/'+[Uri]::EscapeDataString($Tag)) -Headers $headers}catch{if([int]$_.Exception.Response.StatusCode -ne 404){throw}}
    if(-not $release){
        $body=@{tag_name=$Tag;target_commitish=$commit;name='Walking Hard 0.3 - Drafted Sequences';draft=$true;prerelease=$false;body="Draft two opening abilities, place them into a repeating sequence, then draft the third at checkpoint one. Includes physical crowd collisions, ragdolls, concealed doorway/horizon entries, and the Auto Combat toggle on the fight screen. Full equipment inventory, three-copy fusion and severing remain design proposals.`n`nExtract the Windows ZIP, run WalkingHard.exe, or use Launcher/Install.cmd for a main-tracking desktop shortcut. Built from $commit. See README for controls and development."} | ConvertTo-Json
        $release=Invoke-RestMethod ($api+'/releases') -Headers $headers -Method Post -ContentType 'application/json' -Body $body
    }
    foreach($path in @($zip,$manifest)){
        $name=[IO.Path]::GetFileName($path)
        if($release.assets | Where-Object name -eq $name){throw ('Release asset already exists; use a new tag instead of replacing: '+$name)}
        $upload=$release.upload_url -replace '\{.*\}$',''
        $null=Invoke-RestMethod ($upload+'?name='+[Uri]::EscapeDataString($name)) -Headers $headers -Method Post -ContentType 'application/octet-stream' -InFile $path
    }
    $release=Invoke-RestMethod ($api+'/releases/'+$release.id) -Headers $headers -Method Patch -ContentType 'application/json' -Body '{"draft":false}'
    Write-Output $release.html_url
} finally { $token=$null;$credentialText=$null;Pop-Location }
