"""Compare natural-reward weapon trials, including actual encounter loadouts.

python "Analyze Weapon Discovery.py" QA/weapon-discovery/cohort --output QA/weapon-discovery/cohort-summary.json
No third-party packages. Defeats are valid outcomes; engine failures stay separate.
"""
import argparse
import json
from collections import defaultdict
from pathlib import Path
from statistics import mean


def summarize(root):
    groups = defaultdict(list)
    seen = set()
    failures = []
    for folder in sorted(root.glob("projected-*")):
        path = folder / "samples.jsonl"
        if not path.exists():
            failures.append({"source": folder.relative_to(root).as_posix(), "status": "no outcome; inspect player.log"})
            continue
        for line in path.read_text(encoding="utf-8").splitlines():
            r = json.loads(line)
            if not r["valid"]:
                failures.append({"source": path.relative_to(root).as_posix(), "status": r["result"], "errors": r["errors"], "wave": r["wave"], "seed": r["seed"], "policy": r["policy"]})
                continue
            expected = r["weaponOffers"] == "discovery"
            for draft in r["drafts"]:
                if draft["discovery"] != (expected and draft["kind"] == "Mixed" and draft["wave"] in (2, 9, 16)):
                    raise ValueError(f"Unexpected discovery checkpoint in {path}: {draft['wave']}")
            if len(r["weaponEncounters"]) != r["wave"]:
                raise ValueError(f"Missing encounter equipment record in {path}")
            key = r["pool"], r["weaponOffers"], r["committed"], r["policy"]
            identity = (*key, r["seed"])
            if identity in seen:
                raise ValueError(f"Duplicate trial {identity}")
            seen.add(identity)
            f = r["weaponEncounters"]
            def assembled_weapons(e):
                ids = set(e["weapons"])
                if r["policy"] == "cutcarry":
                    return bool(ids & {"sword", "siphonblade"}) and "breachhammer" in ids
                if r["policy"] == "close":
                    return bool(ids & {"fists", "wraps"}) and bool(ids & {"pistol", "scattergun"})
                return bool(ids & {"pistol", "rifle", "scattergun"})
            # Adaptive drafts may use either hand. The raw `components` field is
            # the committed policy's fixed-mount objective, not a general classifier.
            assembled = [e for e in f if assembled_weapons(e)]
            offered = {i for d in r["drafts"] for i in d["offerIds"]}
            changed = sum(a["weapons"] != b["weapons"] or a["ranks"] != b["ranks"] for a, b in zip(f, f[1:]))
            weapon_items = [i for i in r["items"] if i["item"] in {"fists", "wraps", "sword", "siphonblade", "greatsword", "hammer", "breachhammer", "pistol", "rifle", "scattergun"}]
            run = {"source": path.relative_to(root).as_posix(), "seed": r["seed"], "valid": r["valid"], "result": r["result"], "cleared": len(r["encounters"]),
                   "breach_offered": "breachhammer" in offered, "first_assembled": min((e["wave"] for e in assembled), default=None),
                   "assembled_encounters": len(assembled), "encounters_observed": len(f), "final_assembled": bool(f and assembled_weapons(f[-1])), "weapon_changes": changed, "fusions": r["fusions"],
                   "constraints_held": all(e["allowed"] for e in f), "final_weapons": f[-1]["weapons"] if f else [], "final_ranks": f[-1]["ranks"] if f else [],
                   "final_loadout": r["loadout"], "equipped": [{"item": i["item"], "rank": i["rarity"]} for i in r["items"] if i["equipped"]],
                   "weapon_damage": sum(e["weaponDamage"] for e in f), "all_damage": sum(e["damage"] for e in f), "weapon_wait": sum(e["weaponWait"] for e in f),
                   "weapon_reload": sum(e["weaponReload"] for e in f), "fight_seconds": sum(e["seconds"] for e in f),
                   "incoming": sum(s["damage"] for s in r["integrity"]["sources"]), "healing": sum(e["healing"] for e in f),
                   "weapon_assists": sum(i["values"]["assists"] for i in weapon_items), "weapon_healing": sum(i["values"]["healing"] for i in weapon_items),
                   "weapon_control": sum(i["values"]["control"] for i in weapon_items),
                   "districts": [{"district": d+1, "fights": len(fs), "weapon_damage": sum(e["weaponDamage"] for e in fs), "weapon_wait": sum(e["weaponWait"] for e in fs),
                                  "weapon_reload": sum(e["weaponReload"] for e in fs), "seconds": sum(e["seconds"] for e in fs), "final_weapons": fs[-1]["weapons"], "final_ranks": fs[-1]["ranks"]}
                                 for d in range(3) if (fs := [e for e in f if (e["wave"]-1)//7 == d])]}
            groups[key].append(run)
    result = {"scope": "Normal-health natural-reward Full PhysX journeys. Adaptive preferences may pivot. Committed policies restrict weapon mounts and prioritize acquiring their components; abilities/gear still adapt. Small fixed-seed samples are not human win rates or equal-rarity weapon benchmarks. Fight seconds use the active simulation clock, excluding hit stop/reveal/decisions. Weapon wait/reload are summed item contribution seconds (paired items may overlap). Encounters include fatal fights; offer exposure is survival-censored and excludes shops. No failed attempt is counted as a defeat.",
              "failures": failures, "groups": []}
    for (pool, offers, committed, policy), runs in sorted(groups.items()):
        duration = sum(r["fight_seconds"] for r in runs)
        damage = sum(r["all_damage"] for r in runs)
        result["groups"].append({"pool": pool, "offers": offers, "committed": committed, "policy": policy, "n": len(runs),
                                 "wins": sum(r["result"] == "Complete" for r in runs), "mean_cleared": round(mean(r["cleared"] for r in runs), 2),
                                 "breach_offered_runs": sum(r["breach_offered"] for r in runs), "assembled_runs": sum(r["first_assembled"] is not None for r in runs),
                                 "wins_with_complete_weapons": sum(r["result"] == "Complete" and r["final_assembled"] for r in runs),
                                 "weapon_damage_per_fight_second": round(sum(r["weapon_damage"] for r in runs)/duration, 2) if duration else 0,
                                 "weapon_damage_share": round(sum(r["weapon_damage"] for r in runs)/damage, 3) if damage else 0,
                                 "weapon_wait_per_fight_second": round(sum(r["weapon_wait"] for r in runs)/duration, 3) if duration else 0,
                                 "all_valid": all(r["valid"] and r["constraints_held"] for r in runs), "runs": runs})
    return result


if __name__ == "__main__":
    p = argparse.ArgumentParser(description=__doc__)
    p.add_argument("root", type=Path)
    p.add_argument("--output", type=Path, required=True)
    args = p.parse_args()
    result = summarize(args.root)
    args.output.write_text(json.dumps(result, indent=2)+"\n", encoding="utf-8")
    for g in result["groups"]:
        print(f'{g["pool"]:8} {g["offers"]:9} {"committed" if g["committed"] else "adaptive":9} {g["policy"]:8}: {g["wins"]}/{g["n"]} complete, mean cleared {g["mean_cleared"]}, assembled {g["assembled_runs"]}/{g["n"]}, valid={g["all_valid"]}')
    print(f'{len(result["failures"])} attempts without an outcome')
