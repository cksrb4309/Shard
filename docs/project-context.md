# Shard — Project Context

<!-- 자동 로드되지 않음. 시스템 구조, 비주얼 흐름, 데이터 흐름 관련 작업 시에만 로드. -->

## Core Systems

### Inventory
역할: EnergyCore(파편)와 SoulShard(영혼) 두 재화를 보관. setter에서 UI 갱신 자동 연결.
주요 연결: `CoreInteractUI`, `PlayerLevelManager`, `RewardManager`

### PlayerLevelManager
역할: XP 누적, 레벨업, `AbilitySelector.ShowSelectAbility()` 호출.
주요 연결: `Inventory`, `PlayerAttributes`, `AbilitySelector`

### AbilityManager
역할: Ability를 `AbilityEventType`(Attack/Critical/Kill/Passive) 이벤트 맵으로 관리. 중복 획득 시 스택 누적.
주요 연결: `AttackData`, `Ability`, `AbilityNameToIdMapper`

### SpawnManager
역할: 웨이브 코루틴 + 시간 카운터. `currentBlockCount < appearBossBlockCount`이면 `BossSpawn()` 전환.
주요 연결: `StageManager`, `PoolingManager`, `DifficultyManager`

### StageManager
역할: 보스 처치 후 결정체 복귀 대기 → 전장 복원 → 다음 스테이지 or 엔딩 페이즈 분기.
주요 연결: `SpawnManager`, `BlockGroup`, `DifficultyManager`, `EndingExplosion`

### PoolingManager
역할: 이름 기반 + 제네릭 컴포넌트 조회 지원. 미등록 객체 반환 시 즉시 Destroy.
주요 연결: 모든 스폰/투사체/이펙트 생성 지점

### CoreHealth / CoreUpgrade / CoreInteract
역할: 중앙 결정체 HP 관리, 파편 소비 강화, 플레이어 복귀 인터랙션.
주요 연결: `Inventory`, HUD UI

### DifficultyManager
역할: 스테이지 진행에 따른 몬스터 난이도 배율 제공.
주요 연결: `SpawnManager`

## Visual Flow

```
Gameplay Layer (MonoBehaviour)
    → 전투 이벤트 발생 (OnHit, OnKill, OnLevelUp)
AbilityManager.Dispatch(attackData)
    → abilityEventMap에서 해당 이벤트 타입 Ability 순회 발동
UI Layer (HUD, Canvas)
    → Inventory setter 자동 갱신 + RealtimeCanvasUI 알림
VFX / Particles
    → PoolingManager.GetObject<>() 꺼내서 재사용
```

## Data Flow

- 설정 위치: `Assets/12.ScriptableObject/`
  - `Ability/LevelUp/` — 레벨업 Ability SO
  - `Ability/UserCoreUpgrade/` — 코어 강화 Ability SO
  - `Spawn/MonsterSet/` — 스테이지별 몬스터 세트
  - `Spawn/Area/` — 스폰 영역
  - `Ship/ShipSelectOption/` — 캐릭터(함선) 선택 데이터
- 로드 시점: 씬 Awake/Start 에서 Inspector 참조로 로드
- 소비 시스템: `SpawnManager`, `AbilityManager`, `CoreInteract`, `PlayerEquipment`

## System Ordering / Lifecycle

초기화 순서:

1. `GameManager` Awake — 씬 진입 시 전역 매니저 준비
2. `PoolingManager` Awake — 오브젝트 풀 생성
3. `SpawnManager.StartMonsterSpawn()` — 웨이브 시작
4. 플레이어 `OnGameStartPlayerSetting` — 캐릭터 설정 적용
5. `DifficultyManager` 초기값 — 스테이지 1 난이도 세팅

Update 단계:

- Default Update: 플레이어 입력, 몬스터 추적, 투사체 이동
- Coroutine: 웨이브 스폰, 시간 증가, 보스 등장, 스테이지 전환, 엔딩 타이머

## Architecture Rules

- 모든 반복 생성 객체는 `PoolingManager`를 통해 관리. `Instantiate`/`Destroy` 직접 호출 금지
- Ability 추가 시 기존 `Dispatch` 경로를 수정하지 않고 `SubscribedEvents`만 설정
- `StopAllCoroutines()`는 보스 전환처럼 상태 전환이 명확할 때만 사용
- ScriptableObject 데이터는 런타임에서 직접 수정 금지 (런타임 값 캐시 사용)
