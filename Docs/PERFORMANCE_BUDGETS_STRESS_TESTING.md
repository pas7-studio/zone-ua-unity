# Performance budgets and stress testing

This module provides repeatable performance capture for the Zone UA Unity project. It is intentionally separated from gameplay balancing and visual redesign.

## Assets

Create the following assets in Unity:

- `Create -> Zone UA -> Performance -> Budget Profile`
- `Create -> Zone UA -> Performance -> Stress Scenario`

The budget profile defines thresholds for:

- target frames per second;
- main-thread time;
- render-thread time;
- GC allocation per frame;
- reserved memory;
- tracked pooled instances;
- scheduled delayed releases;
- active NPCs;
- active projectiles;
- generated objects.

The stress scenario defines:

- warm-up duration;
- capture duration;
- requested NPC count;
- projectile spawn rate;
- generated-object load;
- whether the world is regenerated before capture;
- the budget profile used to evaluate the run.

## Scene setup

Use:

`Zone UA -> Performance -> Create Stress Harness In Open Scene`

The command creates a `ZoneUA_PerformanceStressHarness` object with:

- `RuntimePerformanceMonitor`;
- `PerformanceStressRunner`.

Assign in the Inspector:

### RuntimePerformanceMonitor

- Budget Profile;
- Sample Interval;
- Overlay visibility;
- output base name.

### PerformanceStressRunner

- Stress Scenario;
- Runtime Performance Monitor;
- Map Generator;
- representative NPC prefab;
- representative projectile prefab;
- optional lightweight generated-load prefab;
- optional spawn root.

Do not use placeholder prefabs for the final baseline. The stress scene must use the same production prefabs and definitions as normal gameplay.

## Capture lifecycle

A run performs:

1. optional world regeneration;
2. NPC and generated-object load creation;
3. warm-up;
4. sustained projectile load;
5. timed capture;
6. final sample capture;
7. JSON and CSV export;
8. cleanup through the runtime object pool when possible.

Reports are written under `Application.persistentDataPath`.

The JSON output is intended for CI and automated comparison. The CSV output is intended for spreadsheets and manual analysis.

## Runtime counters

`RuntimePerformanceMonitor` records:

- instantaneous FPS;
- main-thread milliseconds;
- render-thread milliseconds;
- allocated GC bytes;
- total reserved memory;
- pool instance count;
- scheduled pool releases;
- active NPC count;
- active projectile count;
- generated-object count.

`PerformanceCaptureStatistics` calculates average FPS, p95 main/render thread time and maximum GC/memory values from a complete capture.

The overlay is intended only for Editor and Development builds. Disable it in production builds after the baseline is established.

## Initial budgets

The default profile values are starting guardrails, not verified hardware targets. Record baselines on the actual target device classes before tightening them.

Recommended baseline matrix:

- development desktop;
- minimum supported desktop;
- representative Android device;
- representative high-load world seed;
- representative combat encounter;
- worst-case NPC density.

Track median and high-percentile behaviour. A single fast frame does not prove that a scenario is stable.

## Required validation

- Open the project in Unity 6000.5.5f1.
- Confirm `ProfilerRecorder` counters are available on the target platform.
- Run all `PerformanceBudgetTests`.
- Create the stress harness in a dedicated development scene.
- Assign production NPC, projectile and world prefabs.
- Run the scenario for at least 30 seconds after warm-up.
- Verify JSON and CSV files are produced.
- Verify pooled instances return correctly after cleanup.
- Verify no projectile from an earlier lease is released during a later lease.
- Verify the stress harness does not remain in production scenes.
- Record CPU, memory, GC and active-object baselines before optimisation.

## CI use

The pure budget evaluator is covered by EditMode tests and can participate in the existing GameCI job.

Use the report checker after downloading a JSON capture:

```bash
python scripts/check_performance_capture.py performance-capture.json \
  --minimum-average-fps 60 \
  --maximum-p95-main-thread-ms 16.67 \
  --maximum-p95-render-thread-ms 16.67 \
  --maximum-gc-bytes 1024 \
  --maximum-reserved-memory-bytes 2147483648
```

A future licensed PlayMode performance job should:

1. open a dedicated stress scene;
2. run the configured scenario;
3. upload JSON and CSV reports;
4. run `check_performance_capture.py` with platform-specific thresholds;
5. fail only on sustained or statistically meaningful regressions.

Do not fail CI on one noisy frame. Use a stable capture window and target-hardware-specific thresholds.
