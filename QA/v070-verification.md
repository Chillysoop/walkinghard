# v0.7.0 — branching encounters

The release adds route choices after waves 2 and 5, reward emphasis on the existing mixed pick, shield bearers and volatile carriers. Rifle and Duelist tuning candidates remain external experiment overrides.

## Verification

- Fifteen core encounter fixtures, 45 build-variety/module/weapon-layout fixtures, an eight-wave run and a four-wave ultimate-progression fixture pass: 62 scenarios, each with 508 production assertions. Coverage fixtures use 5,000 health; they are not difficulty measurements.
- The 111 new assertions cover distinct ranked offers, bias surviving rerolls, the ultimate gate, route input guards, one reroll grant, retained checkpoint-five augment, one-wave route lifetime, front/rear/exposed shield damage, blast attribution, single detonation and shared action/cascade budgets.
- The full run completes with 13 item choices and two augment choices. Both routes appear at their expected waves. The first ultimate remains after checkpoint three.
- 1,000 ordinary Fast games across six policies report zero invalid records: 437 wins, 561 defeats, two Warden timeouts. Their spawn, kill identity, sequence order and cascade checks pass. Route choices use deterministic seed/checkpoint parity, not an optimal route policy.
- Launcher tests pass installation, checksum rejection, rollback, archive containment and offline fallback.

## Final player and visual acceptance

`Build/Routes-v070-verified/WalkingHard.exe` is the release player. Its assembly hash, batch inputs and metadata are archived in [v070-verification.json](v070-verification.json).

The 62-scenario suite and 1,000-game batch ran on the same production gameplay code before repairs to the optional `-routeReview` screenshot fixture. Those repairs clear stale action targets, reset the fixture wave clock and recreate the hero motor before placing actors near the camera. The final player reran all 508 production probes during native review, passed the complete eight-wave fixture, and matched all gameplay fields in 64 ordinary regression games exactly (only wall time excluded).

Native screenshots were captured with a briefly visible game window, as previously authorized, without Computer Use. Inspected the route choice, emphasized mixed rewards, shield/carrier silhouettes, open guard and orange blast. The actor captures are constructed QA scenes, not natural spawn snapshots. Natural concealed entry is covered by encounter tests and the simulation invariants.

- [Route choice](v070-21-route-choice.png)
- [Existing mixed reward with equipment emphasis](v070-22-route-rewards.png)
- [Shield bearer and volatile carrier](v070-23-route-enemies.png)
- [Exposed guard](v070-24-shield-exposed.png)
- [Carrier detonation](v070-25-carrier-blast.png)

## Limits and follow-up

Fast physics approximates contacts; it does not establish rendered combat feel or exact population balance. The native review is a visual acceptance check and the physics probes verify bounded behavior. A matched route-difficulty comparison remains useful, particularly because route rewards affect later loadouts. Two defensive/control builds reached the 480-second Warden limit; they are recorded as timeouts, not wins.

The separate balance study contains 624 baseline and 624 candidate Fast games plus 12 Full games, all on the same v0.6.9 executable with matched cases. See [recommendations](v070-balance-review.md). No balance override is bundled as active configuration or loaded by the desktop launcher.
