# Adoption Guide

Use this guide when copying `Assets/unity-cli-tools` into another Unity project.

## Requirements

- Unity Editor project that can compile without errors.
- Unity CLI connector package installed in the destination project.
- UGUI/TextMeshPro available in the destination project. UI Toolkit is also
  used by the `uitk_*` tools. The package assembly references
  `UnityEngine.UI`, `Unity.TextMeshPro`, and Unity UIElements modules.
- `unity-cli` executable available on the machine for status and discovery.
- This folder copied as `Assets/unity-cli-tools`, including `.meta` files.

The editor code is kept C# 9 compatible. The destination project should not need
a `csc.rsp` language-version override for this package.

## Install The Connector

Add the connector package to `Packages/manifest.json` in the destination project:

```json
{
  "dependencies": {
    "com.youngwoocho02.unity-cli-connector": "https://github.com/youngwoocho02/unity-cli.git?path=unity-connector"
  }
}
```

If the project already has the connector, keep the existing compatible version.
After editing the manifest, open Unity and wait for package resolution and script
compilation to finish.

For reproducible tooling, pin a known connector release by appending a tag:

```json
{
  "dependencies": {
    "com.youngwoocho02.unity-cli-connector": "https://github.com/youngwoocho02/unity-cli.git?path=unity-connector#v0.2.21"
  }
}
```

Keep the local CLI binary current:

```powershell
unity-cli update --check
unity-cli update
```

If a project is pinned to a connector version, update the CLI and connector
together during a controlled tooling update.

## Recommended Editor Preference

Set **Edit > Preferences > General > Interaction Mode** to **No Throttling**.
Unity dispatches tool work on the Editor main thread, and background throttling
can delay command handling when the editor window is unfocused.

## Copy The Package

1. Copy the whole `Assets/unity-cli-tools` folder.
2. Keep `.meta` files with the copied files and folders.
3. Open the destination project in Unity.
4. Wait until the editor is not compiling.
5. Confirm the `UnityCliTools.Editor` assembly compiles without errors or
   warnings.

The package assembly definition references `UnityCliConnector.Editor`, so missing
connector installation usually appears as an assembly reference or namespace
compile error.

The package also references `UnityEngine.UI` and `Unity.TextMeshPro` for Canvas
automation tools. If the destination project removed UGUI or TextMeshPro, install
or restore those packages before using the visual/UI tools.

The package references Unity UI Toolkit modules for `UIDocument`,
`PanelSettings`, `VisualTreeAsset`, and `StyleSheet` automation. These modules
ship with modern Unity versions; if a destination Unity version has UI Toolkit
disabled or unavailable, the `uitk_*` tools will not compile there.

## Unity API Compatibility

When this folder is copied into another Unity project, treat compiler warnings in
`Assets/unity-cli-tools` as compatibility work. Do not suppress them by default.

Common examples:

- If Unity reports deprecated `PlayerSettings` calls that take
  `BuildTargetGroup`, update the tool to use
  `UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(...)`.
- If Unity reports ambiguous Unity API type names, use explicit aliases such as
  `using PackageManagerInfo = UnityEditor.PackageManager.PackageInfo;`.
- If Unity reports a C# language-version issue, keep the package syntax C# 9
  compatible instead of adding a project-wide `csc.rsp`.

After compatibility fixes, run the destination project's normal Unity compile
check again before relying on `unity-cli list`.

## Smoke Test

Run these from a terminal:

```powershell
unity-cli --project "<project path>" status
unity-cli --project "<project path>" list
```

The custom tools should appear in groups such as `scene`, `hierarchy`, `asset`,
`shader`, `prefab`, `component`, `diagnostics`, `validation`, and `automation`.
For full UI automation, confirm visual/prefab/automation commands such as
`create_ui_control`, `create_ui_layout_group`, `set_responsive_rect`,
`set_tmp_text_style`, `set_canvas_group`, `set_serialized_array`,
`create_ui_theme`, `set_ui_theme_value`, `set_ui_theme_values`,
`apply_ui_theme`,
`bind_ui_theme_element`, `validate_ui_theme_bindings`,
`create_ui_screen_config`, `apply_ui_screen_config`, `create_ui_prefab`,
`apply_ui_prefab`, `validate_ui_flow`, `create_uitk_screen`,
`create_uitk_document`, `set_uitk_uss_tokens`,
`configure_uitk_tutorial_pager`, and `validate_uitk_ui` are present in
`unity-cli list`.

For an agent-facing bridge smoke test, use the port reported by `status`:

```powershell
$port = 8094
$body = @{
  command = "editor_state"
  params = @{}
} | ConvertTo-Json -Depth 10

Invoke-RestMethod `
  -Method Post `
  -Uri "http://127.0.0.1:$port/command" `
  -ContentType "application/json" `
  -Body $body
```

Also test a read-only tool with parameters:

```powershell
$body = @{
  command = "find_asset"
  params = @{
    folder = "Assets"
    limit = 5
  }
} | ConvertTo-Json -Depth 10

Invoke-RestMethod `
  -Method Post `
  -Uri "http://127.0.0.1:$port/command" `
  -ContentType "application/json" `
  -Body $body
```

## Recommended Operating Mode

- Use `unity-cli --project "<project path>" status` to find the correct editor
  instance and port.
- Use `unity-cli --project "<project path>" list` to confirm the tool set.
- Use direct bridge calls for repeated agent automation.
- Use read-only tools before mutating tools.
- Use `dryRun: true` first for tools that support it.
- For UI flows, validate both layout and behavior with `validate_ui_layout` and
  `validate_ui_flow` before saving or handing work back.
- For UI Toolkit screens, use `create_uitk_screen` for new menu/tutorial-style
  screens, `set_uitk_uss_tokens` for broad visual changes, and
  `validate_uitk_ui` before saving or handing work back.
- For resolution-sensitive UI, apply `set_responsive_rect` to the safe-area root
  or main panel and prefer LayoutGroup/ScrollView for variable content.
- For UI the user should tune later, store global style in `UiTheme` and
  screen-specific text/layout values in `UiScreenConfig`.
- Use `unity-cli exec` for one-off inspection or temporary experiments only.
  Repeated workflows should become custom tools.
- After direct text/YAML edits to `.unity`, `.prefab`, `.asset`, or `.mat`
  files, run `unity-cli reserialize` on the edited paths.

See `docs/bridge-protocol.md` for the direct bridge request format.

## Multiple Unity Instances

When more than one Unity Editor is open, do not rely on the latest discovered
instance. Select the target explicitly:

```powershell
unity-cli --project "C:\Path\To\Project" status
unity-cli --project "C:\Path\To\Project" list
unity-cli --port 8094 status
```

Use `--project` when selecting by workspace. Use `--port` when an automation
already knows the active bridge port from a previous `status` call.

## Troubleshooting

- Tool list is missing custom tools: check Unity compile errors first, then
  confirm the connector package is installed.
- Tool list works but Unity shows warnings in this package: update the affected
  Editor API usage for the destination Unity version before treating the package
  as ready.
- HTTP bridge call fails: confirm Unity is open, the connector is running, and
  the port matches `unity-cli status`.
- Tool returns default-looking results: verify direct bridge parameter names. This
  package currently reads camelCase parameter keys such as `scenePath` and
  `includeInactive`.
- PowerShell rejects `--params` JSON: prefer the direct bridge protocol, or use a
  shell-specific escaping strategy for one-off CLI checks.
- Batchmode compile fails because the project is already open: close the editor
  or use the already-open editor through the bridge.
- Commands are slow while Unity is unfocused: enable **No Throttling** in Unity
  Preferences.
