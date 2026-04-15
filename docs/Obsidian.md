# Obsidian

이 프로젝트는 **Unity 프로젝트 루트 자체를 Obsidian vault로 열 수 있는 구조**를 기본값으로 둔다.

## Placement Rule

- `unity-agent-kit`와 하네스 산출물은 **프로젝트 루트**에 둔다.
- `Assets/` 아래에는 넣지 않는다.
- 이유: Unity가 `Assets/` 아래의 모든 폴더와 파일에 `.meta` 파일을 만들기 때문이다.
- 즉, 문서 vault는 `Assets/` 밖의 루트 문서 계층(`docs/**`, `AGENTS.md`, `CLAUDE.md`)으로 운영한다.

## Minimum Vault Rules

- 루트 폴더를 Obsidian vault로 연다.
- 문서 간 연결이 실제로 유용할 때만 `[[wiki link]]`를 쓴다.
- 새 지식 문서를 만들면 가능한 한 `docs/ProjectWiki/index.md`에서 발견 가능하게 만든다.
- note 이름은 자주 바꾸지 않는다. 링크 안정성이 우선이다.
- 한 note에는 한 시스템, 한 에셋, 한 의사결정처럼 단일 주제를 우선한다.
- handoff note는 짧게 유지하고, 장기 지식은 `docs/ProjectWiki/**`에 남긴다.

## Recommended Linking Pattern

- 시스템 문서: `[[docs/ProjectWiki/systems/SomeSystem]]`
- 에셋 문서: `[[docs/ProjectWiki/assets/SomeAsset]]`
- ADR 문서: `[[docs/ProjectWiki/adr/ADR-001-some-decision]]`

## Local Graph Guidance

- `docs/ProjectWiki/index.md`를 허브 note로 삼는다.
- `docs/HarnessMistakes/**`는 제외하지 않고 클러스터로 유지한다.

## Recommended Excluded Files

```text
Library/**
Temp/**
Logs/**
Obj/**
UserSettings/**
.git/**
```

핵심은 `Library/**` 전체를 제외하는 것이다.

## What This Does Not Require

- 모든 문서에 frontmatter를 강제하지 않는다.
- `.obsidian/` 설정 폴더를 킷이 소유하지 않는다.
