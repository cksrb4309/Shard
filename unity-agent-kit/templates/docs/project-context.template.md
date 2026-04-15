# {{PROJECT_NAME}} — Project Context

<!-- 이 파일은 자동 로드되지 않는다.
     시스템 구조, 비주얼 흐름, 데이터 흐름, 초기화 순서 관련 작업 시에만 로드한다.
     CLAUDE.md에서 포인터로 참조한다. -->

<!-- Obsidian을 쓴다면 관련 장기 문서는 docs/ProjectWiki/** note로 분리하고,
     이 문서에서는 필요한 위치에만 [[wiki link]]로 연결한다. -->

## Core Systems

<!-- 주요 시스템/모듈 목록. 각 항목은 2~3줄 이내. -->

### {{SystemA}}

역할:
주요 연결:

### {{SystemB}}

역할:
주요 연결:

<!-- 시스템 상세 내용은 작업하면서 docs/ProjectWiki/systems/ 에 누적한다. -->

## Visual Flow

<!-- 게임플레이 상태가 화면에 표시되기까지의 흐름. -->

```
{{GAMEPLAY_LAYER}} (ECS / core)
    → 이벤트 또는 상태 변경 발생
{{BRIDGE_LAYER}} (bridge / presenter)
    → 상태를 구독하고 View에 전달
{{VIEW_LAYER}} (pooled views / VFX)
    → 실제 렌더링 처리
```

## Data Flow

<!-- 튜닝/설정값이 어디서 정의되고, 런타임 어느 경로로 소비되는지. -->

- 설정 위치: `Assets/{{CONFIG_PATH}}`
- 로드 시점: {{WHEN_LOADED}}
- 소비 시스템: {{CONSUMER_SYSTEMS}}

## System Ordering / Lifecycle

<!-- 명시적인 시스템 그룹, 업데이트 단계, 부트스트랩 순서, 초기화 의존성. -->

초기화 순서:

1. {{BOOTSTRAP_STEP_1}}
2. {{BOOTSTRAP_STEP_2}}
3. {{BOOTSTRAP_STEP_3}}

Update 단계:

- {{UPDATE_PHASE_EARLY}}: ...
- {{UPDATE_PHASE_DEFAULT}}: ...
- {{UPDATE_PHASE_LATE}}: ...

## Architecture Rules

<!-- 코드 구조, 네이밍, 성능 민감 규칙. AGENTS.md와 중복 최소화. -->

- {{RULE_1}}
- {{RULE_2}}
