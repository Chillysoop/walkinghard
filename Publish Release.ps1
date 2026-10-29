param([string]$Tag='v0.6.0',[switch]$SkipBuild,[string]$PlayerDirectory=(Join-Path $PSScriptRoot 'Build\Windows'))
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
    foreach($f in @('WalkingHard.exe','WalkingHard_Data','UnityPlayer.dll','UnityCrashHandler64.exe','MonoBleedingEdge','D3D12','simulation-capabilities.json')) {
        $source=Join-Path $PlayerDirectory $f
        if(Test-Path -LiteralPath $source){Copy-Item -LiteralPath $source -Destination $stage -Recurse}
    }
    Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'Launcher') -Destination $stage -Recurse
    foreach($doc in @('README.md','DESIGN.md','SIMULATIONS.md','Run Simulations.ps1','Summarize Simulations.ps1','Compare Simulations.ps1','balance.example.json')){Copy-Item -LiteralPath (Join-Path $PSScriptRoot $doc) -Destination $stage}
    $qaStage=Join-Path $stage 'QA';New-Item -ItemType Directory -Path $qaStage -Force | Out-Null
    foreach($doc in @('v060-verification.md','v060-native-contactsheet.jpg','v060-followup-cues.jpg','v060-mechanic-motion.jsonl','v060-simulation-summary.json','v060-balance-comparisons.csv')){Copy-Item -LiteralPath (Join-Path $PSScriptRoot ('QA\'+$doc)) -Destination $qaStage}
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
        $body=@{tag_name=$Tag;target_commitish=$commit;name='Walking Hard '+$Tag+' - Build Variety and Headless Balancing';draft=$true;prerelease=$false;body="Forty authored items and sixteen transferable weapon/ability augments expand the five-action loop. Eight waves include formation previews, thirteen item choices, two augment drafts and a Warden encounter. Weapons, armor, rigs and ultimates have visible signature effects; rarity scales their relevant stats and effects. Checkpoints support module attachment, storage and fusion ownership. Recaps report damage, control, absorption and delays. Engine-only batch scripts run seeded games with actual combat and PhysX, numerical tuning files and before/after CSV comparisons. Balance remains provisional; see QA/v060-verification.md for measured checks and limitations.`n`nExtract the Windows ZIP and run WalkingHard.exe, or use Launcher/Install.cmd for the main-tracking shortcut. See SIMULATIONS.md for bulk simulations. Built from $commit."} | ConvertTo-Json
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
