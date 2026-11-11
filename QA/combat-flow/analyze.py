"""Collect the combat-flow acceptance evidence from retained local test outputs."""
import hashlib
import json
from pathlib import Path
import shutil
import subprocess

ROOT = Path(__file__).resolve().parents[2]
OUT = Path(__file__).resolve().parent


def read(path):
    return json.loads(path.read_text(encoding="utf-8-sig"))


def save(name, value):
    (OUT / name).write_text(json.dumps(value, indent=2) + "\n", encoding="utf-8")


def assembly(folder):
    path = ROOT / "Build" / folder / "WalkingHard_Data/Managed/Assembly-CSharp.dll"
    return hashlib.sha256(path.read_bytes()).hexdigest()


def runs(folder, first=83001, count=12):
    directory = ROOT / "QA/Simulations" / folder
    rows = sorted((json.loads(line) for path in directory.glob("worker-*.jsonl")
                   for line in path.read_text(encoding="utf-8-sig").splitlines() if line), key=lambda r: r["seed"])
    assert len(rows) == count and {r["seed"] for r in rows} == set(range(first, first + count))
    assert all(r["valid"] for r in rows)
    return rows, read(directory / "batch.json")


baseline, baseline_meta = runs("flow-release-12")
candidate, candidate_meta = runs("flow-candidate-12")
acceptance, acceptance_meta = runs("flow-acceptance-6", 83007, 6)
save("ordinary-baseline.json", baseline)
save("ordinary-candidate.json", candidate)
save("ordinary-acceptance.json", acceptance)
comparison = []
for before, after in zip(baseline, candidate):
    assert before["seed"] == after["seed"] and before["policy"] == after["policy"]
    comparison.append({"seed": before["seed"], "policy": before["policy"],
                       "before": {k: before[k] for k in ("result", "waveReached", "elapsed", "kills", "health")},
                       "after": {k: after[k] for k in ("result", "waveReached", "elapsed", "kills", "health")}})
regressions = {}
for prefix, expected in (("flow-core", 15), ("flow-variety", 50), ("flow-full", 1), ("flow-final-core", 15), ("flow-final-full", 1)):
    paths = list((ROOT / "QA").glob(prefix + "-*.json")) or list((ROOT / "Build/Combat-flow-raw").glob(prefix + "-*.json"))
    rows = [read(path) for path in sorted(paths)]
    assert len(rows) == expected, (prefix, len(rows), expected)
    assert all(row["passed"] and row["orderErrors"] == 0 and row["visibleSpawns"] == 0 for row in rows)
    regressions[prefix] = [{k: r[k] for k in ("build", "seed", "passed", "kills", "collisions", "health", "mechanicAssertions", "orderErrors", "visibleSpawns", "itemChoices", "augmentChoices") if k in r} for r in rows]
save("regressions.json", regressions)
fixture_before = read(ROOT / "Build/Flow-baseline-review/report.json")
fixture_after = read(ROOT / "Build/Combat-flow-acceptance-review/report.json")
assert fixture_after["passed"]
save("spacing-baseline.json", fixture_before)
save("native-review.json", fixture_after)
normal_review = read(ROOT / "Build/Combat-flow-normal-review/report.json")
assert normal_review["passed"]
save("native-ordinary-review.json", normal_review)
for name in ("combat-wave-6.png", "run-result.png"):
    shutil.copy2(ROOT / "Build/Combat-flow-normal-review" / name, OUT / ("ordinary-" + name))
for path in (ROOT / "Build/Combat-flow-acceptance-review").glob("*.png"):
    shutil.copy2(path, OUT / path.name)
save("summary.json", {
    "date": "2026-11-10",
    "ordinary": comparison,
    "baselineWins": sum(r["result"] == "Complete" for r in baseline),
    "candidateWins": sum(r["result"] == "Complete" for r in candidate),
    "baselineTimeouts": sum(r["elapsed"] >= 600 for r in baseline),
    "candidateTimeouts": sum(r["elapsed"] >= 600 for r in candidate),
    "finalAcceptance": [{"seed": r["seed"], "policy": r["policy"], "result": r["result"], "elapsed": r["elapsed"], "valid": r["valid"]} for r in acceptance],
    "scope": "Two ordinary seeds per policy, Full physics, unchanged health and automated drafts. Small regression screen; not a win-rate estimate. Other test processes ran concurrently; no performance claim.",
    "regressionScenarios": sum(len(v) for v in regressions.values()),
    "nativeAssertions": fixture_after["assertions"],
    "productionAssertions": fixture_after["productionAssertions"],
})
files = sorted(set(subprocess.check_output(["git", "diff", "--name-only", "4dce44bd752fc28f1f372a0e6cfafa765a9da95d"], cwd=ROOT, text=True).splitlines()
                   + subprocess.check_output(["git", "ls-files", "--others", "--exclude-standard", "Assets"], cwd=ROOT, text=True).splitlines()))
save("provenance.json", {
    "baseCommit": "4dce44bd752fc28f1f372a0e6cfafa765a9da95d",
    "sourceFiles": {f: subprocess.check_output(["git", "hash-object", "--", f], cwd=ROOT, text=True).strip()
                    for f in files if f.startswith("Assets/") and f.endswith((".cs", ".meta"))},
    "assemblies": {name: assembly(name) for name in ("FullRun-batched", "Flow-baseline", "Flow-verified", "Combat-flow", "Combat-flow-final", "Combat-flow-acceptance")},
    "baselineBatch": baseline_meta,
    "candidateBatch": candidate_meta,
    "acceptanceBatch": acceptance_meta,
    "buildRoles": {
        "FullRun-batched": "Published v0.8.0 ordinary-run baseline.",
        "Flow-baseline": "v0.8.0 combat plus isolated spacing recorder; baseline report predates assertions/UI capture.",
        "Flow-verified": "Core 15 regressions. Same combat rules; before final text, label bounds and test-loop assertion consolidation.",
        "Combat-flow": "50 variety and 12 ordinary runs. Final gameplay; before run-summary wait-label text change.",
        "Combat-flow-final": "Final presentation, native flow and normal full-run review, rendered tutorial and full progression; before the ultimate reach correction.",
        "Combat-flow-acceptance": "Final source including ultimate rarity reach: repeated 15 core checks, full progression, six ordinary Full runs and native flow review.",
    },
    "fixtureMetricCaveat": "Spacing waits are sampled from ActionStatus. A carried status can include the initial 0.15s recovery; long Steel Jab waits are unaffected by this subsecond measurement limit.",
})
print(json.dumps(read(OUT / "summary.json"), indent=2))
