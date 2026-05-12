# Tool Roadmap

This directory contains the actual `UnityCliTool` implementations.

## Implementation Order

Priority is based on safe LLM usage:

1. state inspection
2. target discovery
3. narrow single-object mutations
4. batch mutations

## Phase Plan

### Phase 1: minimum usable read stack

Implemented:

- `Diagnostics/editor_state`
- `Diagnostics/selection_state`
- `Asset/find_asset`
- `Hierarchy/find_game_objects`
- `Hierarchy/dump_hierarchy`
- `Scene/list_open_scenes`
- `Scene/open_scene`
- `Validation/find_missing_references`
- `Validation/validate_scene_references`

### Phase 2: deeper inspection

Implemented:

- `Asset/get_asset_info`
- `Shader/find_shaders`
- `Shader/find_materials_using_shader`
- `Shader/inspect_shader_properties`
- `Prefab/find_prefab`
- `Prefab/inspect_prefab`
- `Component/inspect_components`
- `Component/find_missing_scripts`
- `Validation/validate_build_settings`

### Phase 3: safe single mutations

Implemented:

- `Scene/save_scene`
- `Scene/set_object_reference`
- `Scene/create_game_object`
- `Scene/set_transform`
- `Scene/set_active`
- `Component/add_component`
- `Component/remove_component`

### Phase 4: controlled batch mutations

Implemented:

- `Automation/batch_rename`
- `Automation/batch_set_active`
- `Automation/batch_replace_component`
- `Automation/batch_update_serialized_property`

## Shared Rules

- Every tool should return stable JSON-friendly data.
- Read tools should expose enough identifiers for the next tool call.
- Mutating tools should support dry-run when possible.
- Bulk tools should require explicit filters and return affected targets.
- Folder names, command names, and `Group` values should stay aligned.
