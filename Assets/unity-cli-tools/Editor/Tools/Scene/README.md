# Scene Tools

This category mixes read and mutation tools, so it starts with narrow, safe operations.

## Priority

1. `list_open_scenes`
2. `open_scene`
3. `save_scene`
4. `create_game_object`
5. `set_transform`
6. `set_active`

## Status

Implemented:

- `list_open_scenes`
- `open_scene`
- `save_scene`
- `create_game_object`
- `set_transform`
- `set_active`

## Tool Plan

### `list_open_scenes`

Purpose:

- show which scenes are currently loaded and which one is active

Minimum input:

- no required parameters

Minimum output:

- `activeScenePath`
- `scenes[]` with path, name, isLoaded, isDirty, isActive

### `open_scene`

Purpose:

- open a scene by asset path with explicit mode control

Minimum input:

- `path`
- optional `mode` with `single` or `additive`

Minimum output:

- opened scene path
- mode used
- active scene path after open

Implementation notes:

- validate path before opening
- default to `single` unless additive is explicitly requested

### `save_scene`

Purpose:

- save active scene or a specific loaded scene

Minimum input:

- optional `path`

Minimum output:

- saved scene path
- dirty state before and after save

Implementation notes:

- if no path is provided, save the active scene
- avoid side effects on unrelated loaded scenes

### `create_game_object`

Purpose:

- create a single object in a known parent context

Why later:

- safe enough, but state inspection and lookup tools should exist first

### `set_transform`

Purpose:

- adjust position, rotation, or scale for a specific object

Why later:

- requires a reliable target identity strategy first

### `set_active`

Purpose:

- toggle object active state

Why later:

- should come after object lookup and validation patterns are established
