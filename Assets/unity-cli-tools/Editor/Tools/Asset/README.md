# Asset Tools

This category is Phase 1 because asset lookup is a dependency for many other tasks.

## Priority

1. `find_asset`
2. `get_asset_info`
3. `find_assets_by_dependency`
4. `reserialize_targets`

## Status

Implemented:

- `find_asset`
- `get_asset_info`
- `find_assets_by_dependency`
- `reserialize_targets`

## Tool Plan

### `find_asset`

Purpose:

- search the AssetDatabase by name, type, folder, or partial path

Minimum input:

- optional `name`
- optional `type`
- optional `folder`
- optional `limit`

Minimum output:

- `results[]` with name, path, guid, type

Implementation notes:

- this should be one of the first tools implemented
- keep results stable and sorted

### `get_asset_info`

Purpose:

- return detailed metadata for one asset

Minimum input:

- `path` or `guid`
- optional `includeDependencies`

Minimum output:

- path
- guid
- main asset type
- importer type
- dependency paths

Implementation notes:

- require exactly one target identity source
- dependency output should be compact by default

### `find_assets_by_dependency`

Purpose:

- find assets that depend on a target asset

Why later:

- useful for impact analysis, but not required for the first usable workflow

### `reserialize_targets`

Purpose:

- reserialize specific assets under explicit control

Why later:

- mutation with project-wide impact should wait until read tools are proven
