param([string]$Player = (Join-Path $PSScriptRoot 'Build\Windows\WalkingHard.exe'))
$ErrorActionPreference = 'Stop'
if (-not (Test-Path -LiteralPath $Player)) { throw 'Build the Windows player first.' }
New-Item -ItemType Directory -Path (Join-Path $PSScriptRoot 'QA') -Force | Out-Null
New-Item -ItemType Directory -Path (Join-Path $PSScriptRoot 'Logs') -Force | Out-Null
$cases = @(@{Build='sequence';Seed=1741},@{Build='sequence';Seed=4319},@{Build='sword';Seed=1741},@{Build='hammer';Seed=1741},@{Build='rifle';Seed=1741},@{Build='twin';Seed=1741},@{Build='tempest';Seed=1741},@{Build='manual';Seed=1741},@{Build='reroll';Seed=1741})
$reports = foreach ($case in $cases) {
    $stem = 'v04-' + $case.Build + '-' + $case.Seed
    $reportPath = Join-Path $PSScriptRoot ('QA\' + $stem + '.json')
    $logPath = Join-Path $PSScriptRoot ('Logs\qa-' + $stem + '.log')
    if (Test-Path -LiteralPath $reportPath) { Remove-Item -LiteralPath $reportPath }
    $arguments = '-batchmode -nographics -vectorQA -seed ' + $case.Seed + ' -qaBuild ' + $case.Build + ' -qaOutput "' + $reportPath + '" -logFile "' + $logPath + '"'
    $run = Start-Process -FilePath $Player -ArgumentList $arguments -WindowStyle Hidden -PassThru
    if (-not $run.WaitForExit(60000)) { $run.Kill(); throw ('Encounter timed out: ' + $stem) }
    if ($run.ExitCode -ne 0) { throw ('Encounter check failed: ' + $stem + '. See ' + $logPath) }
    $report = Get-Content -LiteralPath $reportPath -Raw | ConvertFrom-Json
    if (-not $report.passed) { throw ('Assertions failed: ' + $stem) }
    $report
}
$reports | Select-Object build,seed,result,kills,launches,collisions,ragdolls,health,visibleSpawns,orderErrors,passed | Format-Table -AutoSize
Write-Output 'Nine runs passed loadout, fusion, ordered pairs, ultimate controls, scoring, concealed entry and weapon physics checks.'
