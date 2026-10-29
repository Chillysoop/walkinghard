param([string]$Player=(Join-Path $PSScriptRoot 'Build\Windows\WalkingHard.exe'),[string]$ReportPrefix='v060',[string[]]$Builds=@(),[switch]$FullRun,[int]$HeroHealth=0)
$ErrorActionPreference='Stop'
if($Builds.Count -eq 0){
    $Builds=@('pinball','pinball-separated','freight','freight-piledriver','ballet','funnel','funnel-breach','cutcarry','close')
    foreach($module in @('FollowThrough','Focused','Echo','Afterfield','SplitPayload','Backwash','Discharge','Anchor','Flurry','PalmDrive','Crosscut','HookedEdge','Groundbreaker','Piledriver','BoreRounds','BreachLoad')){$Builds+=('aug-'+$module)}
    foreach($family in @('fists','sword','hammer','guns')){foreach($layout in @('paired','separate','same','single')){$Builds+=('compare-'+$family+'-'+$layout)}}
    $Builds+=@('compare-fists-shared','compare-sword-2h','compare-hammer-2h','compare-guns-2h')
}
$reports=foreach($build in $Builds){
    $stem=$ReportPrefix+'-'+$build+'-1741'
    $reportPath=Join-Path $PSScriptRoot ('QA\'+$stem+'.json')
    $logPath=Join-Path $PSScriptRoot ('Logs\qa-'+$stem+'.log')
    $args='-batchmode -nographics -vectorQA -seed 1741 -qaBuild '+$build+' -qaOutput "'+$reportPath+'" -logFile "'+$logPath+'"'
    if($FullRun){$args+=' -qaFullRun'}
    if($HeroHealth -gt 0){$args+=' -qaHealth '+$HeroHealth}
    $process=Start-Process -FilePath $Player -ArgumentList $args -WindowStyle Hidden -PassThru
    if(-not $process.WaitForExit(180000)){$process.Kill();throw "Timed out: $stem"}
    if($process.ExitCode -ne 0){throw "Failed: $stem (exit $($process.ExitCode)); see $logPath"}
    $report=Get-Content -LiteralPath $reportPath -Raw | ConvertFrom-Json
    if(-not $report.passed -or $report.mechanicAssertions -lt 56){throw "Assertions failed: $stem"}
    Write-Host ($build+': passed; '+$report.kills+' kills, '+$report.collisions+' collisions, '+$report.health.ToString('0')+' integrity')
    $report
}
$reports | Select-Object build,waves,itemChoices,augmentChoices,kills,collisions,health,mechanicAssertions,passed | Format-Table -AutoSize
