#!/usr/bin/env python3
"""Validate a RuntimePerformanceMonitor JSON capture against CI thresholds."""

from __future__ import annotations

import argparse
import json
import math
import pathlib
import sys
from typing import Any


def percentile(values: list[float], ratio: float) -> float:
    if not values:
        return 0.0
    ordered = sorted(values)
    index = max(0, min(len(ordered) - 1, math.ceil(ratio * len(ordered)) - 1))
    return ordered[index]


def load_samples(path: pathlib.Path) -> list[dict[str, Any]]:
    payload = json.loads(path.read_text(encoding="utf-8"))
    samples = payload.get("samples")
    if not isinstance(samples, list) or not samples:
        raise ValueError("capture contains no samples")
    return samples


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("capture", type=pathlib.Path)
    parser.add_argument("--minimum-average-fps", type=float, default=0.0)
    parser.add_argument("--maximum-p95-main-thread-ms", type=float, default=0.0)
    parser.add_argument("--maximum-p95-render-thread-ms", type=float, default=0.0)
    parser.add_argument("--maximum-gc-bytes", type=int, default=0)
    parser.add_argument("--maximum-reserved-memory-bytes", type=int, default=0)
    args = parser.parse_args()

    try:
        samples = load_samples(args.capture)
    except (OSError, ValueError, json.JSONDecodeError) as error:
        print(f"ERROR: {error}")
        return 2

    average_fps = sum(float(sample.get("framesPerSecond", 0.0)) for sample in samples) / len(samples)
    p95_main = percentile([float(sample.get("mainThreadMilliseconds", 0.0)) for sample in samples], 0.95)
    p95_render = percentile([float(sample.get("renderThreadMilliseconds", 0.0)) for sample in samples], 0.95)
    maximum_gc = max(int(sample.get("gcAllocatedBytes", 0)) for sample in samples)
    maximum_memory = max(int(sample.get("totalReservedMemoryBytes", 0)) for sample in samples)

    print(f"samples={len(samples)}")
    print(f"average_fps={average_fps:.3f}")
    print(f"p95_main_thread_ms={p95_main:.3f}")
    print(f"p95_render_thread_ms={p95_render:.3f}")
    print(f"maximum_gc_bytes={maximum_gc}")
    print(f"maximum_reserved_memory_bytes={maximum_memory}")

    failures: list[str] = []
    if args.minimum_average_fps > 0 and average_fps < args.minimum_average_fps:
        failures.append(f"average FPS {average_fps:.3f} < {args.minimum_average_fps:.3f}")
    if args.maximum_p95_main_thread_ms > 0 and p95_main > args.maximum_p95_main_thread_ms:
        failures.append(f"p95 main thread {p95_main:.3f} ms > {args.maximum_p95_main_thread_ms:.3f} ms")
    if args.maximum_p95_render_thread_ms > 0 and p95_render > args.maximum_p95_render_thread_ms:
        failures.append(f"p95 render thread {p95_render:.3f} ms > {args.maximum_p95_render_thread_ms:.3f} ms")
    if args.maximum_gc_bytes > 0 and maximum_gc > args.maximum_gc_bytes:
        failures.append(f"maximum GC {maximum_gc} > {args.maximum_gc_bytes} bytes")
    if args.maximum_reserved_memory_bytes > 0 and maximum_memory > args.maximum_reserved_memory_bytes:
        failures.append(f"maximum reserved memory {maximum_memory} > {args.maximum_reserved_memory_bytes} bytes")

    if failures:
        for failure in failures:
            print(f"FAIL: {failure}")
        return 1

    print("PASS: performance capture is within configured thresholds")
    return 0


if __name__ == "__main__":
    sys.exit(main())
