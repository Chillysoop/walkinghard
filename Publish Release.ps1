param([string]$Tag='v0.7.7',[switch]$SkipBuild,[string]$PlayerDirectory=(Join-Path $PSScriptRoot 'Build\Windows'))
$ErrorActionPreference='Stop'
$repo='Chillysoop/walkinghard';$api='https://api.github.com/repos/'+$repo
Push-Location $PSScriptRoot
try {
    if (-not $SkipBuild) { & '.\Build Windows.ps1' -OutputDirectory $PlayerDirectory; & '.\Validate Prototype.ps1' -Player (Join-Path $PlayerDirectory 'WalkingHard.exe') -HeroHealth 5000 }
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
    foreach($doc in @('README.md','DESIGN.md','SIMULATIONS.md','ART_VFX_PLAN.md','Run Simulations.ps1','Summarize Simulations.ps1','Compare Simulations.ps1','balance.example.json')){Copy-Item -LiteralPath (Join-Path $PSScriptRoot $doc) -Destination $stage}
    $qaStage=Join-Path $stage 'QA';New-Item -ItemType Directory -Path $qaStage -Force | Out-Null
    foreach($doc in @('v060-verification.md','v060-native-contactsheet.jpg','v060-followup-cues.jpg','v060-mechanic-motion.jsonl','v060-simulation-summary.json','v060-balance-comparisons.csv','v061-verification.md','v061-fast-comparison.json','v061-full-comparison.json','fast-balance-1000-summary.json','fast-balance-calibration.csv','v062-verification.md','v062-checkpoint.png','v062-rarity-mismatch.png','v062-fusion-preview.png','v062-augment-details.png','v062-icon-key.png','v062-fused.png','v062-combat.png','v063-verification.md','v063-descriptions.png','v063-equipment.png','v063-augments.png','v063-ultimate-draft.png','v063-combat.png','v064-verification.md','v064-balance-review.md','v064-early-wave-summary.json','v065-verification.md','v065-native-contactsheet.jpg','v065-native-motion.jsonl','v065-fast-regression.json','v066-verification.md','v066-balance-review.md','v066-balance-results.json','v066-control-regression.json','v067-verification.md','v067-balance-review.md','v067-balance-results.json','v067-control-regression.json','v067-checkpoint.png','v067-augment-preview.png','v067-combat.png','v068-verification.md','v068-balance-review.md','v068-balance-results.json','v069-verification.md','v069-balance-review.md','v069-balance-results.json','v069-checkpoint.png','v069-wave-details.png','v069-feedback-snapshot.json')){Copy-Item -LiteralPath (Join-Path $PSScriptRoot ('QA\'+$doc)) -Destination $qaStage}
    foreach($doc in @('v070-verification.md','v070-verification.json','v070-balance-review.md','v070-21-route-choice.png','v070-22-route-rewards.png','v070-23-route-enemies.png','v070-24-shield-exposed.png','v070-25-carrier-blast.png')){Copy-Item -LiteralPath (Join-Path $PSScriptRoot ('QA\'+$doc)) -Destination $qaStage}
    foreach($doc in @('v071-verification.md','v071-verification.json','v071-26-finale-stop.png','v071-27-finale-turn.png','v071-28-finale-orbit.png','v071-29-finale-reveal.png','v071-30-finale-fight.png','v071-31-finale-contact.png')){Copy-Item -LiteralPath (Join-Path $PSScriptRoot ('QA\'+$doc)) -Destination $qaStage}
    foreach($doc in @('v072-verification.md','v072-verification.json','v072-balance-review.md','v072-balance-results.json','v072-checkpoint-target-wait.png','v072-target-wait-details.png')){Copy-Item -LiteralPath (Join-Path $PSScriptRoot ('QA\'+$doc)) -Destination $qaStage}
    foreach($doc in @('v073-verification.md','v073-verification.json','v073-gameplay-regression.json')){Copy-Item -LiteralPath (Join-Path $PSScriptRoot ('QA\'+$doc)) -Destination $qaStage}
    Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'QA\v073-ui') -Destination $qaStage -Recurse
    foreach($doc in @('v074-verification.md','v074-verification.json')){Copy-Item -LiteralPath (Join-Path $PSScriptRoot ('QA\'+$doc)) -Destination $qaStage}
    Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'QA\v074-ui') -Destination $qaStage -Recurse
    Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'QA\v075-verification.md') -Destination $qaStage
    Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'QA\v075-ui') -Destination $qaStage -Recurse
    foreach($doc in @('v076-verification.md','v076-balance-review.md','v076-verification.json')){Copy-Item -LiteralPath (Join-Path $PSScriptRoot ('QA\'+$doc)) -Destination $qaStage}
    Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'QA\v076-ui') -Destination $qaStage -Recurse
    foreach($doc in @('v077-verification.md','v077-balance-review.md','v077-verification.json')){Copy-Item -LiteralPath (Join-Path $PSScriptRoot ('QA\'+$doc)) -Destination $qaStage}
    Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'QA\v077-ui') -Destination $qaStage -Recurse
    $balanceStage=Join-Path $qaStage 'Balance';New-Item -ItemType Directory -Path $balanceStage -Force | Out-Null
    Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'QA\Balance\v076') -Destination $balanceStage -Recurse
    Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'QA\Balance\v077') -Destination $balanceStage -Recurse
    $runnerStage=Join-Path $balanceStage 'controlled-siphon';New-Item -ItemType Directory -Path $runnerStage -Force | Out-Null
    Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'QA\Balance\controlled-siphon\run.py') -Destination $runnerStage
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
        $body=@{tag_name=$Tag;target_commitish=$commit;name='Walking Hard '+$Tag+' - Recoil Charge Preservation';draft=$true;prerelease=$false;body="Recoil Harness keeps its stored bonus when the base gun hit already kills the target. A surviving piercing or ricochet target, the other pistol, or a later attack can spend it once. Damage and kills still belong to the armor. Hover rules explain the behavior.`n`nAll item stats remain unchanged. Matched Fast and Full/PhysX comparisons are recorded in QA/v077-balance-review.md; regression and native presentation checks are recorded in QA/v077-verification.md.`n`nExtract the ZIP and run WalkingHard.exe, or use Launcher/Install.cmd for an updating shortcut. Built from $commit."} | ConvertTo-Json
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
