# Prefab Tools

This category is Phase 2 because prefab work is common, but basic state and search tools should come first.

## Priority

1. `find_prefab`
2. `inspect_prefab`
3. `validate_prefab`
4. `open_prefab_stage`
5. `save_prefab`

## Tool Plan

### `find_prefab`

Purpose:

- search prefab assets by name, folder, label, or partial path

Minimum input:

- optional `name`
- optional `folder`
- optional `label`

Minimum output:

- `results[]` with name, path, guid, isVariant

### `inspect_prefab`

Purpose:

- inspect a prefab without opening it for editing

Minimum input:

- `path`
- optional `includeComponents`
- optional `maxDepth`

Minimum output:

- root object name
- child count
- component summary
- `isVariant`
- nested prefab usage summary

Implementation notes:

- keep this read-only
- return enough structure for later targeted edits

### `validate_prefab`

Purpose:

- detect missing scripts, broken object references, and obvious structural issues

Why later:

- inspection needs to land first so validation output format can align with it

### `open_prefab_stage`

Purpose:

- open a prefab in prefab stage for later editing tools

Why later:

- avoid stage-edit workflows until read tooling is solid

### `save_prefab`

Purpose:

- persist prefab-stage changes

Why later:

- mutation comes after inspection and validation

