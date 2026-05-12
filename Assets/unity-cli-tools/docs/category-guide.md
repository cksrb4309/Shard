# Category Guide

Use categories that reflect how an agent thinks about Editor work, not how Unity APIs are organized.

## Categories

- `scene`: scene lifecycle and active-scene operations
- `hierarchy`: object discovery and transform tree queries
- `prefab`: prefab lookup, validation, and override checks
- `asset`: asset database search and serializer-safe follow-up actions
- `shader`: shader and material discovery, inspection, and validation
- `component`: component inventory and missing-script analysis
- `validation`: checks that answer "is the project in a safe state?"
- `diagnostics`: current editor state for tool selection and branching
- `automation`: repetitive, bulk, or batch operations
- `build`: build-target and build-readiness checks
- `project`: project settings, package, tag, layer, and setup inventory
- `runtime`: PlayMode state and runtime input probes
- `visual`: camera, Canvas, UI, and visual inspection

## Rule of Thumb

If a tool mainly answers a question, prefer `diagnostics`, `hierarchy`, `asset`,
`shader`, `validation`, `project`, `runtime`, or `visual`.
If it mainly changes project state, prefer `scene`, `prefab`, or `automation`.
If it checks build readiness without building, prefer `build`.
