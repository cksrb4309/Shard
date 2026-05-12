# unity-cli-tools

Reusable package and catalog workspace for `UnityCliTool`-based custom tools.

Target Unity Editor version: `6000.3.11f`.

This repository is organized for two parallel concerns:

- shipping real Editor-side tools as a Unity package
- keeping LLM-friendly documentation and metadata beside the code

## Structure

```text
Editor/        Editor-only package code
Runtime/       Runtime helper components used by generated UI
Tests/         Editor tests for shared code and tools
docs/          Human-readable docs and category guides
catalog/       Machine-readable tool metadata and aliases
prompts/       Workflow prompts for scene/prefab/validation tasks
scripts/       Helper scripts for syncing docs and catalog data
samples/       Templates and examples for new tools
```

## Design Rules

- One command should map to one tool class.
- Tool categories should match both folder names and `Group` values.
- Shared constants stay in `Editor/Core`.
- Shared helper code stays in `Editor/Infrastructure`.
- `unity-cli list` should remain the source of truth for discoverable tool schemas.
- `catalog/tools.json` should be kept in sync with actual tool metadata.

## Using In Another Unity Project

Copy the whole `Assets/unity-cli-tools` folder, including `.meta` files, into the
destination project and install the Unity CLI connector package there.

- `AGENTS.md` contains agent-facing operating rules.
- `docs/adoption-guide.md` contains the copy/setup/smoke-test checklist.
- `docs/bridge-protocol.md` contains the recommended direct bridge calling
  convention for automation.
- `docs/authoring-guide.md` contains custom tool authoring rules.
- `docs/tools/README.md` explains how tool docs stay aligned with
  `unity-cli list`.

## Operating Rules

- Keep the `unity-cli` binary current with `unity-cli update`; use
  `unity-cli update --check` when you only need to inspect availability.
- Pin the connector package with a git tag, for example
  `?path=unity-connector#v0.2.21`, when a project needs reproducible tooling.
- Use `unity-cli --project "<project path>" ...` or `unity-cli --port <port> ...`
  when multiple Unity Editors are open.
- Use `exec` for one-off inspection or temporary experiments. Promote repeated
  workflows into custom tools.
- After direct YAML edits to `.unity`, `.prefab`, `.asset`, or `.mat` files, run
  `unity-cli reserialize` on the edited assets.
- Treat copied-package compiler warnings as compatibility work, not noise. When
  Unity deprecates Editor APIs, update tools to the current API for that Unity
  version, for example `NamedBuildTarget`-based `PlayerSettings` calls in Unity 6.

## Current Status

This repository now includes working `UnityCliTool` implementations for scene,
hierarchy, asset, shader, prefab, component, diagnostics, validation, automation,
build preflight, project settings, package inventory, runtime state, visual/UI
inspection, Canvas/RectTransform/TMP UI creation and styling, layout groups,
UGUI/TMP controls, responsive safe-area/aspect-ratio fitting, CanvasGroup state,
Button event wiring, ScriptableObject-driven UI themes and screen configs,
serialized controller array wiring, UI prefab creation/application, UI flow
validation, UI Toolkit screen/UIDocument/USS token creation and validation,
reusable MonoBehaviour script templates, and input probes.

## Next Steps

1. Add matching docs under `docs/tools/<category>/`.
2. Add Editor tests for shared helpers and representative tool handlers.
3. Fix parameter casing consistency between `unity-cli list` schemas and runtime
   `ToolParams` keys.
