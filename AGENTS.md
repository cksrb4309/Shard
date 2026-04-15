# Shard 에이전트 가이드

<!-- dual-agent 기본값: Codex와 Claude Code 공존 구조 -->

## 범위

이 저장소는 Unity `6000.3.11f1` 프로젝트다.

**Shard**는 중앙 결정체를 거점으로 삼아 파편 채굴, 적 방어, 성장 선택을 반복하며 보스 사이클을 돌파하는 거점 방어형 액션 게임이다.

현재 엔지니어링 포커스: 전투 시스템, Ability 확장, 스테이지 전환 안정화.

안정적으로 유지해야 할 게임플레이 루프:
- 이중 재화(EnergyCore / SoulShard) 수집 및 소비 흐름
- 이벤트 기반 Ability 등록 / 발동 구조
- 블록 파괴 → 보스 전환 → 스테이지 복원 흐름

비자명한 수정 전에 아래 파일을 읽는다:

- `README.md`
- `CLAUDE.md`
- `docs/AgentPromptTemplates.md`

`docs/Obsidian.md`가 있으면 저장소 루트를 문서 vault로 취급하고 하네스 문서는 `Assets/` 밖에 유지한다.
`graphify-out/GRAPH_REPORT.md`가 있으면 넓은 아키텍처 질문이나 저장소 전체 탐색 전에 읽는다.
`docs/Graphify.md`를 참고해서 그래프를 먼저 갱신해야 하는지 판단한다.
여러 폴더에 걸친 코드 접근 시, raw source를 읽기 전에 먼저 그래프로 가장 작은 관련 파일 집합을 파악한다.
긴 shell 출력이 예상되면 넓은 Bash 탐색이나 로그 중심 검증 전에 `docs/RTK.md`를 읽는다.
세션이 delegation을 지원하고 사용자가 허용했으면, bounded side task를 분리하기 전에 `docs/SubAgents.md`를 읽는다.

## 이미지 분석 파이프라인

이미지 분석 작업에는 로컬 MCP 파이프라인을 사용한다.

- `ollama-vision` → 장면 설명, UI 구조, 텍스트 추출 (먼저 사용)
- `image-tools` → 픽셀 색상, 지배색, 거리 측정 (정밀 수치 필요 시만)

## 폴더 맵

```
Assets/
├── 01_Scripts/
│   ├── Gameplay/       — 전투/블록/코어/몬스터/플레이어/능력/오브젝트풀
│   │   ├── Ability/    — AbilityManager, Ability base, CustomAbility
│   │   ├── Block/      — BaseBlock, BlockGroup
│   │   ├── Core/       — CoreHealth, CoreInteract, CoreUpgrade
│   │   ├── Monster/    — Monster, CustomMonster, MonsterAttack
│   │   └── Player/     — PlayerHealth, Inventory, PlayerAttributes, Skill
│   └── Manager/        — GameManager, SpawnManager, StageManager, PoolingManager,
│                          SoundManager, PlayerLevelManager, DifficultyManager
├── 02_Scenes/          — Title.unity, Game.unity
├── 06_Prefabs/         — 게임 프리팹
├── 12.ScriptableObject/— Ability LevelUp/CoreUpgrade, Spawn/MonsterSet, Ship
├── 99_Tests/           — EditMode 스모크 테스트
└── Editor/             — 에디터 커스텀 인스펙터, HarnessValidation
```

## 도구 사용 패턴

- **읽기 턴**: Read, Grep, Glob을 한 번에 병렬 실행
- **분석**: 읽은 내용을 바탕으로 수정 범위와 계획 확정
- **쓰기 턴**: Edit, Write, Bash를 순차 실행

읽기와 쓰기를 여러 턴에 걸쳐 섞지 않는다.

## 그래프 파악

- `graphify-out/GRAPH_REPORT.md`를 고수준 파악에 사용한다
- 구현 결정에는 raw source file을 사용한다
- `graphify codex install` / `graphify claude install` 사용 금지 (하네스 문서 덮어씀)

## Shell Output 압축

`docs/RTK.md`가 있는 경우:

- `git status`, `git diff`, `git log`, `rg/grep`, 테스트 러너 출력, batch 검증 로그에 RTK 경로 우선
- raw source reading, Read/Grep/Glob, unity-cli, unity-prefab-parser-mcp의 대체재로 쓰지 않음

## Shell/Bash 정책

- 위험한 shell 명령은 훅에서 차단됨: `rm`, `Remove-Item -Recurse`, `git reset --hard`, `git clean -fdx`
- 길고 지저분한 출력은 RTK로 압축
- 안전하고 명시적인 검증 명령은 허용

## 기본 수정 경계

**기본값으로 안전:**
- `Assets/01_Scripts/**`
- `Assets/99_Tests/**`
- `Assets/Editor/**`
- `docs/**`
- `tools/**`

**명시적 요청이 필요:**
- `Assets/02_Scenes/**`
- `Assets/06_Prefabs/**`
- `Assets/12.ScriptableObject/**`
- `ProjectSettings/**`

**읽기 전용 (직접 관련 작업 외):**
- `Assets/08.OtherAsset/**` (서드파티)
- `Library/**`, `Temp/**`

`.meta` 파일은 에셋 복구 작업이 아니면 직접 편집하지 않는다.

## 기존 워크트리 규칙

- 관련 없는 사용자 변경을 되돌리지 않는다
- dirty serialized asset이 현재 작업에 필요하지 않으면 그대로 둔다

## 기본 요청 해석

짧은 요청에 대해:
- 작업 목표를 추론한다
- 수정 전에 이 파일과 관련 코드를 읽는다
- code-only scope를 우선 가정한다
- 가장 작은 일치하는 검증 게이트를 선택한다

아래 경우에만 확인을 요청한다:
- 작업이 실질적으로 모호한 경우
- guarded serialized asset 편집이 필요할 가능성이 높은 경우
- 여러 게임플레이 해석이 서로 다른 구현을 의미하는 경우

비자명한 작업 전에:
- `docs/HarnessMistakes/domains/README.md`를 읽어 primary domain 분류
- 해당 domain 파일을 먼저 읽는다
- `Preload extra mistake context: yes`일 때만 추가 category 파일을 읽는다

## Cross-Agent Handoff

`docs/AgentHandoffs/**`를 Codex와 Claude Code 사이의 연결 통로로 사용한다.

Codex 작업 확인 요청 시:
- `docs/AgentHandoffs/pending/codex-to-claude/`를 먼저 확인
- `.gitkeep`만 있으면 handoff 컨텍스트를 더 읽지 않음
- 관련 pending 노트만 최신순으로 읽음
- 이해한 노트는 `docs/AgentHandoffs/consumed/codex-to-claude/`로 이동

Claude Code에 작업을 넘길 때:
- `docs/AgentHandoffs/pending/codex-to-claude/`에 간단한 노트 작성
- 짧게 유지: 작업, 상태, 변경 파일, 검증, 다음 액션

## 서브 에이전트 위임

`docs/SubAgents.md`를 참조. 메인 에이전트가 blocking 결정, guarded asset, 최종 검증을 담당.

## 아키텍처 규칙

- **이중 재화**: EnergyCore(파편, 블록 파괴), SoulShard(영혼, 적 처치). 두 경제를 섞지 않는다
- **Ability 시스템**: `AbilityEventType`(Attack/Critical/Kill/Passive) 이벤트 맵 기반. 능력 추가 시 기존 전투 흐름을 수정하지 않는다
- **풀링**: 몬스터, 투사체, 이펙트는 `PoolingManager`를 통해서만 생성/반환한다. 직접 `Instantiate`/`Destroy` 금지
- **Manager 패턴**: `GameManager`, `SpawnManager`, `StageManager`, `PoolingManager`, `SoundManager`가 각 도메인을 소유한다. 경계를 넘어서 직접 접근하지 않는다
- **Coroutine 기반 흐름**: 스테이지 전환, 보스 등장, 엔딩 시퀀스는 Coroutine으로 처리. 임의 중단 시 `StopAllCoroutines()`를 먼저 확인한다

## 고위험 영역

- `Assets/02_Scenes/Game.unity` — 게임 씬 (모든 Manager 및 오브젝트 연결)
- `Assets/02_Scenes/Title.unity` — 타이틀 씬
- `Assets/06_Prefabs/**` — 게임 프리팹
- `Assets/12.ScriptableObject/**` — Ability, SpawnSet, Ship 설정 데이터
- `ProjectSettings/EditorBuildSettings.asset`

## 검증 워크플로

에디터 **닫힌 상태** (우선순위순):

1. `tools\compile-unity.cmd` — 컴파일만
2. `tools\smoke-unity.cmd` — 씬 무결성, 필수 에셋, missing script
3. `tools\test-editmode.cmd` — EditMode 스모크 테스트
4. `tools\validate-unity.cmd` — 위 전체 순서대로 실행

에디터 **열린 상태**: batchmode 실행 불가. 대신:
- `Tools/Harness Validation/Run Smoke Validation`
- `Tools/Harness Validation/Run EditMode Smoke Tests`
- `Tools/Harness Validation/Run Full Validation`

## 실수 학습 루프

실수 확인 시:
- `docs/HarnessMistakes/README.md`로 primary category 분류
- 해당 `categories/<category>.md` 갱신
- 해당 `domains/<domain>.md`의 Known recurring mistakes 갱신
- 규칙 승격이 필요하면 이 파일, `CLAUDE.md`, `docs/AgentPromptTemplates.md`에 반영

## 스모크 검증 의도

BatchValidationRunner가 확인하는 내용:
- `Assets/02_Scenes/Game.unity` 존재 및 missing MonoBehaviour 없음
- `Assets/99_Tests/` 내 테스트 스크립트 존재
- 빌드 세팅에 씬이 등록되어 있는지 (Strict 모드)

## 태스크 라우팅 힌트

- **전투 버그**: `Gameplay/Player/`, `Gameplay/Ability/`, `Gameplay/Monster/` 위주
- **재화/성장**: `Gameplay/Player/Inventory.cs`, `Manager/PlayerLevelManager.cs`
- **Ability 추가**: `Gameplay/Ability/CustomAbility/`, `12.ScriptableObject/Ability/`
- **스폰/보스**: `Manager/SpawnManager.cs`, `Manager/StageManager/`
- **UI 동기화**: `UI/HUD/`, `Manager/GameSceneUIConnectManager.cs`
- **검증 도구**: `tools/`, `Assets/Editor/HarnessValidation/`

## 보고 기대값

- 변경한 파일 목록
- 실행한 검증 커맨드와 결과
- 씬, 프리팹, ScriptableObject 에셋에서의 미검증 리스크 명시

검증 결과 없이 Unity 작업이 완료됐다고 주장하지 않는다.
