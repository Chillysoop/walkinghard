# Walking Hard 0.6.8 — verification

29 October 2026. Final player: `Build/SiphonBalance-v068/WalkingHard.exe`, Unity 6000.3.27f1.

Assembly-CSharp.dll SHA-256: `b2d257d971d9d7d2a0fee4b9bed837bbad1c3dc63d4d311d12000dfd2c2569d3`.

All gameplay, UI text, production checks and version changes were included before this build. Only experiment scripts, reports and release documentation changed afterward. Publish this tested player with `-SkipBuild`.

## Implemented behavior

Siphon Blade heals its own credited damage and enables other attacks to heal against its marks. Equipping it no longer grants unrelated damage global Lifesteal. Reclaimer uses 3/4/5/6/7% global Lifesteal with a 1.5-times contribution below half health. Redline accelerates six sequence turns. Harvest, health bonuses, enemy tuning and the shared originating-action cap are unchanged. Item and character-sheet wording describe these distinctions.

## Production and encounter checks

- All 15 core scenarios pass: sequences, sword/hammer/rifle/twin weapons, Tempest, manual ultimate, rerolls, half/quadruple speed, armor, quick equip, persistent weapons and reload indicators.
- All 45 variety scenarios pass: nine hybrid builds, sixteen modules and twenty weapon layouts.
- Complete eight-wave fixture passes: `v068-full-1741.json`, 303 kills.
- Four-wave ultimate-progression fixture passes: `v068-progression-1741.json`, 155 kills.
- Every one of these 62 reports passes 388 production mechanic assertions. Their encounter health is explicitly 5,000 for coverage, so those wins are not balance evidence.
- Launcher checks pass valid install, checksum rejection, failed-build rollback, archive containment and offline fallback.

New probes run at all five rarities: equipped blade does not inflate the global character-sheet stat; native blade drain uses the originating weapon's rarity without summing its offhand; unrelated weapon/ability damage to unmarked foes cannot borrow blade healing; marks enable ability drain; augment and delayed collision credits preserve blade healing; Reclaimer uses exact independent rarity rates and its 1.5-times low-health contribution. Existing overkill, corpse, cap, maximum-health, mark, Harvest and fusion probes remain.

Redline probes check ammo refill, six actual accelerated turns, unchanged 18-second recharge and all five haste values. Ultimate activation consumes no charge; a paired action consumes one; the buff ends after its sixth sequence turn.

The core runner's outer shell wrapper reported a failure after all fifteen passing scenarios because it checked the native `$LASTEXITCODE` variable after a PowerShell script. This was a wrapper issue, not a game failure. The remaining Full fixture was then launched separately and passed. Verification uses the actual JSON results, not that wrapper status.

## Balance experiments

- 420 legacy-rule controls reproduce all gameplay fields, including item attribution and wave reports. Experimental metrics, version and wall time are excluded from that equality check.
- 4,680 newly executed affected Fast cases: blade-only 1,260; armor-only 1,000; combined healing 1,500; six-turn Redline 920. Unaffected cases are reused to reconstruct the 4,080-case matrix, explicitly identified in the report.
- 1,000 ordinary mixed-policy Fast games: 434 wins, 849 clear wave three, zero invalid games or timeouts, 95.7 seconds end to end.
- Six preselected ordinary-health Full physics games: all valid, three clears, zero timeouts. These provide small-sample coverage rather than a population win rate.
- Total new balance executions: 6,100 Fast and six Full. The 62 elevated-health QA scenarios above are separate.
- Actual healing reconciles with potential drain minus the originating-action cap loss and overheal across every affected controlled case.

Exact inputs, rule files, per-case results and rarity summaries are in `QA/Balance/v068/`. Provenance and ordinary draft comparisons are in [v068-balance-results.json](v068-balance-results.json). Interpretation and limitations: [v068-balance-review.md](v068-balance-review.md).

## Visual verification limitation

The native presentation-review fixture was attempted with D3D11 in hidden batch mode and again in a hidden native window. Capture failed; the resulting checkpoint image was black. No new visual result is claimed, and that image is not shipped as QA evidence. Logs remain local in `Logs/v068-ui.log` and `Logs/v068-ui-native.log`.

This release changes UI wording but not layout or artwork. Visual readability of the new wording remains unconfirmed. Headless tests verify mechanics and data, not rendered text. No Computer Use automation or scheduled task was used.

## Limits

Fast simulation approximates body motion and contact. The previous Fast/Full comparison agreed on only 14/24 individual outcomes; do not interpret Fast win percentages as a precise prediction of player difficulty. Six Full checks and fixed loadouts do not establish comprehensive balance. Redline's net gain is only one win across its 920 controlled cases, and Reclaimer remains the strongest armor in these fixtures; further tuning requires its own comparison.
