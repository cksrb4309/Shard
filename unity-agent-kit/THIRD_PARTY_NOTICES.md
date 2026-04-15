# Third-Party Notices

`unity-agent-kit` is a project template and operations kit.  
It does **not** bundle third-party binaries or source code for the tools below by default.  
Instead, it provides documentation, setup templates, and workflow guidance for tools the user installs separately.

## Included by reference, not bundled

### graphify

- Purpose: explicit knowledge-graph workflow for repository structure understanding
- Upstream: [safishamsi/graphify](https://github.com/safishamsi/graphify)
- License: MIT
- Notes:
  - This kit documents `graphify` usage and provides wrapper scripts and workflow docs.
  - The actual `graphify` package is installed separately by the user.

### RTK

- Purpose: shell output compression for token savings
- Upstream: [rtk-ai/rtk](https://github.com/rtk-ai/rtk)
- License: Apache-2.0
- Notes:
  - This kit documents RTK usage and safe operating rules.
  - The actual RTK binary and global hook installation are managed outside this repository.

### unity-cli

- Purpose: Unity Editor control for play state, console, screenshots, and test execution
- Upstream: [RageAgainstThePixel/unity-cli](https://github.com/RageAgainstThePixel/unity-cli)
- License: MIT
- Notes:
  - This kit standardizes on `unity-cli` for Unity Editor control.
  - Unity Editor control implementations based on `Unity-MCP` are not part of this kit's supported toolchain.

### unity-prefab-parser-mcp

- Purpose: token-efficient reading of `.unity` and `.prefab` structures
- Upstream package name: `unity-prefab-parser`
- Local integration name in this kit: `unity-prefab-parser-mcp`
- License: MIT
- Notes:
  - This tool is treated as a read-optimization companion, not as the primary Unity Editor control layer.
  - This kit may reference it in `.mcp.json`, `.codex/config.template.toml`, and companion setup docs.

## Trademarks and product names

This repository may refer to product or project names such as Codex, Claude Code, Claude Desktop, Unity, Obsidian, graphify, RTK, and unity-cli for compatibility and integration guidance.  
Those names remain the property of their respective owners.

## What this means for redistribution

- If you publish this repository as-is, this notice is intended to clarify which external tools are referenced.
- If you later copy third-party source code, scripts, or substantial documentation excerpts into this repository, you may need additional attribution, license files, or notice handling beyond this document.
