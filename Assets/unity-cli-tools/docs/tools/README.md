# Tool Documentation

Tool documentation explains how to combine this package's custom tools with
`unity-cli` built-in commands.

## Source Of Truth

`unity-cli list` is the source of truth for discoverable command names, groups,
descriptions, and parameter schemas. Tool docs must be updated whenever the
corresponding `[UnityCliTool]`, nested `Parameters` class, or runtime
`ToolParams` keys change.

## Documentation Checklist

Each tool page should include:

- Purpose and when to use it.
- Required and optional parameters.
- Direct bridge parameter example.
- Output shape and stable fields agents can chain into later calls.
- Safety notes, especially mutation and `dryRun` behavior.
- Related validation commands.
- Example `unity-cli` invocation for manual use.

## Recommended Read Flow

1. Start with diagnostics: `editor_state`, `selection_state`,
   `list_open_scenes`, `play_mode_state`.
2. Discover targets: `find_asset`, `find_game_objects`, `dump_hierarchy`,
   `inspect_components`, `list_cameras`, `inspect_ui_canvases`,
   `inspect_ui_hierarchy`, `find_ui_elements`.
3. Mutate narrowly: scene, component, prefab, visual, or automation tools with
   explicit filters and `dryRun` first.
4. For Canvas/UGUI work, inspect before editing with `inspect_rect_transform` and
   `inspect_canvas_scaler`, create or mutate with `create_ui_element`,
   `create_ui_control`, `create_ui_layout_group`, `set_canvas_scaler`,
   `set_rect_transform`, `set_responsive_rect`, `set_ui_text`,
   `set_tmp_text_style`, `set_ui_image`, `set_canvas_group`, or
   `set_button_state`.
5. For UI Toolkit screen work, create assets and documents with
   `create_uitk_screen` or `create_uitk_document`, tune visual tokens with
   `set_uitk_uss_tokens`, wire tutorial paging with
   `configure_uitk_tutorial_pager`, then validate with `validate_uitk_ui`.
6. Validate: `validate_scene_references`, `find_missing_references`,
   `find_missing_scripts`, `validate_ui_layout`, `validate_ui_flow`,
   `validate_uitk_ui`, `build_preflight`.
7. Inspect setup: `inspect_project_settings`, `inspect_tags_layers`,
   `list_packages`, `inspect_import_settings`, `addressables_state`.
8. Use built-in CLI commands for broader editor operations: `console`, `test`,
   `reserialize`, `profiler`, `menu`, and `editor`.

## Canvas/UI Tool Set

- `inspect_ui_canvases`: list Canvas objects.
- `inspect_ui_hierarchy`: inspect UI hierarchy under Canvas or UI root.
- `find_ui_elements`: search by name, TMP text, component, interactable, or
  raycast target.
- `inspect_rect_transform`: inspect anchors, pivot, offsets, size, and corners.
- `set_rect_transform`: update layout fields with `dryRun` support.
- `inspect_canvas_scaler`: inspect CanvasScaler configuration.
- `set_canvas_scaler`: set responsive CanvasScaler configuration.
- `set_responsive_rect`: add or configure runtime responsive fitting for
  Safe Area, centered max-size panels, stretch-to-parent, or aspect-fit panels.
- `create_ui_element`: create reusable Canvas, panel, image, button, TMP text, or
  empty RectTransform elements.
- `create_ui_control`: create reusable Toggle, Slider, ScrollView,
  TMP_InputField, or TMP_Dropdown controls.
- `create_ui_layout_group`: add or update VerticalLayoutGroup,
  HorizontalLayoutGroup, GridLayoutGroup, ContentSizeFitter, LayoutElement, and
  AspectRatioFitter.
- `validate_ui_layout`: detect common Canvas, RectTransform, raycast, and
  EventSystem issues.
- `set_ui_text`: update `TMP_Text` content with `dryRun` support.
- `set_tmp_text_style`: update TMP font asset, font size, color, alignment,
  wrapping, and overflow mode.
- `set_ui_image`: update `Image` sprite, color, material, type, or raycast
  target with `dryRun` support.
- `set_canvas_group`: update CanvasGroup alpha, interactable, blocksRaycasts,
  and ignoreParentGroups.
- `create_ui_theme`: create a `UiTheme` ScriptableObject for colors, fonts,
  sprites, and shared UI style tokens.
- `set_ui_theme_value`: update one `UiTheme` token, such as `primary`,
  `panel`, `title_size`, `body_font`, or `button_sprite`.
- `set_ui_theme_values`: update multiple color, scalar, font, or sprite tokens
  in one call, then optionally apply the theme to a UI root immediately.
- `bind_ui_theme_element`: attach or update `UiThemeBinding` on a scene UI
  element.
- `apply_ui_theme`: assign a `UiTheme` to a UI root and apply all bindings.
- `validate_ui_theme_bindings`: check theme applier, theme reference, binding
  coverage, and invalid roles.
- `create_ui_screen_config`: create a `UiScreenConfig` ScriptableObject for
  screen-specific title, page text, labels, and responsive panel values.
- `apply_ui_screen_config`: apply screen config text and responsive panel values
  to scene UI.
- `set_button_state`: inspect or update `Button` interactable state and target
  graphic raycast behavior.
- `set_button_on_click`: inspect, add, clear, or replace persistent Button
  `onClick` listeners for public parameterless MonoBehaviour methods.
- `create_mono_script`: create reusable MonoBehaviour scripts from safe
  templates, including a generic UI pager template.
- `set_serialized_array`: resize serialized arrays/lists and assign scene or
  asset object references for reusable UI controllers.
- `create_ui_prefab`: save a UI hierarchy as a prefab or update an existing UI
  prefab asset.
- `apply_ui_prefab`: instantiate a UI prefab under a scene parent.
- `validate_ui_flow`: validate CanvasScaler, EventSystem, Button onClick
  listeners, controller references, pages array, and active state together.

## UI Toolkit Tool Set

Use UI Toolkit for screen-like UI such as title menus, settings, tutorial pages,
inventory/shop lists, help screens, and debug panels. Keep UGUI for HUD,
world-space UI, joystick/touch controls, camera-tied overlays, and animation or
shader-heavy UI.

- `create_uitk_screen`: generate UXML, USS, PanelSettings, a scene UIDocument,
  stylesheet binding, and optional tutorial pager wiring in one operation.
- `create_uitk_document`: instantiate a UIDocument GameObject from existing
  UXML/USS/PanelSettings assets.
- `set_uitk_uss_tokens`: batch update USS custom-property tokens such as
  `uitk-primary`, `uitk-panel`, `uitk-title-size`, and `uitk-body-size`.
- `configure_uitk_tutorial_pager`: add or update `UitkTutorialPager` page and
  button element bindings.
- `validate_uitk_ui`: verify UIDocument references, required UXML element
  names, required USS tokens, stylesheet binding, and tutorial pager targets.

## UI Toolkit Screen Pattern

For menu/tutorial-style runtime screens, prefer this sequence:

1. Run `create_uitk_screen` with `dryRun: true` to preview generated asset paths,
   page names, and document object creation.
2. Run `create_uitk_screen` with `dryRun: false` after checking the target scene
   and asset folder.
3. Use `set_uitk_uss_tokens` for broad visual changes instead of editing USS by
   hand.
4. If the UXML already exists, use `create_uitk_document` and
   `configure_uitk_tutorial_pager` separately.
5. Run `validate_uitk_ui` with the required element names and token names before
   treating the screen as complete.
6. Check `unity-cli console --type error,warning` after USS changes because
   malformed USS can import successfully but still emit parser errors.

Example direct bridge call:

```json
{
  "command": "create_uitk_screen",
  "params": {
    "scenePath": "Assets/Scenes/Title.unity",
    "name": "TitleTutorial",
    "folderPath": "Assets/UI/UITK",
    "title": "How To Play",
    "pageTexts": [
      "Move with the controls shown on screen.",
      "Avoid hazards and follow objectives.",
      "Press X to close this tutorial."
    ],
    "createDocumentObject": true,
    "addTutorialPager": true,
    "overwrite": false,
    "dryRun": false
  }
}
```

Example token update:

```json
{
  "command": "set_uitk_uss_tokens",
  "params": {
    "ussPath": "Assets/UI/UITK/TitleTutorial.uss",
    "tokens": {
      "uitk-primary": "rgb(23, 101, 84)",
      "uitk-panel": "rgb(248, 244, 232)",
      "uitk-title-size": "48px"
    },
    "dryRun": false
  }
}
```

## Generic UI Runtime Pattern

For reusable UI flows, avoid hardcoding a project-specific tool. Use the generic
sequence:

1. Create layout objects with `create_ui_element`.
2. Create standard controls with `create_ui_control`.
3. Configure responsive scaling and layout with `set_canvas_scaler`,
   `set_responsive_rect`, `set_rect_transform`, and `create_ui_layout_group`.
4. Apply presentation with `set_ui_text`, `set_tmp_text_style`,
   `set_ui_image`, and `set_canvas_group`.
5. Create or assign a theme with `create_ui_theme`, `bind_ui_theme_element`, and
   `apply_ui_theme`.
6. Store screen-specific copy and layout limits in `create_ui_screen_config`,
   then apply them with `apply_ui_screen_config`.
7. Generate a reusable controller script with `create_mono_script`.
8. Add the generated component with `add_component` after Unity recompiles.
9. Wire serialized arrays and object references with `set_serialized_array` or
   `set_object_reference`.
10. Wire buttons to public methods with `set_button_on_click`.
11. Save or reuse stable UI chunks with `create_ui_prefab` and
   `apply_ui_prefab`.
12. Validate the result with `inspect_ui_hierarchy`, `validate_ui_layout`,
   `validate_ui_theme_bindings`, and `validate_ui_flow`.

## Theme-Driven UI Pattern

When implementing UI that the user or agent should adjust later, bind visual
elements to ScriptableObject data from the start:

1. Create a shared `UiTheme` with `create_ui_theme`.
2. Bind panels, buttons, headings, body text, muted text, icons, and images with
   `bind_ui_theme_element` or pass `themeRole` during `create_ui_element` /
   `create_ui_control`.
3. Use `set_ui_theme_values` for global batch changes such as all primary
   buttons, panel colors, font sizes, and shared sprites in one request.
4. Use `set_ui_theme_value` only when changing one token.
5. Use `UiScreenConfig` for per-screen copy and responsive panel limits, not for
   global visual style.
6. Run `apply_ui_theme`, `apply_ui_screen_config`, and
   `validate_ui_theme_bindings` before saving scenes or prefabs.

Example batch theme update:

```json
{
  "command": "set_ui_theme_values",
  "params": {
    "themePath": "Assets/UI/Themes/DefaultUiTheme.asset",
    "colors": {
      "panel": "0.06,0.10,0.14,0.96",
      "primary": "0.20,0.76,0.58,1",
      "button_text": "0.02,0.04,0.05,1"
    },
    "values": {
      "title_size": 54,
      "body_size": 31
    },
    "references": {
      "panel_sprite": "Assets/UI/Sprites/Panel.png",
      "button_sprite": "Assets/UI/Sprites/Button.png"
    },
    "scenePath": "Assets/Scenes/Title.unity",
    "rootHierarchyPath": "Canvas_Tutorial",
    "applyAfterUpdate": true,
    "dryRun": false
  }
}
```

## Extreme Aspect Ratio Pattern

For UI that must tolerate portrait, landscape, tablet, ultrawide, and tall phone
screens, do not rely only on fixed `RectTransform` sizes. Use this pattern:

1. Configure the root Canvas with `set_canvas_scaler`.
2. Add a full-screen or safe-area container with `set_responsive_rect` mode
   `safe_area` or `stretch_parent`.
3. Add the main panel with `set_responsive_rect` mode `centered_max_size` and
   explicit `margins`, `minSize`, and `maxSize`.
4. Put variable content inside LayoutGroup or ScrollView controls so narrow
   portrait screens can wrap or scroll instead of clipping.
5. Use `create_ui_layout_group` with `aspect_ratio_fitter` only for elements
   that must preserve a visual ratio, such as previews or cards.
6. Run `validate_ui_layout` with `canvasHierarchyPath` to focus validation on
   the edited UI root.

## Built-In Command Pairing

- Use `unity-cli console --type error,warning,log` after changes to inspect
  editor feedback.
- Use `unity-cli test` when the project has Unity Test Framework coverage.
- Use `unity-cli reserialize <path>` after direct text/YAML edits to Unity
  assets.
- Use `unity-cli exec` for one-off inspection or temporary experiments only.
- Use `unity-cli profiler` when investigating runtime/editor performance.

## Parameter Naming Note

Command names are snake_case. This package currently reads direct bridge
`params` keys using the camelCase names passed to `ToolParams`, for example
`scenePath`, `hierarchyPath`, `componentType`, `includeInactive`, and `dryRun`.

If `unity-cli list` displays a different casing, treat that as a documentation
and implementation mismatch to fix before relying on the tool in automation.
