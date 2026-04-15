# Harness Mistakes

이 폴더는 하네스 또는 에이전트 운영 과정에서 드러난 재발 가능한 실수를 기록하는 곳입니다.

목표는 단순 회고가 아니라, 같은 종류의 실수를 다시 저지르지 않도록 운영 규칙과 검증을 강화하는 것입니다.

## Navigation

- Root hub: [[docs/HarnessMistakes/README]]
- Domain index: [[docs/HarnessMistakes/domains/README]]
- Category index: [[docs/HarnessMistakes/categories/README]]

## 사용 규칙

- 작업 시작 시에는 먼저 `domains/README.md`를 읽고 primary domain을 고릅니다.
- 해당 domain 파일이 `Preload extra mistake context: yes`일 때만 관련 category 파일을 추가로 읽습니다.
- 실수가 확인되면 primary category를 1개 고릅니다.
- 필요하면 secondary category를 언급할 수 있지만, 실제 기록은 primary category 파일에 남깁니다.
- 해당 범주 파일의 `Incident Log`에 새 항목을 append합니다.
- 같은 항목이 반복되면 기존 규칙을 더 강하게 바꾸고, AGENTS/CLAUDE/검증/테스트에도 반영합니다.

## Category Index

- [[docs/HarnessMistakes/categories/scope-boundary]]
  - 허용 범위를 벗어난 수정, unrelated change 훼손, guarded asset 오수정
- [[docs/HarnessMistakes/categories/context-drift]]
  - 프로젝트 구조나 현재 상태를 잘못 이해한 경우
- [[docs/HarnessMistakes/categories/validation-gap]]
  - 해야 할 검증을 빼먹거나, 약한 검증으로 완료 처리한 경우
- [[docs/HarnessMistakes/categories/behavioral-regression]]
  - 최소 게이트는 통과했지만 의도된 동작을 놓친 경우
- [[docs/HarnessMistakes/categories/tooling-automation-gap]]
  - 도구/자동화/에디터 상태 이해 부족으로 잘못된 실행 흐름을 안내한 경우
- [[docs/HarnessMistakes/categories/communication-handoff]]
  - 사용자에게 물어봐야 할 지점, 수동 실행 요청, 완료 보고 방식이 부정확했던 경우

## Domain Routing

- [[docs/HarnessMistakes/domains/README]]
  - 작업 도메인 선택 기준
- [[docs/HarnessMistakes/domains/combat-gameplay]]
- [[docs/HarnessMistakes/domains/ui-bridge]]
- [[docs/HarnessMistakes/domains/scene-prefab]]
- [[docs/HarnessMistakes/domains/data-assets]]
- [[docs/HarnessMistakes/domains/vfx-presentation]]
- [[docs/HarnessMistakes/domains/validation-tooling]]
- [[docs/HarnessMistakes/domains/docs-harness]]
  - 해당 도메인의 현재 반복 실수 여부와 추가 preload 필요 여부

## Incident Entry Format

각 category 파일에는 아래 형식으로 기록합니다.

```text
### YYYY-MM-DD - [짧은 제목]
- Trigger: 무엇이 실수였는가
- Impact: 어떤 잘못된 결과가 나왔는가
- Detection: 어떻게 발견되었는가
- Rule Change: 어떤 운영 규칙을 바꿨는가
- Validation Change: 어떤 검증/테스트/체크리스트를 추가했는가
- Follow-up Files: 어떤 파일들을 같이 수정했는가
```

## 분류 우선순위

여러 범주가 걸쳐 보이면 이 순서대로 primary를 고릅니다.

1. scope-boundary
2. validation-gap
3. context-drift
4. behavioral-regression
5. tooling-automation-gap
6. communication-handoff

경계가 애매하면 `Rule Change`가 가장 직접적으로 바뀌는 범주를 primary로 고릅니다.
