"""Rebuild the descriptive matched-Expedition report from the retained raw rows."""
import json
from pathlib import Path
from statistics import mean

ROOT = Path(__file__).resolve().parent


def read_json(path):
    return json.loads(path.read_text(encoding="utf-8-sig"))


def load(name):
    directory = ROOT / "balance" / name
    rows = [json.loads(line) for path in sorted(directory.glob("worker-*.jsonl"))
            for line in path.read_text(encoding="utf-8-sig").splitlines() if line.strip()]
    keys = [(row["seed"], row["policy"]) for row in rows]
    if len(rows) != 18 or len(set(keys)) != len(keys):
        raise ValueError(f"Incomplete or duplicated {name} batch")
    if any(not row["valid"] or row["errors"] or row["result"] == "TimedOut" for row in rows):
        raise ValueError(f"Invalid or timed-out {name} run")
    return read_json(directory / "batch.json"), dict(zip(keys, rows))


def summarize(rows):
    rows = list(rows)
    return {
        "runs": len(rows),
        "complete": sum(r["result"] == "Complete" for r in rows),
        "defeated": sum(r["result"] == "Defeated" for r in rows),
        "meanFinalIntegrity": round(mean(r["health"] for r in rows), 2),
        "meanCombatSeconds": round(mean(r["elapsed"] for r in rows), 2),
        "meanKills": round(mean(r["kills"] for r in rows), 2),
        "crewRepairs": sum(r["repairs"] for r in rows),
        "crewThrows": sum(r["throws"] for r in rows),
        "crewRetrievals": sum(r["retrievals"] for r in rows),
        "foremanVents": sum(r["foremanVents"] for r in rows),
    }


def main():
    old_meta, old = load("baseline")
    new_meta, new = load("candidate")
    fields = ("mode", "runs", "workers", "firstSeed", "policy", "waves",
              "maxCombatSeconds", "assemblySha256", "expedition")
    if any(old_meta[k] != new_meta[k] for k in fields) or old.keys() != new.keys():
        raise ValueError("Comparison inputs do not match")
    if not old_meta["legacyExpedition"] or new_meta["legacyExpedition"]:
        raise ValueError("Expected legacy/new itinerary comparison")
    cases = []
    for key in sorted(old):
        before, after = old[key], new[key]
        cases.append({
            "seed": key[0], "policy": key[1],
            "baselineResult": before["result"], "candidateResult": after["result"],
            "baselineWave": before["waveReached"], "candidateWave": after["waveReached"],
            "baselineIntegrity": round(before["health"], 2),
            "candidateIntegrity": round(after["health"], 2),
            "candidateDestinations": after["itinerary"]["chosen"],
        })
    policies = [{"policy": policy,
                 "baseline": summarize(r for r in old.values() if r["policy"] == policy),
                 "candidate": summarize(r for r in new.values() if r["policy"] == policy)}
                for policy in sorted({key[1] for key in old})]
    report = {
        "scope": "36 normal-health Full-physics Expeditions, matched seeds and draft policies; 18 per ruleset, three per policy.",
        "limits": "Descriptive screening only. Different destinations, enemies, event spending and later offers all contribute. Policies use ordinary drafts, automated ultimates and simple route/event heuristics. Final integrity is not normalized for gear maximum and includes defeats. Combat time is simulated time, not a performance benchmark. No inference of equal build strength or causal win-rate improvement; Full physics is not bit-identical across sessions.",
        "settings": {k: old_meta[k] for k in fields},
        "baseline": summarize(old.values()), "candidate": summarize(new.values()),
        "bothComplete": sum(c["baselineResult"] == c["candidateResult"] == "Complete" for c in cases),
        "lossToWin": sum(c["baselineResult"] == "Defeated" and c["candidateResult"] == "Complete" for c in cases),
        "winToLoss": sum(c["baselineResult"] == "Complete" and c["candidateResult"] == "Defeated" for c in cases),
        "policies": policies, "cases": cases,
    }
    (ROOT / "balance" / "report.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
    print(json.dumps({k: report[k] for k in ("baseline", "candidate", "bothComplete", "lossToWin", "winToLoss")}, indent=2))


if __name__ == "__main__":
    main()
