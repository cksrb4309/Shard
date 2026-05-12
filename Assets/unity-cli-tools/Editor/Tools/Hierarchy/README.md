# Hierarchy Tools

This category is a Phase 1 implementation target.

## Goal

Let an agent find scene objects and inspect structure without mutating anything.

## Priority

1. `find_game_objects`
2. `dump_hierarchy`
3. `find_objects_with_component`

## Tool Plan

### `find_game_objects`

Purpose:

- search loaded scenes for objects matching name, tag, layer, active state, or partial path

Minimum input:

- optional `name`
- optional `tag`
- optional `layer`
- optional `activeOnly`
- optional `scenePath`
- optional `includeInactive`

Minimum output:

- `results[]` with name, scene path, hierarchy path, active state, component names

Implementation notes:

- support partial and case-insensitive search first
- return hierarchy path because names alone are ambiguous

### `dump_hierarchy`

Purpose:

- return a compact transform tree for a target root or active scene

Minimum input:

- optional `rootPath`
- optional `scenePath`
- optional `maxDepth`
- optional `includeComponents`

Minimum output:

- `root`
- `children[]`
- node name, hierarchy path, active state, child count

Implementation notes:

- cap default depth to avoid overly large responses
- support dumping from current selection later if needed

### `find_objects_with_component`

Purpose:

- find all objects using a given component type

Why later:

- useful, but `find_game_objects` and `dump_hierarchy` provide broader early value

Implemented:

- `Component/find_objects_with_component`
