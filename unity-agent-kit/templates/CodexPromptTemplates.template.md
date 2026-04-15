# {{PROJECT_NAME}} Codex 프롬프트 템플릿

이 파일은 레거시 호환용 템플릿입니다.

새 프로젝트에는 가능하면 `docs/AgentPromptTemplates.md`를 사용하고,
이미 `docs/CodexPromptTemplates.md`를 쓰는 프로젝트에서만 이 이름을 유지하세요.

운영 중 실수가 확정되면 `docs/HarnessMistakes/README.md`를 기준으로 범주를 고르고 해당 category 파일을 갱신하세요.

## 이 파일의 목적

이 프로젝트에서 프롬프트 단독으로 하네스가 작동하지 않는다.

하네스는 아래 조각들이 합쳐질 때 작동한다:

- 작업 요청
- `AGENTS.md`
- 자동화된 검증
- 작업별 테스트 또는 완료 조건 확인

매번 전체 템플릿을 작성할 필요는 없다.

아래처럼 짧게 요청해도:

```text
이 기능이 가끔 안 돼.
```

Codex는 이를 아래 항목으로 확장해서 처리한다:

- 작업 목표
- 안전한 수정 범위 추론
- 완료 조건
- 일치하는 검증 경로

더 세밀하게 제어하고 싶을 때만 전체 템플릿을 사용한다.

Claude Code 작업 확인을 요청하면 Codex는:

- `docs/AgentHandoffs/pending/claude-to-codex/`만 먼저 확인한다
- 현재 작업과 관련된 unread 노트만 읽는다
- 이해한 노트는 `docs/AgentHandoffs/consumed/claude-to-codex/`로 이동한다
- 사용자가 명시적으로 요청하지 않는 한 `consumed/**`를 다시 읽지 않는다

## 기본 템플릿

```text
작업 목표:
- [원하는 기능/버그 수정 한 줄]

배경:
- [현재 증상 또는 목적]
- [관련 시스템/파일 경로]

수정 허용 범위:
- [예: runtime code folders]
- [필요 시 tests]

수정 금지 범위:
- [예: scenes/prefabs/data assets/project settings]

완료 조건:
- [조건 1]
- [조건 2]
- [조건 3]

순서:
- 관련 파일 전부 읽고 분석한 다음 수정해.

검증:
- AGENTS.md 기준으로 검증해줘.
- Unity 에디터가 닫혀 있으면 batch 검증을 실행해줘.
- Unity 에디터가 열려 있으면 내가 실행할 메뉴와 기대 결과를 같이 알려줘.
- 이번 변경에 맞는 테스트가 필요하면 최소 범위로 추가해줘.

보고 방식:
- 변경 파일
- 검증 결과
- 남은 리스크나 수동 확인 항목
```

## 템플릿 1: 코드 전용 작업

런타임 코드와 테스트 안에서만 작업해야 할 때 사용한다.

## 템플릿 2: 코드 + 데이터 작업

ScriptableObject 또는 config asset 편집이 포함될 때 사용한다.

## 템플릿 3: 씬 또는 프리팹 작업

guarded serialized asset을 의도적으로 허용할 때만 사용한다.

## 템플릿 4: 조사 우선

분석만 원하고 아직 코드 변경은 원하지 않을 때 사용한다.

## 템플릿 5: Claude Code Handoff 확인

```text
Claude Code 작업 내역 확인해줘.

`docs/AgentHandoffs/pending/claude-to-codex/`만 먼저 확인하고,
unread handoff note가 있으면 관련된 것만 읽어줘.
읽은 note는 `docs/AgentHandoffs/consumed/claude-to-codex/`로 이동해줘.
없으면 handoff가 없다고만 알려주고 다른 handoff 파일은 읽지 마.
```

## 한 줄 단축 요청

```text
[기능/버그]를 수정해줘. 관련 파일 전부 읽고 분석한 다음 수정해. AGENTS.md 규칙을 따르고, 수정은 [허용 범위] 안에서만 해줘. 완료 조건은 [조건]이고, 검증은 가능한 자동으로 수행해줘. Unity 에디터가 열려 있으면 내가 실행할 메뉴와 기대 결과를 같이 알려줘.
```
