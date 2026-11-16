"""Summarize completed, normal-health journey samples. Standard library only.

Usage: python "Analyze Progression.py" QA/progression --output QA/progression/summary.json
Pilot directories are deliberately excluded. A defeated run is valid evidence;
timeouts and invariant failures remain failures, not losses.
"""
import argparse
import json
from collections import defaultdict
from pathlib import Path
from statistics import mean


def longest_gap(sample, flag):
    """Completed encounters without that offer; the unfinished tail is censored."""
    offered = {r["wave"] for r in sample["drafts"] if r[flag]}
    gap = longest = 0
    for encounter in sample["encounters"]:
        gap = 0 if encounter["wave"] in offered else gap + 1
        longest = max(longest, gap)
    return longest


def summarize(root):
    groups = defaultdict(list)
    identities = set()
    for path in sorted(root.glob("*/samples.jsonl")):
        if not path.parent.name.startswith(("projected-", "legacy-")):
            continue
        for line in path.read_text(encoding="utf-8").splitlines():
            row = json.loads(line)
            row["_source"] = path.relative_to(root).as_posix()
            identity = row["drafter"], row["pool"], row["seed"], row["policy"], row.get("weaponOffers", "original"), row.get("committed", False)
            if identity in identities:
                raise ValueError(f"Duplicate sample: {identity}")
            identities.add(identity)
            groups[row["drafter"], row["pool"], row["policy"], row.get("weaponOffers", "original"), row.get("committed", False)].append(row)
    incidents = root / "incidents.json"
    output = {"scope": "Fixed seeds, 1x time including hit stop, normal health, Full PhysX. Policies are preferences, not fixed loadouts; inspect final_equipped for pivots. Upgrade/defense opportunity uses a heuristic, not measured combat benefit. Gaps count completed encounters and are survival-censored. Unfinished fatal encounters are included only in incoming-damage records.", "initial_incidents": json.loads(incidents.read_text(encoding="utf-8")) if incidents.exists() else [], "groups": [], "runs": []}
    for (drafter, pool, policy, offers, committed), rows in sorted(groups.items()):
        districts = []
        for district in range(3):
            waves = [w for r in rows for w in r["encounters"] if (w["wave"] - 1) // 7 == district]
            if waves:
                districts.append({"district": district + 1, "encounters": len(waves), "mean_carried": round(mean(w["carried"] for w in waves), 2), "mean_integrity_out": round(mean(w["health"] for w in waves), 2)})
        output["groups"].append({"drafter": drafter, "pool": pool, "policy": policy, "weaponOffers": offers, "committed": committed, "n": len(rows), "valid": all(r["valid"] for r in rows), "wins": sum(r["result"] == "Complete" for r in rows), "mean_encounters_cleared": round(mean(len(r["encounters"]) for r in rows), 2), "mean_fusions": round(mean(r["fusions"] for r in rows), 2), "districts": districts})
        for r in rows:
            output["runs"].append({"source": r["_source"], "drafter": drafter, "pool": pool, "policy": policy, "weaponOffers": offers, "committed": committed, "seed": r["seed"], "result": r["result"], "wave": r["wave"], "combat_minutes": round(r["seconds"] / 60, 2), "fusions": r["fusions"], "weapon_offer_gap": longest_gap(r, "weaponOffered"), "weapon_upgrade_gap": longest_gap(r, "weaponUpgrade"), "matching_copy_gap": longest_gap(r, "copyOffered"), "defense_offer_gap": longest_gap(r, "defenseOffered"), "top_damage": sorted(r["integrity"]["sources"], key=lambda s: -s["damage"])[:3], "final_loadout": r["loadout"], "final_equipped": [{"item": i["item"], "rarity": i["rarity"]} for i in r["items"] if i["equipped"]]})
    return output


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("root", type=Path)
    parser.add_argument("--output", type=Path, required=True)
    args = parser.parse_args()
    result = summarize(args.root)
    args.output.write_text(json.dumps(result, indent=2) + "\n", encoding="utf-8")
    for group in result["groups"]:
        print(f'{group["drafter"]:9} {group["policy"]:8} {group["wins"]}/{group["n"]} complete; mean cleared {group["mean_encounters_cleared"]}; valid={group["valid"]}')
