param(
    [Parameter(Mandatory=$true)][string]$Baseline,
    [Parameter(Mandatory=$true)][string]$Candidate,
    [string]$OutputDirectory
)
$ErrorActionPreference='Stop'
$a=Get-Content -LiteralPath (Join-Path $Baseline 'batch.json') -Raw | ConvertFrom-Json
$b=Get-Content -LiteralPath (Join-Path $Candidate 'batch.json') -Raw | ConvertFrom-Json
$baselineMode=if($a.mode){$a.mode}else{'Full'}
$candidateMode=if($b.mode){$b.mode}else{'Full'}
if($baselineMode -ne $candidateMode){throw 'Balance comparisons require the same simulation mode. Full/Fast differences measure model error, not a tuning effect.'}
foreach($field in @('runs','workers','firstSeed','policy','waves','maxCombatSeconds')){if($a.$field -ne $b.$field){throw ('Comparison requires matching '+$field+'. Use the same seed set, workers, policy and wave count.')}}
& (Join-Path $PSScriptRoot 'Summarize Simulations.ps1') -Directory $Baseline | Out-Null
& (Join-Path $PSScriptRoot 'Summarize Simulations.ps1') -Directory $Candidate | Out-Null
foreach($directory in @($Baseline,$Candidate)){
    $summary=Get-Content -LiteralPath (Join-Path $directory 'summary.json') -Raw | ConvertFrom-Json
    if($summary.missing -ne 0 -or $summary.invalid -ne 0 -or $summary.duplicateSeeds -ne 0){throw 'Compare complete, valid batches with unique seeds.'}
}
$left=Import-Csv -LiteralPath (Join-Path $Baseline 'games.csv')
$right=@{};Import-Csv -LiteralPath (Join-Path $Candidate 'games.csv') | ForEach-Object {$right[$_.seed]=$_}
$pairs=@(foreach($game in $left){
    $other=$right[$game.seed]
    if(-not $other -or $other.policy -ne $game.policy){throw ('Missing matching seed/policy: '+$game.seed)}
    [pscustomobject]@{seed=$game.seed;policy=$game.policy;baselineResult=$game.result;candidateResult=$other.result;winDelta=([int]($other.result -eq 'Complete')-[int]($game.result -eq 'Complete'));waveDelta=([double]$other.waveReached-[double]$game.waveReached);killDelta=([double]$other.kills-[double]$game.kills);scoreDelta=([double]$other.score-[double]$game.score);healthDelta=([double]$other.health-[double]$game.health)}
})
$comparison=@($pairs | Group-Object policy | ForEach-Object {
    $g=@($_.Group)
    [pscustomobject]@{policy=$_.Name;matchedSeeds=$g.Count;changedLossToWin=@($g | Where-Object winDelta -eq 1).Count;changedWinToLoss=@($g | Where-Object winDelta -eq -1).Count;winRateDeltaPoints=[Math]::Round(100*($g.winDelta | Measure-Object -Average).Average,2);meanWaveDelta=[Math]::Round(($g.waveDelta | Measure-Object -Average).Average,2);meanKillDelta=[Math]::Round(($g.killDelta | Measure-Object -Average).Average,2);meanScoreDelta=[Math]::Round(($g.scoreDelta | Measure-Object -Average).Average,0);meanHealthDelta=[Math]::Round(($g.healthDelta | Measure-Object -Average).Average,2)}
})
if(-not $OutputDirectory){$OutputDirectory=Join-Path $Candidate ('comparison-'+[Guid]::NewGuid().ToString('N').Substring(0,8))}
if(Test-Path -LiteralPath $OutputDirectory){throw 'Choose a new comparison output directory.'}
$null=New-Item -ItemType Directory -Path $OutputDirectory
$pairs | Export-Csv -LiteralPath (Join-Path $OutputDirectory 'paired-games.csv') -NoTypeInformation
$comparison | Export-Csv -LiteralPath (Join-Path $OutputDirectory 'comparison.csv') -NoTypeInformation
@{baseline=(Resolve-Path -LiteralPath $Baseline).Path;candidate=(Resolve-Path -LiteralPath $Candidate).Path;baselineAssembly=$a.assemblySha256;candidateAssembly=$b.assemblySha256;matchedSeeds=$pairs.Count;note='Matched starting seeds and drafting policies; tuning can change later offers and behavior. PhysX is not guaranteed bitwise deterministic. These are policy outcomes, not human skill estimates.'} | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $OutputDirectory 'comparison.json') -Encoding UTF8
$comparison | Select-Object policy,matchedSeeds,changedLossToWin,changedWinToLoss,winRateDeltaPoints,meanWaveDelta | Format-Table -AutoSize
Write-Output (Resolve-Path -LiteralPath $OutputDirectory).Path
