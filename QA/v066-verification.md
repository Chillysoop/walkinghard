# Walking Hard 0.6.6 verification

29 October 2026. Build: `Build/DefenseBalance-v066-final/WalkingHard.exe`. Assembly-CSharp.dll SHA-256: `8ce390368580dfd6bdddd50a45d268a67e86c9766e4eb13930f1b2f3b4dde5a0a`.

User-approved Counterguard/Duelist tuning is implemented and retained after separate and combined tests. See `v066-balance-review.md` and `v066-balance-results.json` for evidence and limits.

- 3 × 1,000 Fast candidate games: zero invalid/missing/duplicate games; seeds 12000-12999, four workers, eight waves, normal health.
- 32-game legacy control: all 27 gameplay fields match the prior baseline; version and wall time excluded.
- 12 + 12 paired Full PhysX games: valid, no timeouts, all survive the first three waves; normal health. Targeted Duelist-rich seed block, not a population sample.
- `progression` and `manual` Full QA scenarios: 187 production-effect assertions each. Explicit 5,000-health fixture used only for mechanics coverage. Both preserve ordering, delay indicators, weapon retention, kill identity and checkpoint-three ultimate progression.
- New ward tests cover all five rarities, expiry, capped mitigation, two charges, third hit, first-only stagger, blocked-hit preservation, non-stacking refresh, real reload/sever activation and run reset.
- Text descriptions read the same duration/charge helpers as combat. New live ward status reports remaining charges. No geometry/layout changes require a new rendered acceptance pass.
- Release PowerShell syntax and `git diff --check` pass.

The initial sever-trigger probe accidentally allowed an earlier Counterguard bank to kill its target. The final fixture clears that bank before testing a sword sever; no gameplay change followed the legacy-control run. Counterguard capacity, conversion/caps, offensive stats, enemy stats and general spawn cadence remain unchanged.
