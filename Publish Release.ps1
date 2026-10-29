param([string]$Tag='v0.5.1',[switch]$SkipBuild,[string]$PlayerDirectory=(Join-Path $PSScriptRoot 'Build\Windows'))
$ErrorActionPreference='Stop'
$repo='Chillysoop/walkinghard';$api='https://api.github.com/repos/'+$repo
Push-Location $PSScriptRoot
try {
    if (-not $SkipBuild) { & '.\Build Windows.ps1' -OutputDirectory $PlayerDirectory; & '.\Validate Prototype.ps1' -Player (Join-Path $PlayerDirectory 'WalkingHard.exe') }
    $commit=(& git rev-parse HEAD).Trim()
    if ((& git status --porcelain).Length -gt 0) { throw 'Commit the source and validation reports before publishing.' }
    $remote=(& git ls-remote origin refs/heads/main) -split '\s+'
    if ($remote[0] -ne $commit) { throw 'Push this exact commit to main before publishing.' }
    $releases=Join-Path $PSScriptRoot 'Releases';New-Item -ItemType Directory -Path $releases -Force | Out-Null
    $stage=Join-Path $releases ('Stage-'+[Guid]::NewGuid().ToString('N'));New-Item -ItemType Directory -Path $stage -Force | Out-Null
    foreach($f in @('WalkingHard.exe','WalkingHard_Data','UnityPlayer.dll','UnityCrashHandler64.exe','MonoBleedingEdge','D3D12')) {
        $source=Join-Path $PlayerDirectory $f
        if(Test-Path -LiteralPath $source){Copy-Item -LiteralPath $source -Destination $stage -Recurse}
    }
    Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'Launcher') -Destination $stage -Recurse
    foreach($doc in @('README.md','DESIGN.md')){Copy-Item -LiteralPath (Join-Path $PSScriptRoot $doc) -Destination $stage}
    $qaStage=Join-Path $stage 'QA';New-Item -ItemType Directory -Path $qaStage -Force | Out-Null
    foreach($doc in @('v051-verification.md','v051-armed-cast.png','v05-verification.md','v05-checkpoint.png','v05-combat.png','v05-hover.png','v05-fusion.png')){Copy-Item -LiteralPath (Join-Path $PSScriptRoot ('QA\'+$doc)) -Destination $qaStage}
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
        $body=@{tag_name=$Tag;target_commitish=$commit;name='Walking Hard '+$Tag+' - Weapons Stay in Hand';draft=$true;prerelease=$false;body="Equipped weapons stay in their mounted hands through abilities and ultimates, including mixed sword/pistol loadouts. Basic attacks animate and emit effects from their weapon mount even after reordering. Matching pairs and two-handed groups preserve their rules. Existing preparation timing, damage and ultimate scheduling are unchanged. Fourteen standalone scenarios check the gameplay contracts, with per-frame held-model checks and a reversed mixed-weapon fixture. Native Windows visual checks cover armed casting.`n`nExtract the Windows ZIP, run WalkingHard.exe, or use Launcher/Install.cmd for the main-tracking shortcut. Built from $commit. See README and QA/v051-verification.md for details."} | ConvertTo-Json
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
