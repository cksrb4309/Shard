# Diagnostics Tools

This category is the first implementation target.

## Goal

Answer "what is the editor doing right now?" before any search or mutation tool runs.

## Priority

1. `editor_state`
2. `selection_state`
3. `console_snapshot`

## Tool Plan

### `editor_state`

Purpose:

- return the current editor context used to branch later tool calls

Minimum input:

- no required parameters

Minimum output:

- `isPlaying`
- `isPaused`
- `isCompiling`
- `isUpdating`
- `projectPath`
- `activeScenePath`
- `openSceneCount`
- `selectionCount`

Implementation notes:

- this should be lightweight and safe to call often
- avoid dumping large scene data here

### `selection_state`

Purpose:

- return the current selected objects and assets in a machine-friendly form

Minimum input:

- optional `includeComponents`
- optional `includeAssetInfo`

Minimum output:

- `objects[]` with name, path, instance id, type
- `assets[]` with path, guid, type

Implementation notes:

- identifiers returned here should be reusable in later tools
- keep per-item payload concise by default

### `console_snapshot`

Purpose:

- return a compact summary of recent warnings and errors

Why later:

- built-in `unity-cli console` already covers the basic need
- custom filtering can wait until core read tools exist

