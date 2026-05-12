# Architecture

This repository separates executable Unity package code from discoverability assets.

## Layers

1. `Editor/Tools`
   Actual `UnityCliTool` implementations.
2. `Editor/Infrastructure`
   Shared parsing, response, logging, and selection helpers.
3. `docs/tools`
   Human-readable behavior notes, parameter intent, edge cases, and examples.
4. `catalog`
   Machine-readable metadata for indexing, aliases, and search.
5. `prompts`
   Task-oriented guidance for LLM workflows that combine multiple tools.

## Intended Flow

1. Implement a tool class in `Editor/Tools/<Category>`.
2. Register clear name, description, and group metadata.
3. Add the matching doc page in `docs/tools/<category>/`.
4. Update or generate `catalog/tools.json`.
5. Validate discoverability through `unity-cli list`.

