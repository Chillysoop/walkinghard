param(
    [string]$Player=(Join-Path $PSScriptRoot 'Build\Windows\WalkingHard.exe'),
    [string]$OutputDirectory=(Join-Path $PSScriptRoot ('QA\progression-'+(Get-Date -Format 'yyyyMMdd-HHmmss'))),
    [int[]]$Seeds=@(1741,4319),
    [string[]]$Policies=@('pinball','storm','sever','gun','cutcarry','close','siphon'),
    [ValidateSet('fresh','unlocked')][string]$Pool='unlocked',
    [ValidateRange(1,6)][int]$Workers=4,
    [ValidatePattern('^[a-z0-9-]*$')][string]$RunLabel='',
    [switch]$Legacy,
    [switch]$CommittedWeapons,
    [switch]$OriginalWeaponOffers
)
$ErrorActionPreference='Stop'
$Player=(Resolve-Path -LiteralPath $Player).Path
$OutputDirectory=[IO.Path]::GetFullPath($OutputDirectory)
$mode=if($Legacy){'legacy'}else{'projected'}
if($CommittedWeapons -and ($Legacy -or @($Policies | Where-Object {$_ -notin @('gun','cutcarry','close')}).Count -gt 0)){throw 'Committed trials require projected gun, cutcarry, or close policies.'}
$offers=if($OriginalWeaponOffers){'original'}else{'discovery'}
$commitment=if($CommittedWeapons){'committed'}else{'adaptive'}
$pending=[Collections.Generic.Queue[object]]::new()
foreach($seed in $Seeds){foreach($policy in $Policies){
    if($policy -notin @('pinball','storm','sever','gun','cutcarry','close','siphon')){throw "Unknown policy: $policy"}
    $suffix=if($RunLabel){"-$RunLabel"}else{''};$directory=Join-Path $OutputDirectory "$mode-$seed-$policy-$offers-$commitment$suffix"
    if(Test-Path -LiteralPath $directory){throw "Use fresh output paths: $directory"}
    $pending.Enqueue(@{Seed=$seed;Policy=$policy;Directory=$directory})
}}
$active=[Collections.Generic.List[object]]::new()
$failures=[Collections.Generic.List[string]]::new()
try {
    while($pending.Count -gt 0 -or $active.Count -gt 0){
        while($pending.Count -gt 0 -and $active.Count -lt $Workers){
            $case=$pending.Dequeue();New-Item -ItemType Directory -Path $case.Directory -Force | Out-Null
            $assembly=Join-Path (Split-Path -Parent $Player) 'WalkingHard_Data\Managed\Assembly-CSharp.dll'
            @{assemblySha256=(Get-FileHash -LiteralPath $assembly -Algorithm SHA256).Hash;seed=$case.Seed;policy=$case.Policy;pool=$Pool;drafter=$mode;weaponOffers=$offers;committed=[bool]$CommittedWeapons} | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $case.Directory 'manifest.json')
            $arguments='-batchmode -nographics -progressionReview -seeds '+$case.Seed+' -journeyPolicy '+$case.Policy+' -pool '+$Pool+' -saveDirectory "'+$case.Directory+'\profile" -reviewOutput "'+$case.Directory+'" -logFile "'+$case.Directory+'\player.log"'
            if($Legacy){$arguments+=' -legacyDraftPolicy'}
            if($CommittedWeapons){$arguments+=' -committedWeapons'}
            if($OriginalWeaponOffers){$arguments+=' -originalWeaponOffers'}
            $case.Process=Start-Process -FilePath $Player -ArgumentList $arguments -WindowStyle Hidden -PassThru
            $active.Add($case)
        }
        foreach($case in @($active.ToArray())){
            if(-not $case.Process.HasExited){continue}
            $case.Process.WaitForExit()
            if($case.Process.ExitCode -ne 0){$failures.Add($case.Directory);Write-Output "FAILED: $($case.Directory) (exit $($case.Process.ExitCode))";$active.Remove($case) | Out-Null;continue}
            $sample=Get-Content -LiteralPath (Join-Path $case.Directory 'samples.jsonl') -Raw | ConvertFrom-Json
            if(-not $sample.valid){throw "Invalid sample: $($case.Directory)"}
            Write-Output "$mode / $($case.Seed) / $($case.Policy): $($sample.result), encounter $($sample.wave)"
            $active.Remove($case) | Out-Null
        }
        if($active.Count -gt 0){Start-Sleep -Seconds 2}
    }
    if($failures.Count -gt 0){throw ("Review failures: "+($failures -join ', '))}
} finally {
    # Only child processes created by this invocation may be stopped.
    foreach($case in $active){if(-not $case.Process.HasExited){$case.Process.Kill()}}
}
