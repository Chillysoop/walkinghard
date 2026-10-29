param(
    [string]$Player=(Join-Path $PSScriptRoot 'Build\Windows\WalkingHard.exe'),
    [ValidateRange(1,1000000)][int]$Runs=1000,
    [ValidateRange(1,16)][int]$Workers=2,
    [ValidateRange(0,2000000000)][int]$Seed=1,
    [ValidateSet('mixed','random','balanced','pinball','storm','sever','gun')][string]$Policy='mixed',
    [ValidateSet(3,8)][int]$Waves=8,
    [ValidateRange(120,600)][int]$MaxCombatSeconds=480,
    [string]$BalanceFile,
    [ValidateRange(60,3600)][int]$GameTimeoutSeconds=600,
    [string]$OutputDirectory=(Join-Path $PSScriptRoot ('QA\Simulations\batch-'+[DateTime]::UtcNow.ToString('yyyyMMdd-HHmmss')+'-'+[Guid]::NewGuid().ToString('N').Substring(0,6)))
)
$ErrorActionPreference='Stop'
$playerPath=(Resolve-Path -LiteralPath $Player).Path
if([long]$Seed+$Runs-1 -gt [int]::MaxValue){throw 'The requested seed range exceeds Int32. Choose a smaller first seed.'}
$capabilities=Join-Path ([IO.Path]::GetDirectoryName($playerPath)) 'simulation-capabilities.json'
if(-not (Test-Path -LiteralPath $capabilities)){throw 'This player has no simulation support manifest. Build the current source with Build Windows.ps1, then point -Player to that build.'}
if((Get-Content -LiteralPath $capabilities -Raw | ConvertFrom-Json).schema -ne 1){throw 'Unsupported simulation runner schema.'}
if(Test-Path -LiteralPath $OutputDirectory){throw 'Choose a new output directory; batches are never silently mixed or overwritten.'}
$Workers=[Math]::Min($Workers,$Runs)
$null=New-Item -ItemType Directory -Path $OutputDirectory
$OutputDirectory=(Resolve-Path -LiteralPath $OutputDirectory).Path
$assembly=Join-Path ([IO.Path]::GetDirectoryName($playerPath)) 'WalkingHard_Data\Managed\Assembly-CSharp.dll'
@{runs=$Runs;workers=$Workers;firstSeed=$Seed;policy=$Policy;waves=$Waves;maxCombatSeconds=$MaxCombatSeconds;startedUtc=[DateTime]::UtcNow.ToString('o');assemblySha256=(Get-FileHash -LiteralPath $assembly -Algorithm SHA256).Hash;player=$playerPath} | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $OutputDirectory 'batch.json') -Encoding UTF8
if($BalanceFile){$balanceSource=(Resolve-Path -LiteralPath $BalanceFile).Path;Copy-Item -LiteralPath $balanceSource -Destination (Join-Path $OutputDirectory 'balance.json')}
$processes=@();$outputs=@();$timer=[Diagnostics.Stopwatch]::StartNew();$failed=$false;$nextProgress=30
try {
    for($worker=0;$worker -lt $Workers;$worker++){
        $count=[int][Math]::Floor(($Runs-1-$worker)/$Workers)+1
        $output=Join-Path $OutputDirectory ('worker-'+$worker+'.jsonl')
        $outputs+=$output
        $log=Join-Path $OutputDirectory ('worker-'+$worker+'.log')
        $arguments='-batchmode -nographics -simRuns '+$count+' -seed '+($Seed+$worker)+' -simStride '+$Workers+' -simPolicy '+$Policy+' -simWaves '+$Waves+' -simMaxSeconds '+$MaxCombatSeconds+' -simOutput "'+$output+'" -logFile "'+$log+'"'
        if($BalanceFile){$arguments+=' -simBalance "'+(Join-Path $OutputDirectory 'balance.json')+'"'}
        $processes+=Start-Process -FilePath $playerPath -ArgumentList $arguments -WindowStyle Hidden -PassThru
    }
    Write-Host ('Running '+$Runs+' real-engine games across '+$Workers+' workers. Results stream to '+$OutputDirectory)
    do {
        $running=$false
        for($i=0;$i -lt $processes.Count;$i++){
            $process=$processes[$i]
            if(-not $process.WaitForExit(250)){
                $running=$true
                $lastProgress=if(Test-Path -LiteralPath $outputs[$i]){(Get-Item -LiteralPath $outputs[$i]).LastWriteTimeUtc}else{$process.StartTime.ToUniversalTime()}
                if(([DateTime]::UtcNow-$lastProgress).TotalSeconds -gt $GameTimeoutSeconds){throw ('Worker '+$process.Id+' produced no completed game for '+$GameTimeoutSeconds+' seconds. Partial results and logs are preserved.')}
            }
        }
        if($timer.Elapsed.TotalSeconds -ge $nextProgress){$nextProgress+=30;Write-Host ('Batch running for '+[Math]::Round($timer.Elapsed.TotalSeconds)+'s. Inspect live results with Summarize Simulations.ps1 -Directory "'+$OutputDirectory+'"')}
    }while($running)
    foreach($process in $processes){if($process.ExitCode -ne 0){$failed=$true;Write-Warning ('Worker '+$process.Id+' exited '+$process.ExitCode+'. Partial results remain available.')}}
} finally {
    foreach($process in $processes){if(-not $process.HasExited){$process.Kill()}}
    $timer.Stop()
    $batchPath=Join-Path $OutputDirectory 'batch.json'
    $batch=Get-Content -LiteralPath $batchPath -Raw | ConvertFrom-Json
    $batch | Add-Member -NotePropertyName completedUtc -NotePropertyValue ([DateTime]::UtcNow.ToString('o')) -Force
    $batch | ConvertTo-Json | Set-Content -LiteralPath $batchPath -Encoding UTF8
}
& (Join-Path $PSScriptRoot 'Summarize Simulations.ps1') -Directory $OutputDirectory -BatchSeconds $timer.Elapsed.TotalSeconds
if($failed){throw 'A simulation worker failed. Inspect worker logs and invalid rows; losses alone are not failures.'}
$summary=Get-Content -LiteralPath (Join-Path $OutputDirectory 'summary.json') -Raw | ConvertFrom-Json
if($summary.missing -ne 0 -or $summary.duplicateSeeds -ne 0 -or $summary.invalid -ne 0){throw 'Batch integrity failed: missing, duplicate, or invalid games.'}
Write-Output $OutputDirectory
