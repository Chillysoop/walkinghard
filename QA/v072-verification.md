# Walking Hard 0.7.2 verification

October 29, 2026. Shipping player: `Build/Reliability-v072-verified/WalkingHard.exe`, built with Unity 6000.3.27f1. `Assembly-CSharp.dll` SHA-256: `10dbb4e7918855038663f92cf6e1dad6004caac668271ca38cbaa015c0826eac`.

## Implemented

- Duelist guards three damaging hits, keeping its duration, mitigation, first-hit stagger, blocked-hit preservation and replacement refresh rules. Rifle remains 42 damage.
- Continuous target waits of at least 0.5 seconds count as pauses. A checkpoint warning appears above the sequence for meaningful repeated/long waits, linking to actual last-wave duration, count and reach. No action is skipped or reordered. Snapshots remain unchanged by checkpoint edits, and cumulative counts follow fusion ownership.
- Development-only cases can specify a five-action sequence and route. Ordinary simulations can independently override encounter route and reward emphasis. Default play retains the existing route and reward behavior.
- The Full route self-test now suspends controlled experiment settings while checking ordinary checkpoint rules, restoring them afterward.
- `ART_VFX_PLAN.md` contains the requested proposed free-resource presentation pass. No downloaded art, VFX assets or new Deadeye items are included in this release.

## Checks

- **15 core fixtures**, all passing on the shipping player: `v072-core-*.json`.
- **45 build-variety fixtures**, all passing on the shipping player: `v072-variety-*.json`. The existing validator was run in three independent groups to reduce wall time; each case ran once with its own report/log.
- **One full eight-wave progression fixture**, passing: `v072-shipping-full-1741.json`, 314 kills, 144 collisions, 13 item choices and two augment choices. Core/variety/progression fixtures use 5,000 integrity to exercise the systems, not estimate balance.
- **533 production assertions** cover the existing action/effect contracts plus three-charge Duelist behavior at all five rarities and target-wait counting, snapshot stability and warning reset.
- Native D3D11 checkpoint/detail captures finish with `WAIT_REVIEW_SUCCESS`. Inspected `v072-checkpoint-target-wait.png` and `v072-target-wait-details.png`; the warning fits above the existing sequence and opens a readable explanation.
- Launcher checks pass: valid install, checksum rejection, failed-build rollback, archive containment and offline fallback.
- **64 matched gameplay games** exactly match v0.7.1 with only the approved `duelist.wardHits=3` override. Comparison excludes version, wall runtime and the new `waitEpisodes` telemetry. All other result fields and wave records match.

## Balance evidence

The report in `v072-balance-review.md` separates:

- 600 fixed ranged-sequence Fast games.
- 288 fixed-loadout route Fast games.
- 720 reward-only normal-drafting Fast games with standard encounters.
- 480 linked-route normal-drafting Fast games.
- 12 Full ranged-sequence games and six Full route games.
- Two old timeout seeds, each reproduced on the old and current builds.

The 2,088 Fast comparisons used gameplay candidate assembly `63aff7e28927426ba7f3e054146691d04ba88d37f58ea019fc5f2098c57d7c38`. The only subsequent source change was the Full startup test repair, which Fast mode does not execute. All reported Full comparisons use the shipping assembly. Their actual hashes, inputs and totals are in `v072-balance-results.json` and `Balance/v072/`.

Two initial Full route pairs diverged before the first affected wave in multi-game workers. Those pairs are excluded from comparative evidence; the reported six-case set uses a fresh process per case and checks identical early-wave records. This is a small physics coverage slice, not proof the routes have equal population win rates. The report explicitly discusses the rifle/mark disagreement between Fast and Full.

No further rifle, boss or route tuning is applied. Proposed Volatile offer improvements and Deadeye additions remain recommendations.

## Reproduce

Use `Build Windows.ps1`, then `Validate Prototype.ps1` and `Validate Variety.ps1`, pointing `-Player` at the built executable and using fresh report prefixes. Add `Validate Variety.ps1 -Builds full -FullRun`. Use `-HeroHealth 5000` for the structural fixtures above.

`QA/Balance/v072/prepare.py` generates fixed cases in `QA/Simulations/v072-inputs`; run them with `QA/Balance/controlled-siphon/run.py`. `run_rewards.py` and its `--linked` mode generate separate ordinary-drafting experiments. `run_fresh_full.py` runs each matched Full route in its own process. `timeout_repro.py` requires the retained old and current players. `analyze.py` validates the evidence and writes JSON/CSV. Reproduction scripts reject existing output folders.

For native UI review, launch the built player with `-vectorQA -qaBuild ui -qaHealth 5000 -qaHoldCheckpoint -presentationReview -waitReview -force-d3d11`. Its own screenshot routine captures the two checkpoint views and exits; a visible window is required on this machine.
