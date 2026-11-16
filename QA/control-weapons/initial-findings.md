# Initial findings retained

- The shared source checkout was being edited by a separate constellation task. An initial build found missing enum members from that concurrent work. This task moved to `WalkingHard-Crowds`, based on the installed bfa7e65 commit; the other checkout and its changes were preserved.
- First collision-layer candidate used layer 8, already used by world line-of-sight queries. The Breaker probe failed. Dedicated actor layers 12–15 fixed that conflict. All 15 filter-only core runs then passed (`v089-filters-fixed-*`).
- First item build failed because the effect API accepts three Arc arguments, not four. Fixed the calls; original compiler log remains in `Logs/build-v089-items-initial.log`.
- First item mechanics probe expected the entire augmented impulse at every rarity. The implementation intentionally caps force for nearby targets; the assertion now checks that cap. This did not change gameplay values (`qa-v089-items-r1-sequence-1741.log`).
- Next run passed the mechanics but found the catalog assertion still expected 47 rather than 49 items. Updated the expected count (`qa-v089-items-r2-sequence-1741.log`). All 15 final item core cases passed.
- Initial native mechanic fixtures placed enemies too close to demonstrate control before reaching melee. The fixture now uses distant surviving targets and applies the authored weapon identity model. Original motion traces and images remain in `QA/v089-control-motion-initial`; this is a fixture correction, not a weapon buff.
- Hover wording was corrected from promising 1.8m final clearance to stating that no pull is applied inside 1.8m. Inertia and incoming movement remain physical; the force cap does not teleport or snap a target to a stopping point.
