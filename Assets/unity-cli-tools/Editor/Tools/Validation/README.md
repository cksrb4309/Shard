# Validation Tools

This category is Phase 1 because it is the safety layer around scene and prefab work.

## Priority

1. `validate_scene_references`
2. `find_missing_references`
3. `validate_build_settings`
4. `validate_selection`

## Status

Implemented:

- `validate_scene_references`
- `find_missing_references`
- `validate_build_settings`
- `validate_selection`

## Tool Plan

### `validate_scene_references`

Purpose:

- detect missing object references and missing scripts in loaded or target scenes

Minimum input:

- optional `scenePath`
- optional `includeInactive`

Minimum output:

- `issues[]` with severity, scene path, object path, component, message
- issue totals by type

Implementation notes:

- keep issue records compact and sortable
- this is a strong candidate for the first validation tool

### `find_missing_references`

Purpose:

- find missing serialized object references and return precise repair inputs

Minimum input:

- optional `scenePath`
- optional `includeInactive`
- optional `componentType`
- optional `propertyPath`

Minimum output:

- `results[]` with scene path, hierarchy path, component type/index, property path
- `setReferenceInput` object ready for `set_object_reference`

Implementation notes:

- inspect loaded scenes through Unity serialization APIs instead of parsing scene YAML
- return compact records so LLM workflows do not need to read full `.unity` files

### `validate_build_settings`

Purpose:

- inspect build settings scene list for obvious problems

Minimum input:

- no required parameters

Minimum output:

- scenes in build order
- duplicates
- missing paths
- disabled scenes

### `validate_selection`

Purpose:

- run lightweight validation against the current selection

Why later:

- depends on consistent selection and component inspection output
