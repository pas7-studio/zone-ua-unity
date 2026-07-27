#!/usr/bin/env python3
"""Fast repository checks that do not require a Unity installation."""

from __future__ import annotations

import json
import pathlib
import re
import sys
from dataclasses import dataclass

ROOT = pathlib.Path(__file__).resolve().parents[1]
ASSETS = ROOT / "Assets"
GUID_RE = re.compile(r"^guid:\s*([0-9a-fA-F]{32})\s*$", re.MULTILINE)
REQUIRED_ACTIONS = {
    "Move",
    "Look",
    "Sprint",
    "Fire",
    "Reload",
    "SwitchFireMode",
    "Weapon1",
    "Weapon2",
    "HideWeapon",
}


@dataclass(frozen=True)
class Issue:
    severity: str
    code: str
    path: str
    message: str


def relative(path: pathlib.Path) -> str:
    return path.relative_to(ROOT).as_posix()


def check_meta_files(issues: list[Issue]) -> None:
    for path in ASSETS.rglob("*"):
        if path.name.startswith(".") or path.suffix == ".meta":
            continue
        meta = pathlib.Path(f"{path}.meta")
        if not meta.is_file():
            issues.append(Issue("error", "META_MISSING", relative(path), "Asset or folder has no .meta file."))


def check_guids(issues: list[Issue]) -> None:
    owner_by_guid: dict[str, pathlib.Path] = {}
    for meta in ASSETS.rglob("*.meta"):
        text = meta.read_text(encoding="utf-8", errors="replace")
        match = GUID_RE.search(text)
        if not match:
            issues.append(Issue("error", "META_GUID_MISSING", relative(meta), "Meta file has no valid 32-character GUID."))
            continue
        guid = match.group(1).lower()
        previous = owner_by_guid.get(guid)
        if previous is not None:
            issues.append(
                Issue(
                    "error",
                    "META_GUID_DUPLICATE",
                    relative(meta),
                    f"GUID is already used by {relative(previous)}.",
                )
            )
        else:
            owner_by_guid[guid] = meta


def load_json(path: pathlib.Path, issues: list[Issue], code: str) -> object | None:
    try:
        return json.loads(path.read_text(encoding="utf-8"))
    except (OSError, json.JSONDecodeError) as error:
        issues.append(Issue("error", code, relative(path), str(error)))
        return None


def check_asmdefs(issues: list[Issue]) -> None:
    owner_by_name: dict[str, pathlib.Path] = {}
    for path in ASSETS.rglob("*.asmdef"):
        payload = load_json(path, issues, "ASMDEF_INVALID_JSON")
        if not isinstance(payload, dict):
            continue
        name = payload.get("name")
        if not isinstance(name, str) or not name.strip():
            issues.append(Issue("error", "ASMDEF_NAME_MISSING", relative(path), "Assembly definition has no name."))
            continue
        previous = owner_by_name.get(name)
        if previous is not None:
            issues.append(
                Issue("error", "ASMDEF_NAME_DUPLICATE", relative(path), f"Assembly name is already used by {relative(previous)}.")
            )
        else:
            owner_by_name[name] = path


def check_input_actions(issues: list[Issue]) -> None:
    path = ASSETS / "_ZoneUA" / "Input" / "ZoneUAInput.inputactions"
    if not path.is_file():
        issues.append(Issue("error", "INPUT_ACTIONS_MISSING", relative(path), "Central InputActionAsset is missing."))
        return
    payload = load_json(path, issues, "INPUT_ACTIONS_INVALID_JSON")
    if not isinstance(payload, dict):
        return
    maps = payload.get("maps")
    if not isinstance(maps, list):
        issues.append(Issue("error", "INPUT_MAPS_MISSING", relative(path), "InputActionAsset has no maps array."))
        return
    player = next((item for item in maps if isinstance(item, dict) and item.get("name") == "Player"), None)
    if player is None:
        issues.append(Issue("error", "INPUT_PLAYER_MAP_MISSING", relative(path), "Player action map is missing."))
        return
    actions = player.get("actions")
    names = {item.get("name") for item in actions if isinstance(item, dict)} if isinstance(actions, list) else set()
    for missing in sorted(REQUIRED_ACTIONS - names):
        issues.append(Issue("error", "INPUT_ACTION_MISSING", relative(path), f"Required action is missing: {missing}."))


def check_manifest(issues: list[Issue]) -> None:
    path = ROOT / "Packages" / "manifest.json"
    payload = load_json(path, issues, "PACKAGE_MANIFEST_INVALID_JSON")
    if not isinstance(payload, dict):
        return
    dependencies = payload.get("dependencies")
    if not isinstance(dependencies, dict) or "com.unity.inputsystem" not in dependencies:
        issues.append(Issue("error", "INPUT_PACKAGE_MISSING", relative(path), "Unity Input System package is not declared."))


def check_project_version(issues: list[Issue]) -> None:
    path = ROOT / "ProjectSettings" / "ProjectVersion.txt"
    if not path.is_file():
        issues.append(Issue("error", "PROJECT_VERSION_MISSING", relative(path), "ProjectVersion.txt is missing."))
        return
    text = path.read_text(encoding="utf-8", errors="replace")
    if "m_EditorVersion:" not in text:
        issues.append(Issue("error", "PROJECT_VERSION_INVALID", relative(path), "Editor version entry is missing."))
    elif "m_EditorVersion: 2022.2.8f1" in text:
        issues.append(
            Issue(
                "warning",
                "PROJECT_VERSION_LEGACY",
                relative(path),
                "Repository still declares Unity 2022.2.8f1; commit Unity 6000.5.5f1 migration files after opening the project locally.",
            )
        )


def main() -> int:
    issues: list[Issue] = []
    check_meta_files(issues)
    check_guids(issues)
    check_asmdefs(issues)
    check_input_actions(issues)
    check_manifest(issues)
    check_project_version(issues)

    for issue in issues:
        print(f"::{issue.severity} file={issue.path},title={issue.code}::{issue.message}")

    errors = sum(issue.severity == "error" for issue in issues)
    warnings = sum(issue.severity == "warning" for issue in issues)
    print(f"Unity repository validation: {errors} error(s), {warnings} warning(s).")
    return 1 if errors else 0


if __name__ == "__main__":
    sys.exit(main())
