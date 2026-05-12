# Bridge Protocol Guide

Use the direct bridge protocol for agent automation. It avoids shell quoting
problems, removes per-call CLI process overhead, and talks to the already-open
Unity Editor.

## When To Use It

- Use direct bridge calls for repeated tool execution.
- Use `unity-cli --project "<project path>" status` to discover the correct
  project and port.
- Use `unity-cli --project "<project path>" list` to inspect available command
  names and descriptions.
- Keep `unity-cli --params` for occasional manual checks only.
- Use `unity-cli --port <port>` only after the target bridge port is known.

## Request Shape

Send an HTTP POST request to the connector:

```text
POST http://127.0.0.1:<port>/command
Content-Type: application/json
```

Body:

```json
{
  "command": "find_game_objects",
  "params": {
    "name": "Player",
    "includeInactive": true,
    "limit": 20
  }
}
```

Command names are snake_case. Direct bridge `params` keys for this package should
use the camelCase names read by the tool handlers, such as `scenePath`,
`hierarchyPath`, `componentType`, `includeInactive`, `includeRootCount`, and
`dryRun`.

The connector discovery output may display parameter names in snake_case. Until
connector-level parameter normalization exists, follow the examples in this
package or inspect the matching tool code when writing raw bridge requests.

## PowerShell Example

```powershell
$project = "C:\Path\To\UnityProject"
unity-cli --project $project status

$port = 8094
$body = @{
  command = "list_open_scenes"
  params = @{
    includeRootCount = $true
  }
} | ConvertTo-Json -Depth 10

Invoke-RestMethod `
  -Method Post `
  -Uri "http://127.0.0.1:$port/command" `
  -ContentType "application/json" `
  -Body $body
```

## Minimal Agent Sequence

1. Call `unity-cli --project "<project path>" status`.
2. Use the reported port for bridge calls.
3. Call `editor_state`.
4. Call `list_open_scenes`.
5. Use read-only discovery tools to identify exact targets.
6. For mutation tools, call with `dryRun: true` first.
7. Apply the mutation only after the target set is verified.
8. Validate and save only when the task requires it.
9. If assets were edited as text/YAML, run `unity-cli reserialize` on those
   assets.

## Safety Defaults

- Prefer stable identifiers: asset path, GUID, scene path, hierarchy path.
- Set `limit` on search and batch tools.
- Set `dryRun: true` before mutating when available.
- Avoid broad filters like empty name/tag/layer on batch tools.
- Re-check editor state after compile, domain reload, or scene changes.
- Use `unity-cli exec` for one-off inspection or temporary experiments, not as a
  replacement for repeated custom workflows.
