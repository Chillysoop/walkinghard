param([string]$Player = (Join-Path $PSScriptRoot 'Build\Windows\Vector.exe'))
$ErrorActionPreference = 'Stop'
if (-not (Test-Path -LiteralPath $Player)) { throw 'Build the Windows player first.' }
$cases = @(
    @{ Build = 'pinball'; Seed = 1741 },
    @{ Build = 'pinball'; Seed = 4319 },
    @{ Build = 'launch'; Seed = 1741 },
    @{ Build = 'power'; Seed = 1741 },
    @{ Build = 'reroll'; Seed = 1741 }
)
New-Item -ItemType Directory -Path (Join-Path $PSScriptRoot 'QA') -Force | Out-Null
New-Item -ItemType Directory -Path (Join-Path $PSScriptRoot 'Logs') -Force | Out-Null
$reports = @()
foreach ($case in $cases) {
    $stem = 'v02-' + $case.Build + '-' + $case.Seed
    $reportPath = Join-Path $PSScriptRoot ('QA\' + $stem + '.json')
    $logPath = Join-Path $PSScriptRoot ('Logs\qa-' + $stem + '.log')
    $arguments = '-batchmode -nographics -vectorQA -seed ' + $case.Seed + ' -qaBuild ' + $case.Build + ' -qaOutput "' + $reportPath + '" -logFile "' + $logPath + '"'
    $run = Start-Process -FilePath $Player -ArgumentList $arguments -WindowStyle Hidden -PassThru -Wait
    if ($run.ExitCode -ne 0) { throw ('Encounter check failed: ' + $stem + '. See ' + $logPath) }
    $report = Get-Content -LiteralPath $reportPath -Raw | ConvertFrom-Json
    if (-not $report.passed) { throw ('Assertions failed: ' + $stem) }
    $reports += $report
}
# The reroll stream must not alter gameplay. Both paths install the same upgrades.
$normal = $reports | Where-Object { $_.build -eq 'pinball' -and $_.seed -eq 1741 }
$rerolled = $reports | Where-Object { $_.build -eq 'reroll' }
foreach ($metric in @('elapsed', 'health', 'spawned', 'kills', 'launches', 'collisions', 'collisionDamage')) {
    if ($normal.$metric -ne $rerolled.$metric) { throw ('Rerolls affected encounter metric: ' + $metric) }
}
if ($normal.waves[0].launches -ne 0 -or $normal.waves[1].collisions -ne 0 -or $normal.waves[2].collisions -lt 3) {
    throw 'Upgrade effects were not gated to the correct waves.'
}
$reports | Select-Object build, seed, result, kills, launches, collisions, health, rerolls, passed | Format-Table -AutoSize
Write-Output 'All encounter, upgrade-gating, checkpoint, and reroll-isolation checks passed.'
