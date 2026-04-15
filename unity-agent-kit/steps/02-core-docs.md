# Step 02 — 핵심 에이전트 문서

## 이 단계의 목표

`harness-version`, `AGENTS.md`, `CLAUDE.md`, `docs/project-context.md`, `README.md`를 설치하거나 갱신한다.

---

## harness-version

프로젝트 루트에 `harness-version` 파일을 만든다.

```
kit-commit: [git -C [킷경로] rev-parse HEAD]
installed: [오늘 날짜 YYYY-MM-DD]
kit-path: [킷 절대 경로]
agent: codex | claude-code
```

---

## AGENTS.md

기준 템플릿: `unity-agent-kit/templates/AGENTS.template.md`

기본값: **Codex 호환 자기완결형** (dual-agent 모드 기본)
`AGENTS.claude.template.md`는 사용자가 Claude Code 전용 경량형을 명시할 때만 사용.

> **언어**: 템플릿은 영문이다. 프로젝트 기존 AGENTS.md가 있으면 그 언어를 따른다.
> 없으면 템플릿을 기준으로 작성하되, 한국어 프로젝트라면 한국어로 작성해도 된다.

포함 내용:
- 프로젝트 범위/목표
- 주요 폴더 맵
- safe / explicit / read-only write boundary
- guarded assets 목록 (`.unity`, `.prefab`, `ProjectSettings/**`, guarded data path)
- validation workflow
- 실수 학습 루프 포인터 (`docs/HarnessMistakes/domains/README.md`)

---

## CLAUDE.md

기준 템플릿: `unity-agent-kit/templates/CLAUDE.template.md`

**100줄 이하 엄수** — 매 세션 자동 로드. 길수록 토큰 낭비 선형 증가.

CLAUDE.md에 직접 포함:
- 프로젝트 개요 (엔진, 장르, 아키텍처)
- 주요 런타임 루프 (4~6단계)
- 폴더 맵 (최상위만)
- 검증 커맨드 (`tools\compile-unity.cmd` 등)
- Guarded assets 목록
- 참조 포인터: AGENTS.md, project-context.md, ProjectWiki, HarnessMistakes

`docs/project-context.md`로 분리:
- 핵심 시스템/모듈 상세
- Visual Flow, Data Flow
- 시스템 초기화 순서 / 업데이트 단계

---

## docs/project-context.md

기준 템플릿: `unity-agent-kit/templates/docs/project-context.template.md`

템플릿을 복사한 뒤, Step 01 스캔 결과를 바탕으로 아래 항목을 채운다:
- Core Systems 목록 및 역할 설명
- Visual Flow (렌더링/UI 갱신 흐름)
- Data Flow (ScriptableObject, Config, 런타임 데이터 흐름)
- 시스템 초기화 순서 / Update 단계

자동 로드되지 않음. 에이전트가 관련 작업 시에만 읽는다.

---

## README.md 갱신 기준

아래 내용이 최신 상태인지 확인하고 갱신한다:
- 현재 프로젝트 실제 상태
- 주요 전투/성장/표현 계층
- 하네스와 검증 흐름
- 에디터가 열렸을 때와 닫혔을 때의 검증 경로

이미 충분하면 하네스 관련 섹션만 추가해도 된다.

→ 완료되면 `unity-agent-kit/steps/03-harness-docs.md`를 읽는다.
