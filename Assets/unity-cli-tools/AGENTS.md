# unity-cli-tools Agent Instructions

These instructions apply when this `Assets/unity-cli-tools` folder is copied into
another Unity project.

## Purpose

This folder provides custom `UnityCliTool` commands for controlling Unity through
the `unity-cli` connector. Prefer these tools over manual scene, prefab, or asset
YAML edits when an equivalent tool exists.

## Required Setup

- Keep this folder under `Assets/unity-cli-tools`.
- Copy `.meta` files together with the folder.
- Install the Unity CLI connector package in the destination project.
- Keep UGUI/TextMeshPro available; Canvas automation tools reference
  `UnityEngine.UI` and `Unity.TextMeshPro`.
- Pin the connector version when reproducibility matters, for example
  `?path=unity-connector#v0.2.21`.
- Keep the local CLI binary current with `unity-cli update`.
- Wait for the Unity Editor to finish compiling before calling tools.
- Use `unity-cli --project "<project path>" status` to identify the active editor
  and bridge port.

See `docs/adoption-guide.md` for the full copy/setup checklist.

## Invocation Standard

- Use `unity-cli` for human-facing checks such as `status` and `list`.
- When multiple Unity Editors are open, select the target with
  `--project "<project path>"` or `--port <port>`.
- Use the direct bridge protocol for agent automation:
  `POST http://127.0.0.1:<port>/command`.
- Send command names in snake_case, for example `editor_state` or
  `find_game_objects`.
- Send direct bridge `params` keys in the camelCase names used by this package's
  tool handlers, for example `scenePath`, `includeInactive`, and `dryRun`.
- Check `docs/bridge-protocol.md` before writing new automation scripts.

## Safe Workflow

1. Start with `editor_state` and `list_open_scenes`.
2. Discover targets with read-only tools such as `find_asset`,
   `find_game_objects`, `dump_hierarchy`, and `inspect_components`.
3. For mutating tools, run with `dryRun: true` first whenever the tool supports
   it.
4. Apply changes only after the intended targets are clear.
5. Validate with tools such as `validate_scene_references`,
   `find_missing_references`, `validate_ui_layout`, `validate_ui_flow`, or
   `selection_state`.
6. Save scenes only when requested or when the task explicitly requires it.
7. After direct YAML edits to Unity assets, run `unity-cli reserialize` on the
   edited paths.

## UI Workflow Rules

- Prefer generic UI tools over project-specific one-off tools.
- Keep UGUI for HUD, world-space UI, mobile controls, camera-tied overlays, and
  animation/shader-heavy UI. Prefer UI Toolkit for title/menu/settings/tutorial,
  inventory/shop/list, help, and debug-panel screens.
- Create Canvas, panels, buttons, TMP text, and images with `create_ui_element`.
- Create standard Toggle, Slider, ScrollView, TMP_InputField, and TMP_Dropdown
  controls with `create_ui_control`.
- Configure responsive layout with `set_canvas_scaler`, `set_responsive_rect`,
  `set_rect_transform`, and `create_ui_layout_group`.
- For UI that must survive extreme portrait/landscape ratios, use
  `set_responsive_rect` on safe-area roots and main panels instead of relying
  only on fixed `RectTransform` sizes.
- Apply TMP style, image state, and CanvasGroup interaction state with
  `set_tmp_text_style`, `set_ui_image`, and `set_canvas_group`.
- For UI the user may tune later, create or reuse `UiTheme` and
  `UiScreenConfig` assets, bind themeable elements with `bind_ui_theme_element`,
  and apply them with `apply_ui_theme` / `apply_ui_screen_config`.
- Prefer `set_ui_theme_values` for user-requested broad visual changes so
  colors, font sizes, and shared sprites can be updated in one operation.
- Generate reusable runtime behavior with `create_mono_script`, then add it with
  `add_component` after Unity recompiles.
- Wire controller arrays and object references with `set_serialized_array` and
  `set_object_reference`.
- Wire `Button.onClick` with `set_button_on_click` only to public parameterless
  MonoBehaviour methods.
- Save or update reusable UI prefab assets with `create_ui_prefab`; instantiate
  them with `apply_ui_prefab`.
- Run `validate_ui_flow` before treating a multi-page or button-driven UI flow
  as complete.
- Run `validate_ui_theme_bindings` before treating a theme-driven UI as
  complete.
- For UI Toolkit screens, create assets and scene documents with
  `create_uitk_screen` or `create_uitk_document`.
- Use `set_uitk_uss_tokens` for broad UI Toolkit visual changes instead of
  editing USS manually.
- Wire tutorial-style UI Toolkit page flow with
  `configure_uitk_tutorial_pager`.
- Run `validate_uitk_ui` and inspect `unity-cli console --type error,warning`
  before treating a UI Toolkit screen as complete.

## Mutation Rules

- Avoid broad project-wide mutation unless the user explicitly asks for it.
- Always set filters and limits for batch tools.
- Do not overwrite serialized object references unless the tool request includes
  explicit overwrite intent.
- Do not edit generated connector package files in `Library/PackageCache`.
- Do not introduce a project-level `csc.rsp` just to support newer C# syntax;
  this package is intended to stay C# 9 compatible.
- Do not leave copied-package compiler warnings unresolved. Deprecation warnings
  usually mean the destination Unity version expects a newer Editor API; update
  the tool code instead of suppressing the warning.
- Use `unity-cli exec` for one-off inspection or temporary experiments only.
  Repeated workflows should become custom tools with discoverable parameters.
