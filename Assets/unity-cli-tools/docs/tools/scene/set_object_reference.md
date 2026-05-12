# set_object_reference

Assign one serialized object reference on one component in a loaded scene.

## Inputs

- `scenePath`: optional loaded scene path containing the target object.
- `hierarchyPath`: target GameObject hierarchy path. Required.
- `componentType`: target component type name or full name. Required.
- `componentIndex`: zero-based index among components with the same type. Default: `0`.
- `propertyPath`: serialized object reference property path. Required.
- `referenceHierarchyPath`: replacement scene GameObject hierarchy path.
- `referenceComponentType`: optional component type to assign from the replacement GameObject.
- `referenceComponentIndex`: zero-based replacement component index. Default: `0`.
- `referenceAssetPath`: replacement asset path.
- `referenceAssetGuid`: replacement asset GUID.
- `allowOverwrite`: allow replacing a non-null reference. Default: `false`.
- `dryRun`: preview without mutation. Default: `true`.

Exactly one replacement source should normally be provided: `referenceHierarchyPath`, `referenceAssetPath`, or `referenceAssetGuid`.

## Output Shape

- `target`: scene path, hierarchy path, component type/index, property path.
- `reference`: assigned object name, type, source, and asset path when applicable.
- `previousReference`: previous value if one existed.
- `sceneMarkedDirty`: true when the operation mutated the scene.

## Safety Notes

The command defaults to `dryRun=true`. Set `dryRun=false` only after checking the preview. Existing references are protected unless `allowOverwrite=true`.

## Example

```bash
unity-cli execute set_object_reference --params "{\"scenePath\":\"Assets/Scenes/Main.unity\",\"hierarchyPath\":\"World/Spawner\",\"componentType\":\"EnemySpawner\",\"propertyPath\":\"enemyPrefab\",\"referenceAssetPath\":\"Assets/Prefabs/Enemy.prefab\",\"dryRun\":false}"
```
