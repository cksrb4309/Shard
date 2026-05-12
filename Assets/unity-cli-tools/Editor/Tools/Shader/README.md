# Shader Tools

This category starts in Phase 2 because shader tasks depend on stable asset lookup patterns.

## Priority

1. `find_shaders`
2. `find_materials_using_shader`
3. `inspect_shader_properties`
4. `validate_shader_assets`
5. `set_material_shader`
6. `set_material_property`

## Tool Plan

### `find_shaders`

Purpose:

- search shader assets by name and folder scope

Minimum input:

- optional `name`
- optional `folder`
- optional `includeHidden`
- optional `limit`

Minimum output:

- `results[]` with shader name, asset path, guid, hidden flag, support state

### `find_materials_using_shader`

Purpose:

- list materials bound to one shader target

Minimum input:

- `shaderName` or `shaderPath` or `shaderGuid`
- optional `folder`
- optional `limit`

Minimum output:

- resolved shader identity
- `results[]` with material name, path, guid, render queue

### `inspect_shader_properties`

Purpose:

- inspect shader property schema and defaults for automation-safe edits

Minimum input:

- `shaderName` or `shaderPath` or `shaderGuid`

Minimum output:

- shader identity
- property count
- `properties[]` with name, type, default, range/texture metadata

### `validate_shader_assets`

Purpose:

- detect missing shader references and unsupported shader assignments

Why later:

- depends on reliable shader/material discovery responses first

### `set_material_shader`

Purpose:

- replace one material shader safely

Why later:

- mutation should follow inspection and validation paths

### `set_material_property`

Purpose:

- mutate one material property with strict type validation

Why later:

- highest mutation risk in this category

