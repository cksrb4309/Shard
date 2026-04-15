# Shard

<!-- 목표: 100줄 이하 유지. 매 세션 자동 로드. -->

- Engine: Unity `6000.3.11f1`
- Genre/Type: 거점 방어형 액션 게임 (포트폴리오)
- Architecture: MonoBehaviour + Manager 패턴, 오브젝트 풀링, 이벤트 기반 Ability

## 런타임 루프

1. 타이틀 → 캐릭터 선택 → Game 씬 진입
2. 파편 채굴 (블록 파괴) → EnergyCore 수집
3. 적 처치 → SoulShard + XP 수집 → 레벨업 → Ability 선택
4. 블록 파괴 누적 → 보스 등장 (`SpawnManager`)
5. 보스 처치 → 결정체 복귀 → 전장 복원 → 다음 스테이지 (`StageManager`)
6. Stage 3 완료 → 엔딩 페이즈 (60초 생존 → 정화 폭발)

## 폴더 맵

```
Assets/
├── 01_Scripts/          — 런타임 코드 (Gameplay/, Manager/)
├── 02_Scenes/           — Title.unity, Game.unity
├── 06_Prefabs/          — 게임 프리팹
├── 12.ScriptableObject/ — Ability, Spawn, Ship config
├── 99_Tests/            — 테스트
└── Editor/              — 에디터 툴링
```

## 검증

```
tools\compile-unity.cmd
tools\smoke-unity.cmd
tools\test-editmode.cmd
tools\validate-unity.cmd
```

에디터가 열려 있으면: `Tools/Harness Validation/Run Smoke Validation`

## 보호 에셋

- `Assets/02_Scenes/Game.unity`
- `Assets/02_Scenes/Title.unity`
- `Assets/06_Prefabs/**`
- `Assets/12.ScriptableObject/**`
- `ProjectSettings/**`

## 참조

- `AGENTS.md` — 수정 경계, 검증 워크플로, 아키텍처 규칙 (비자명한 작업 전 필독)
- `docs/project-context.md` — 시스템 구조, 데이터 흐름, 초기화 순서 (관련 작업 시에만 로드)
- `docs/ProjectWiki/index.md` — 누적 시스템/에셋/ADR 지식 (시스템·에셋 관련 작업 시에만 로드)
- `docs/HarnessMistakes/domains/README.md` — 실수 컨텍스트 (코드 수정 포함 작업 시에만 로드)
- `docs/AgentHandoffs/README.md` — Codex ↔ Claude Code handoff 규칙 (다른 에이전트 작업 내역 확인 시에만 로드)
- `docs/Obsidian.md` — Obsidian vault 운영 규칙 (문서 작업 시에만 로드)
- `docs/RTK.md` — shell output 압축 규칙 (긴 shell output이 예상될 때만 로드)
- `docs/SubAgents.md` — delegation 허용 세션에서만 읽는 서브 에이전트 운영 규칙
- `docs/Graphify.md` — graphify 설치/갱신 규칙 (구조 질문 전 `graphify-out/GRAPH_REPORT.md` 먼저 확인)
- `graphify-out/GRAPH_REPORT.md` — 코드 접근 범위를 좁힐 때 먼저 참고
- `docs/AgentPromptTemplates.md` — 요청 템플릿
