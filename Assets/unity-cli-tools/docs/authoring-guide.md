# Authoring Guide

## Tool File Convention

- One tool per file.
- Use a stable command name.
- Keep parameter schema explicit.
- Return structured data that an agent can chain into later commands.

## Recommended Implementation Pattern

1. Parse incoming values into a typed parameter object.
2. Resolve Unity objects through stable paths, GUIDs, or names.
3. Validate assumptions early and return clear failures.
4. Keep Unity mutations narrow and auditable.
5. Return concise summaries plus machine-friendly fields.

## Connector Tool Rules

- Tool classes must be `static`.
- Tool classes must have `[UnityCliTool]` with a stable command name,
  description, and group.
- Tool handlers must expose `public static object HandleCommand(JObject parameters)`
  or the supported async variant.
- Return `SuccessResponse(message, data)` for successful results.
- Return `ErrorResponse(message)` for validation or execution failures.
- Add a nested `Parameters` class with `[ToolParameter]` attributes so
  `unity-cli list` can expose parameter schemas to humans and agents.
- Keep the parameter names documented by `Parameters` aligned with the keys read
  through `ToolParams`.
- Avoid duplicate tool names. The connector logs duplicates and only the first
  discovered handler is used.

## Unity Version Compatibility

Custom tools are copied between Unity projects, so warning-free compilation is
part of the contract. Treat compiler warnings in this package as issues to fix.

- Prefer current Unity Editor APIs for the destination Unity version.
- For Unity 6 `PlayerSettings`, prefer `UnityEditor.Build.NamedBuildTarget`
  overloads instead of deprecated `BuildTargetGroup` overloads.
- Use explicit type aliases when Unity exposes similarly named types from
  multiple namespaces.
- Keep source syntax C# 9 compatible unless the package intentionally raises its
  minimum Unity/C# version.
- Do not solve compatibility warnings by adding broad warning suppressions or a
  project-wide `csc.rsp`.

## `exec` vs Custom Tool

Use `unity-cli exec` for one-off inspection, quick experiments, and temporary
queries. Do not make agents depend on long `exec` snippets for repeated
workflows.

Create or extend a custom tool when the operation is repeated, needs clear
parameters, should be discoverable through `unity-cli list`, or mutates Unity
state.

## Serializer Safety

Agents may sometimes edit Unity YAML files directly. After direct text edits to
`.unity`, `.prefab`, `.asset`, or `.mat` files, run `unity-cli reserialize` on
the edited paths so Unity loads and writes them through its own serializer.

## Documentation Convention

For every tool, keep a matching doc page with:

- purpose
- inputs
- output shape
- safety notes
- example prompts
- example `unity-cli` invocations

The `unity-cli list` output is the source of truth for command names,
descriptions, groups, and parameter schemas. Whenever a tool's attribute,
`Parameters` class, or `ToolParams` keys change, update the matching tool doc in
the same change.

## Testing Convention

- shared helpers go in `Tests/Editor/Core`
- tool-focused tests go in `Tests/Editor/Tools`
- prioritize deterministic tests over snapshot-heavy tests
