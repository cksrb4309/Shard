# Component Tools

This category starts in Phase 2 because it depends on stable object targeting.

## Priority

1. `inspect_components`
2. `find_missing_scripts`
3. `find_objects_with_component`
4. `add_component`
5. `remove_component`
6. `set_serialized_property`

## Status

Implemented:

- `inspect_components`
- `find_missing_scripts`
- `find_objects_with_component`
- `add_component`
- `remove_component`

Covered by `batch_update_serialized_property`:

- `set_serialized_property`

## Tool Plan

### `inspect_components`

Purpose:

- inspect components attached to a target object

Minimum input:

- `hierarchyPath` or `instanceId`
- optional `includeSerializedFields`

Minimum output:

- target identity
- component type list
- selected serialized field summary

Implementation notes:

- start with a summary view, not a full property dump
- later tools can build on the returned component names

### `find_missing_scripts`

Purpose:

- find GameObjects with missing MonoBehaviour scripts across scenes or prefabs

Minimum input:

- optional `scenePath`
- optional `folder`
- optional `includePrefabs`

Minimum output:

- affected object path
- container scene or prefab path
- missing count

### `find_objects_with_component`

Purpose:

- search loaded scenes for objects with a named component type

Why later:

- overlaps somewhat with hierarchy search and can follow it

### `add_component`

Purpose:

- add one component to one identified object

Why later:

- mutation should wait until inspection and targeting are reliable

### `remove_component`

Purpose:

- remove one component from one identified object

Why later:

- same reason as `add_component`, with more risk

### `set_serialized_property`

Purpose:

- change a serialized field in a controlled way

Why later:

- highest complexity in this category
