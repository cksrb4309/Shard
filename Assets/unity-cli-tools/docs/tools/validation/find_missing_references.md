# find_missing_references

Find missing serialized object references in loaded scenes without reading `.unity` YAML.

## Inputs

- `scenePath`: optional loaded scene path filter.
- `includeInactive`: include inactive objects during traversal. Default: `true`.
- `componentType`: optional component type name or full name filter.
- `propertyPath`: optional serialized property path filter.
- `limit`: max returned rows. Default: `500`.

## Output Shape

- `inspectedObjectCount`: number of GameObjects inspected.
- `inspectedComponentCount`: number of components inspected after filters.
- `results[]`: missing reference records.
- `results[].setReferenceInput`: target fields ready for `set_object_reference`.

Each result includes `scenePath`, `hierarchyPath`, `componentType`, `componentFullType`, `componentIndex`, `propertyPath`, and `missingReferenceInstanceId`.

## Safety Notes

This tool is read-only. It uses `SerializedObject` and only reports object-reference properties where Unity still has a missing reference instance id.

## Example

```bash
unity-cli execute find_missing_references --params "{\"scenePath\":\"Assets/Scenes/Main.unity\",\"componentType\":\"EnemySpawner\"}"
```
