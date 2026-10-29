param([Parameter(Mandatory=$true)][string]$Directory,[double]$BatchSeconds=0)
$ErrorActionPreference='Stop'
$results=@(Get-ChildItem -LiteralPath $Directory -Filter 'worker-*.jsonl' | ForEach-Object {
    $stream=[IO.File]::Open($_.FullName,[IO.FileMode]::Open,[IO.FileAccess]::Read,[IO.FileShare]::ReadWrite)
    $reader=[IO.StreamReader]::new($stream)
    try{$snapshot=$reader.ReadToEnd()}finally{$reader.Dispose()}
    # A running worker may be halfway through appending its last JSON record.
    $end=$snapshot.LastIndexOf("`n")
    if($end -ge 0){foreach($line in $snapshot.Substring(0,$end).Split("`n")){if($line.Trim()){$line | ConvertFrom-Json}}}
})
if($results.Count -eq 0){throw 'No completed games yet.'}
$valid=@($results | Where-Object valid)
function Average($values){if(@($values).Count -eq 0){return 0};return ($values | Measure-Object -Average).Average}
$summary=@($valid | Group-Object policy | ForEach-Object {
    $games=@($_.Group);$n=$games.Count;$wins=@($games | Where-Object result -eq 'Complete').Count;$p=$wins/$n;$z=1.96;$den=1+$z*$z/$n;$center=($p+$z*$z/(2*$n))/$den;$margin=$z*[Math]::Sqrt($p*(1-$p)/$n+$z*$z/(4*$n*$n))/$den
    [pscustomobject]@{policy=$_.Name;games=$n;wins=$wins;timedOut=@($games | Where-Object result -eq 'TimedOut').Count;winPercent=[Math]::Round(100*$p,2);ci95Low=[Math]::Round(100*($center-$margin),2);ci95High=[Math]::Round(100*($center+$margin),2);meanWave=[Math]::Round((Average $games.waveReached),2);meanKills=[Math]::Round((Average $games.kills),1);meanScore=[Math]::Round((Average $games.score),0);meanSeconds=[Math]::Round((Average $games.wallSeconds),2)}
})
$summary | Export-Csv -LiteralPath (Join-Path $Directory 'policies.csv') -NoTypeInformation
$flattened=@(foreach($game in $valid){foreach($item in $game.items){if($item.equipped){[pscustomobject]@{item=$item.item;rarity=$item.rarity;module=$item.module;win=([int]($game.result -eq 'Complete'));kills=$item.kills;direct=$item.values.direct;collision=$item.values.collision;control=$item.values.control;absorption=$item.values.absorption;reload=$item.values.reload;preparation=$item.values.preparation;rangeWait=$item.values.rangeWait;augment=$item.values.augment;assists=$item.values.assists}}}})
$items=@($flattened | Group-Object item,rarity,module | ForEach-Object {$g=@($_.Group);[pscustomobject]@{item=$g[0].item;rarity=$g[0].rarity;module=$g[0].module;equippedRuns=$g.Count;observedWinPercent=[Math]::Round(100*(Average $g.win),2);meanKills=[Math]::Round((Average $g.kills),1);meanDirect=[Math]::Round((Average $g.direct),1);meanCollision=[Math]::Round((Average $g.collision),1);meanControlSeconds=[Math]::Round((Average $g.control),2);meanAbsorbed=[Math]::Round((Average $g.absorption),1);meanReloadSeconds=[Math]::Round((Average $g.reload),2);meanPreparationSeconds=[Math]::Round((Average $g.preparation),2);meanRangeWaitSeconds=[Math]::Round((Average $g.rangeWait),2);meanAugmentDamage=[Math]::Round((Average $g.augment),1);meanAssists=[Math]::Round((Average $g.assists),1)}})
$items | Export-Csv -LiteralPath (Join-Path $Directory 'items.csv') -NoTypeInformation
$valid | Select-Object seed,policy,result,waveReached,kills,collisions,reloads,score,health,wallSeconds,loadout | Export-Csv -LiteralPath (Join-Path $Directory 'games.csv') -NoTypeInformation
$metadata=Get-Content -LiteralPath (Join-Path $Directory 'batch.json') -Raw | ConvertFrom-Json
if($BatchSeconds -le 0){$endTime=if($metadata.completedUtc){[DateTime]::Parse($metadata.completedUtc).ToUniversalTime()}else{[DateTime]::UtcNow};$BatchSeconds=($endTime-[DateTime]::Parse($metadata.startedUtc).ToUniversalTime()).TotalSeconds}
@{requested=$metadata.runs;completed=$results.Count;valid=$valid.Count;invalid=$results.Count-$valid.Count;missing=$metadata.runs-$results.Count;duplicateSeeds=$results.Count-@($results.seed | Sort-Object -Unique).Count;batchSeconds=$BatchSeconds;gamesPerMinute=if($BatchSeconds -gt 0){$valid.Count*60/$BatchSeconds}else{0};policies=$summary;note='Real Unity combat and PhysX; headless rendering only. Policy and end-loadout item win rates are observational, not causal. Small samples have wide uncertainty. Use identical seed lists and worker settings for comparisons.'} | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $Directory 'summary.json') -Encoding UTF8
$summary | Format-Table -AutoSize
Write-Host ($valid.Count.ToString()+' valid games; '+($results.Count-$valid.Count)+' invalid; '+($metadata.runs-$results.Count)+' not yet reported.')
