# Automation Tools

This category is intentionally last.

## Goal

Run repetitive mutations only after search, inspection, and validation tools are trustworthy.

## Priority

1. `batch_rename`
2. `batch_set_active`
3. `batch_replace_component`
4. `batch_update_serialized_property`

## Status

Implemented:

- `batch_rename`
- `batch_set_active`
- `batch_replace_component`
- `batch_update_serialized_property`

## Tool Plan

### `batch_rename`

Purpose:

- rename a filtered set of objects or assets using explicit rules

Minimum input:

- target filter
- rename pattern
- optional `dryRun`

Minimum output:

- matched count
- changed count
- preview or applied rename pairs

Implementation notes:

- dry-run should be enabled by default

### `batch_set_active`

Purpose:

- set active state for a filtered object set

Minimum input:

- target filter
- `active`
- optional `dryRun`

Minimum output:

- affected targets
- before and after state summary

### `batch_replace_component`

Purpose:

- replace one component type with another over a filtered set

Why later:

- higher risk and likely project-specific

### `batch_update_serialized_property`

Purpose:

- update one serialized field across many targets

Why later:

- highest risk in the repository and should come last
