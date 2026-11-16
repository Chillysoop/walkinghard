"""Summarize matched production-PhysX encounters; no external packages."""
import csv
import json
import statistics
import sys
from pathlib import Path

root = Path(sys.argv[1])
samples = [json.loads(line) for line in (root / "samples.jsonl").read_text().splitlines()]
assert len(samples) == 126
keys = [(s["variant"], s["build"], s["district"], s["seed"]) for s in samples]
assert len(set(keys)) == len(keys)
assert all(s["orderErrors"] == s["visibleSpawns"] == 0 for s in samples)
durable = {"Enforcer", "Freight", "Linebreaker", "Repair", "Bellkeeper"}


def aggregate(group):
    roles = [r for sample in group for r in sample["roles"]]
    def category(allowed):
        rs = [r for r in roles if r["role"] in allowed]
        total = lambda field: sum(r[field] for r in rs)
        return {
            "spawned": total("spawned"), "hit": total("hit"),
            "firstActionKills": total("firstActionKills"),
            "firstActionKillPct": round(100 * total("firstActionKills") / max(1, total("hit")), 2),
            "killedAfterHit": total("killedAfterHit"),
            "meanDamagingActionsToDeath": round(total("actionsOnKilled") / max(1, total("killedAfterHit")), 2),
            "livingActions": total("livingActions"),
            "specialistStarts": total("specialistStarts"), "specialistCompleted": total("specialistCompleted"),
            "interrupts": total("interrupts"), "carryTransitions": total("carried"),
        }
    result = {"cases": len(group), "finished": sum(s["result"] == "Drafting" for s in group)}
    for field in ["kills", "bodies", "carried", "health", "healing", "seconds"]:
        result[field] = round(statistics.mean(s[field] for s in group), 2)
    result["durable"] = category(durable)
    result["fodder"] = category({"Raider", "Shooter", "Cinderlobber", "Reclaimer", "Shield bearer"})
    result["champion"] = category({"Champion"})
    result["championKills"] = sum(s["championKills"] for s in group)
    return result


summary = {variant: aggregate([s for s in samples if s["variant"] == variant]) for variant in ["baseline", "candidate"]}
summary["byBuildDistrict"] = []
for build in sorted({s["build"] for s in samples}):
    for district in [1, 2, 3]:
        summary["byBuildDistrict"].append({"build": build, "district": district, **{
            variant: aggregate([s for s in samples if s["build"] == build and s["district"] == district and s["variant"] == variant])
            for variant in ["baseline", "candidate"]}})
(root / "summary.json").write_text(json.dumps(summary, indent=2) + "\n")
with (root / "by-build-district.csv").open("w", newline="") as f:
    writer = csv.writer(f)
    writer.writerow(["build", "district", "variant", "cases", "kills", "carried", "healing", "durable_first_action_kill_pct", "durable_actions_to_death", "specialist_starts", "specialist_completed"])
    for group in summary["byBuildDistrict"]:
        for variant in ["baseline", "candidate"]:
            g = group[variant]
            writer.writerow([group["build"], group["district"], variant, g["cases"], g["kills"], g["carried"], g["healing"], g["durable"]["firstActionKillPct"], g["durable"]["meanDamagingActionsToDeath"], g["durable"]["specialistStarts"], g["durable"]["specialistCompleted"]])
print(json.dumps({k: v for k, v in summary.items() if k != "byBuildDistrict"}, indent=2))
