"""Summarize the full-run rollout using saved engine evidence; never launch games."""
import csv
import json
import statistics
from pathlib import Path

ROOT = Path(__file__).resolve().parents[3]
OUT = Path(__file__).parent


def read(path):
    return json.loads(path.read_text(encoding="utf-8-sig"))


def summarize(rows, key):
    result = []
    for label in sorted({key(r) for r in rows}):
        group = [r for r in rows if key(r) == label]
        result.append(dict(group=label, runs=len(group),
                           wins=sum(r["result"] == "Complete" for r in group),
                           timeouts=sum(r["result"] == "TimedOut" for r in group),
                           meanWave=statistics.mean(r["waveReached"] for r in group),
                           meanKills=statistics.mean(r["kills"] for r in group),
                           meanSeconds=statistics.mean(r["elapsed"] for r in group),
                           meanHealth=statistics.mean(r["health"] for r in group)))
    return result


def batch(relative, count, seed=None):
    path = ROOT / relative
    rows = [json.loads(line) for file in sorted(path.glob("worker-*.jsonl"))
            for line in file.read_text(encoding="utf-8-sig").splitlines() if line.strip()]
    assert len(rows) == count, (relative, len(rows), count)
    assert all(r["valid"] and not r["errors"] for r in rows), relative
    meta = read(path / "batch.json")
    if seed is not None:
        assert {r["seed"] for r in rows} == set(range(seed, seed + count))
        policies = ["pinball", "storm", "sever", "gun", "cutcarry", "close"]
        assert all(r["policy"] == policies[abs(r["seed"]) % 6] for r in rows)
        assert meta["waves"] == 8 and all(1 <= r["waveReached"] <= 8 for r in rows)
        assert all(r["itemChoices"] == 13 and r["augmentChoices"] == 2
                   for r in rows if r["result"] == "Complete")
        key = lambda r: r["policy"]
    else:
        cases = read(path / "cases.json")["cases"]
        ids = {c["id"]: c for c in cases}
        assert len(ids) == len(rows) == len({r["controlled"]["id"] for r in rows})
        assert {r["controlled"]["id"] for r in rows} == set(ids)
        for r in rows:
            c = ids[r["controlled"]["id"]]
            assert r["seed"] == c["seed"] and r["controlled"]["rank"] == c["rank"]
            assert r["controlled"]["ultimateEquipWave"] == 4
            assert r["controlled"]["initialHealth"] <= 400
        key = lambda r: r["controlled"]["candidate"]
    meta.update(valid=count, invalid=0, missing=0, duplicates=0,
                timeouts=sum(r["result"] == "TimedOut" for r in rows))
    if (path / "balance.json").exists():
        meta["balance"] = read(path / "balance.json")
    return rows, dict(batch=meta, groups=summarize(rows, key))


sets = {
    "baseline-fast": ("QA/Simulations/full-run-baseline-fast", 600, 82002),
    "presentation-control": ("QA/Simulations/full-run-presentation-control", 24, 82002),
    "formation-fast": ("QA/Simulations/full-run-formation-fast", 600, 82002),
    "combined-fast": ("QA/Simulations/full-run-combined-fast", 600, 82002),
    "baseline-full": ("QA/Simulations/full-run-baseline-full", 24, 83004),
    "combined-full": ("QA/Simulations/full-run-combined-full", 24, 83004),
    "tether-full": ("Build/Balance/full-run-tether-candidate", 16, None),
    "paired-full": ("Build/Balance/full-run-paired", 12, None),
}
data, summaries = {}, {}
for name, args in sets.items():
    data[name], summaries[name] = batch(*args)

fields = ["policy", "result", "waveReached", "kills", "collisions", "launches",
          "reloads", "score", "health", "itemChoices", "augmentChoices", "elapsed", "loadout"]
baseline = {r["seed"]: r for r in data["baseline-fast"]}
assert all(all(r[f] == baseline[r["seed"]][f] for f in fields)
           for r in data["presentation-control"])
summaries["presentation-control"]["exactMatchingGameplayFields"] = fields

for mode in ("fast", "full"):
    left = {r["seed"]: r for r in data["baseline-" + mode]}
    right = {r["seed"]: r for r in data["combined-" + mode]}
    assert left.keys() == right.keys()
    summaries["combined-" + mode]["matchedOutcomes"] = dict(
        unchanged=sum(left[s]["result"] == right[s]["result"] for s in left),
        newWins=sum(left[s]["result"] != "Complete" and right[s]["result"] == "Complete" for s in left),
        lostWins=sum(left[s]["result"] == "Complete" and right[s]["result"] != "Complete" for s in left))

original = read(OUT.parent / "v080-rifle-factorial/results.json")
summaries["tether-original"] = summarize(original, lambda r: r["controlled"]["candidate"])
for r in data["tether-full"]:
    old = next(x for x in original if x["controlled"]["id"] == r["controlled"]["id"])
    assert r["seed"] == old["seed"] and r["loadout"] == old["loadout"]
    assert r["controlled"]["initialHealth"] == old["controlled"]["initialHealth"] == 280

(OUT / "summary.json").write_text(json.dumps(summaries, indent=2) + "\n", encoding="utf-8")
columns = ["study", "seed", "case", "policy", "result", "waveReached", "kills", "collisions",
           "launches", "reloads", "score", "elapsed", "health", "itemChoices", "augmentChoices", "loadout"]
with (OUT / "games.csv").open("w", encoding="utf-8", newline="") as f:
    writer = csv.DictWriter(f, fieldnames=columns)
    writer.writeheader()
    for study, rows in data.items():
        for r in sorted(rows, key=lambda x: (x["seed"], x.get("controlled", {}).get("id", ""))):
            row = {k: r[k] for k in columns if k in r}
            row.update(study=study, case=r.get("controlled", {}).get("id", ""))
            writer.writerow(row)
(OUT / "full-results.json").write_text(json.dumps(
    {k: v for k, v in data.items() if k.endswith("full")}, separators=(",", ":")) + "\n", encoding="utf-8")
print(json.dumps({k: v.get("groups", v) if isinstance(v, dict) else v for k, v in summaries.items()}, indent=2))
