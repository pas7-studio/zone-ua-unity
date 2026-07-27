#!/usr/bin/env python3
from __future__ import annotations

import argparse
import json
from dataclasses import dataclass
from pathlib import Path
from typing import Iterable


@dataclass(frozen=True)
class Rule:
    code: str
    severity: str
    message: str
    tokens: tuple[str, ...]
    allowed_fragments: tuple[str, ...] = ()


RULES = (
    Rule("LEGACY_INPUT", "error", "Use PlayerInputRouter/Input System.", (
        "Input.GetAxis", "Input.GetAxisRaw", "Input.GetButton", "Input.GetButtonDown",
        "Input.GetKey", "Input.GetKeyDown", "Input.mousePosition",
    ), ("/Editor/", "/Tests/", "LegacyUsageRule.cs")),
    Rule("DIRECT_HUD_DEPENDENCY", "error", "Gameplay must publish events instead of touching ammo HUD.", (
        "GlobalSystem.Instance.AmmoUI", ".AmmoUI.",
    ), ("/Editor/", "/Tests/")),
    Rule("TAG_BASED_COMBAT", "warning", "Use faction relationships instead of Player/Enemy tags.", (
        'CompareTag("Player")', 'CompareTag("Enemy")', 'FindGameObjectWithTag("Player")',
        'FindGameObjectsWithTag("Enemy")', "whoRecieveDamage",
    ), ("/Editor/", "/Tests/", "Bullet.cs")),
    Rule("LEGACY_HEALTH_API", "warning", "Use the modern Health API.", (
        ".setHeals(", ".restoreSomeHeals(", ".restoreDefaultHeals(", ".getHeals(",
        ".receiveDamage(", ".getIsAlive(",
    ), ("/Editor/", "/Tests/", "Health.cs")),
    Rule("GLOBAL_FIND", "warning", "Use serialized composition, registration or events.", (
        "FindObjectOfType<", "FindObjectsOfType<", "GameObject.Find(",
    ), ("/Editor/", "/Tests/", "RuntimePerformanceMonitor.cs")),
)


def iter_sources(root: Path) -> Iterable[Path]:
    for relative in (Path("Assets/Script"), Path("Assets/_ZoneUA/Runtime")):
        folder = root / relative
        if not folder.exists():
            continue
        for path in folder.rglob("*.cs"):
            normalised = path.as_posix()
            if "/Editor/" in normalised or "/Tests/" in normalised:
                continue
            yield path


def scan(root: Path) -> list[dict[str, object]]:
    findings: list[dict[str, object]] = []
    for path in iter_sources(root):
        relative = path.relative_to(root).as_posix()
        lines = path.read_text(encoding="utf-8", errors="replace").splitlines()
        for rule in RULES:
            if any(fragment.lower() in relative.lower() for fragment in rule.allowed_fragments):
                continue
            for number, source in enumerate(lines, start=1):
                if source.lstrip().startswith("//"):
                    continue
                if any(token in source for token in rule.tokens):
                    findings.append({
                        "code": rule.code,
                        "severity": rule.severity,
                        "path": relative,
                        "line": number,
                        "message": rule.message,
                        "source": source.strip(),
                    })
    return findings


def main() -> int:
    parser = argparse.ArgumentParser(description="Check Zone UA runtime code for deprecated integration paths.")
    parser.add_argument("--root", type=Path, default=Path.cwd())
    parser.add_argument("--output", type=Path, default=Path("Logs/legacy-usage.json"))
    args = parser.parse_args()

    root = args.root.resolve()
    findings = scan(root)
    output = args.output if args.output.is_absolute() else root / args.output
    output.parent.mkdir(parents=True, exist_ok=True)
    output.write_text(json.dumps({"findings": findings}, indent=2), encoding="utf-8")

    for finding in findings:
        print(f"{finding['severity'].upper()} {finding['code']} {finding['path']}:{finding['line']} {finding['message']}")

    errors = sum(1 for finding in findings if finding["severity"] == "error")
    warnings = sum(1 for finding in findings if finding["severity"] == "warning")
    print(f"Legacy usage scan: {errors} error(s), {warnings} warning(s), {len(findings)} total.")
    return 1 if errors else 0


if __name__ == "__main__":
    raise SystemExit(main())
