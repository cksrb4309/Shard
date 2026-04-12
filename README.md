# Shard

`Shard`는 중앙 결정체를 거점으로 삼아 파편을 채굴하고, 적의 공세를 막아내며, 성장 선택을 누적해 보스 사이클을 돌파하는 거점 방어형 액션 게임 프로젝트입니다.

이 프로젝트에서는 단순히 전투 요소를 추가하는 데 그치지 않고, `채굴`, `성장`, `방어`, `복귀`, `보스전`, `스테이지 전환`이 하나의 게임플레이 루프로 맞물리도록 시스템을 구현하는 데 집중했습니다.

## 한 줄 소개

파편 채굴, 적 처치, 성장 선택, 보스전 진입이 각각 분리된 기능으로 끊기지 않도록 게임플레이 시스템을 연결한 거점 방어형 액션 게임입니다.

## 핵심 게임플레이 루프

1. 결정체 주변의 파편 블록을 파괴해 자원을 획득한다.
2. 적을 처치해 추가 전투 재화를 확보한다.
3. 결정체로 복귀해 코어, 플레이어, 동료 전력을 강화한다.
4. 파편 파괴 진행도가 누적되면 보스가 등장한다.
5. 보스를 처치하면 전장을 복원하고 다음 스테이지로 넘어간다.
6. 마지막에는 결정체 과부하를 버텨내는 엔딩 페이즈를 수행한다.

이 루프에서 중요한 구현 포인트는, 각 행동이 독립적인 기능으로 끝나지 않도록 만드는 것이었습니다.

- 파편 채굴은 성장 재화 수급일 뿐 아니라 보스전 도달 속도를 당깁니다.
- 적 처치는 위협 제거에 그치지 않고 동료 전력 확장으로 이어집니다.
- 결정체 복귀는 재화 소비와 성장 선택이 일어나는 시스템 허브 역할을 합니다.

<details>
<summary><strong>게임플레이 목표 자세히 보기</strong></summary>

<br />

이 프로젝트에서 중점적으로 구현한 부분은 플레이어가 전장 안에서 반복적으로 선택을 수행하게 만드는 흐름입니다.

- 지금 더 바깥으로 나가 파편을 채굴할 것인가
- 결정체로 돌아가 현재 재화를 성장에 투자할 것인가
- 적을 더 정리해 영혼을 모을 것인가
- 보스 등장 전에 빌드를 더 완성할 것인가

즉, 액션 요소를 개별 기능으로 추가하는 것이 아니라 `판단이 누적되는 전투 루프`가 실제 플레이에서 동작하도록 시스템을 연결하는 데 초점을 맞췄습니다.

</details>

## 핵심 시스템

<details open>
<summary><strong>1. 이중 재화 기반 성장 구조</strong></summary>

<br />

### 1. 이중 재화 기반 성장 구조

성장 시스템은 하나의 자원으로 모든 선택을 해결하지 않도록 구성했습니다.

- 파편/에너지: 블록 파괴로 획득, 코어 및 플레이어 강화에 사용
- 영혼: 적 처치로 획득, 동료 전투기 소환 및 강화에 사용
- 경험치: 전투와 채굴을 통해 획득, 레벨업과 능력 선택에 사용

구현 관점에서 중요한 점은, 재화별 사용처를 분리해 서로 다른 플레이 행동이 서로 다른 성장 축으로 이어지도록 만든 것입니다.  
플레이어는 바깥에서 파편을 수급하고, 전투를 통해 영혼을 확보하며, 결정체 복귀를 통해 성장 선택을 수행하게 됩니다.

이 구조를 통해 성장 시스템이 단순 수치 증가 메뉴가 아니라, `공간 이동`, `전투 리스크`, `복귀 타이밍`과 연결되도록 구현했습니다.

</details>

<details open>
<summary><strong>2. 이벤트 기반 능력 시스템</strong></summary>

<br />

### 2. 이벤트 기반 능력 시스템

능력 시스템은 전투 이벤트에 반응하는 형태로 구현했습니다.

- 능력은 `Attack`, `Critical`, `Kill`, `Passive` 같은 이벤트 타입에 연결됩니다.
- 전투 결과는 `AttackData`를 통해 전달됩니다.
- 각 능력은 이벤트에 반응해 추가 공격, 상태 이상, 처치 보상, 조건부 효과를 실행합니다.

이 구조를 적용한 이유는 아래와 같습니다.

- 능력을 단순 수치 증가로 소비하지 않기
- 플레이어의 전투 양상에 따라 체감이 달라지도록 만들기
- 새로운 능력을 추가하더라도 기존 전투 구조를 크게 수정하지 않고 확장 가능하게 유지하기

즉, 능력은 단순 보상 목록이 아니라 `전투 규칙을 바꾸는 시스템 파츠`로 동작하도록 구현했습니다.

</details>

<details open>
<summary><strong>3. 보스와 스테이지 전환이 연결된 진행 구조</strong></summary>

<br />

### 3. 보스와 스테이지 전환이 연결된 진행 구조

보스전은 별도의 이벤트처럼 분리하지 않고, 전체 진행 흐름 안에서 자연스럽게 이어지도록 구현했습니다.

- 기본 웨이브는 외곽 스폰 포인트에서 반복 생성됩니다.
- 파괴된 블록 수가 누적되면 보스 등장 조건이 충족됩니다.
- 보스 처치 후에는 결정체 복귀, 전장 복원, 다음 스테이지 전환이 이어집니다.
- 최종 스테이지에서는 과부하 타이머와 대량 스폰을 결합한 엔딩 페이즈를 구현했습니다.

이 구조 덕분에 보스전은 갑자기 끼어드는 이벤트가 아니라, 이전까지의 채굴과 성장 진행도에 따라 도달하는 전투 단계로 동작합니다.

</details>

<details>
<summary><strong>4. 전장 복원 구조</strong></summary>

<br />

### 4. 전장 복원 구조

이 프로젝트에서는 전장을 일회성 공간으로 소비하지 않도록 구성했습니다.

- 플레이어는 파편을 파괴하며 전장을 바꿉니다.
- 보스 처치 후 결정체가 전장을 복원합니다.
- 복원된 공간은 다음 스테이지의 새로운 자원 상태와 압박 구조로 다시 사용됩니다.

같은 공간을 다시 사용하더라도, 단순 반복이 아니라 다음 단계의 전투 공간처럼 동작하도록 흐름을 구성했습니다.

</details>

## 구현 관점에서 중요했던 점

<details>
<summary><strong>구현 관점에서 중요했던 점 자세히 보기</strong></summary>

<br />

이 프로젝트에서 중요했던 점은 개별 기능을 추가하는 것보다, 시스템 간 연결이 플레이 흐름 안에서 자연스럽게 이어지도록 만드는 것이었습니다.

특히 아래 기준을 두고 구현했습니다.

- 채굴이 재화 수집뿐 아니라 보스 도달 속도와 연결되도록 만들기
- 적 처치가 위협 제거에 그치지 않고 다음 전력 확장으로 이어지게 만들기
- 성장 선택이 전투와 분리되지 않도록 결정체 복귀 흐름에 묶기
- 보스전과 스테이지 전환이 단절되지 않도록 하나의 진행 루프로 연결하기
- 엔딩 페이즈가 세션 마지막 전투 단계로 자연스럽게 이어지게 만들기

README를 보는 입장에서 중요하게 봐야 할 포인트도 여기에 있습니다.  
이 프로젝트는 기능 수를 늘리는 방향보다, `전투`, `재화`, `성장`, `웨이브`, `보스`, `엔딩`이 서로 어떤 관계로 묶이는지 코드와 흐름으로 정리한 프로젝트입니다.

</details>

## 사용 기술

- Unity 6 (`6000.3.11f1`)
- C#
- Universal Render Pipeline
- Unity Input System
- UGUI / TextMesh Pro
- Object Pooling

## 관련 구현 예시

- `AbilityManager`: 이벤트 기반 능력 등록 및 발동
- `PlayerLevelManager`: 경험치 누적, 레벨업, 능력 선택 트리거
- `Inventory`: 파편과 영혼 재화 분리 관리
- `CoreInteractUI`: 재화 소비와 성장 선택 연결
- `SpawnManager`: 웨이브 진행, 보스 등장, 엔딩 스폰 처리
- `StageManager`: 보스 처치 이후 스테이지 복원 및 엔딩 페이즈 전환
- `PoolingManager`: 반복 생성 오브젝트 재사용

<details>
<summary><strong>AbilityManager 코드 보기</strong></summary>

<br />

파일: `Assets/01_Scripts/Gameplay/Ability/AbilityManager.cs`

```csharp
private readonly Dictionary<AbilityEventType, List<Ability>> abilityEventMap = new();

public void RegisterAbility(Ability ability)
{
    AbilityNameToIdMapper.RegisterAbility(ability.abilityName);

    int relicId = AbilityNameToIdMapper.GetId(ability.abilityName);

    if (!abilityDict.ContainsKey(relicId))
    {
        abilityDict[relicId] = ability;
        ability.SetCount(1);
    }
    else
    {
        abilityDict[relicId].Add();
    }

    if (ability.SubscribedEvents.Contains(AbilityEventType.Passive))
    {
        return;
    }

    foreach (var type in ability.SubscribedEvents)
    {
        if (!abilityEventMap.ContainsKey(type))
        {
            abilityEventMap[type] = new List<Ability>();
        }
        if (!abilityEventMap[type].Contains(ability))
        {
            abilityEventMap[type].Add(ability);
        }
    }
}

public void Dispatch(AttackData attackData)
{
    if (abilityEventMap.TryGetValue(attackData.Type, out var relics))
    {
        foreach (var relic in relics)
        {
            relic.OnEvent(attackData);
        }
    }
}
```

</details>

<details>
<summary><strong>Inventory / CoreInteractUI 코드 보기</strong></summary>

<br />

파일: `Assets/01_Scripts/Gameplay/Player/Inventory.cs`  
파일: `Assets/01_Scripts/UI/HUD/CoreInteractUI.cs`

```csharp
int energyCore = 0;
int soulShard = 0;

int EnergyCore
{
    get => energyCore;
    set
    {
        energyCore = value;
        InventoryUI.SetEnergyCoreText(energyCore);
    }
}

int SoulShard
{
    get => soulShard;
    set
    {
        soulShard = value;
        InventoryUI.SetSoulShardText(soulShard);
    }
}

public void GainEnergeCoreApply(int value)
{
    EnergyCore += value;
}

public void GainSoulShardApply(int value)
{
    SoulShard += value;
}
```

```csharp
bool IsCoreUpgrade()
{
    if (inventory.GetEnergyCore() >= currentCoreCost)
    {
        inventory.UseEnergyCore(currentCoreCost);
        currentCoreCost = (int)(currentCoreCost * nextCoreCostMultiplier);
        coreUpgradeCostText.text = "필요 파편 : " + currentCoreCost.ToString();
        return true;
    }

    RealtimeCanvasUI.NotificationText("비용이 모자랍니다");
    return false;
}

bool IsSummonUpgrade()
{
    if (inventory.GetSoulShard() >= currentSummonCost)
    {
        inventory.UseSoulShard(currentSummonCost);
        currentSummonCost = (int)(currentSummonCost * nextSummonCostMultiplier);
        summonCostText.text = "필요 영혼 : " + currentSummonCost.ToString();
        return true;
    }

    RealtimeCanvasUI.NotificationText("비용이 모자랍니다");
    return false;
}
```

</details>

<details>
<summary><strong>SpawnManager / StageManager 코드 보기</strong></summary>

<br />

파일: `Assets/01_Scripts/Manager/SpawnManager.cs`  
파일: `Assets/01_Scripts/Manager/StageManager/StageManager.cs`

```csharp
IEnumerator StartIncreaseTimeCoroutine()
{
    while (true)
    {
        yield return new WaitForSeconds(1f);

        time++;

        if (currentBlockCount < appearBossBlockCount)
        {
            BossSpawn();
        }
    }
}

void BossSpawn()
{
    StopAllCoroutines();
    StartCoroutine(BossSpawnCoroutine());
}
```

```csharp
IEnumerator WaitNextStageCoroutine()
{
    if (currentStage == 2)
    {
        currentStage = 3;
        yield return new WaitForSeconds(1f);

        spawnManager.EndingSpawn();
        StartCoroutine(EndingTimerCoroutine());
    }
    else
    {
        StartCoroutine(NextLevelCoroutine());

        searchNearPlayersObj.SetActive(true);
        while (!isAroundPlayers) yield return null;

        blockGroup.StageClear();
        StartCoroutine(NextStageColorCoroutine());

        yield return new WaitForSeconds(1f);

        spawnManager.StartMonsterSpawn();
        DifficultyManager.NextStageSetting();
    }
}
```

</details>

<details>
<summary><strong>PoolingManager 코드 보기</strong></summary>

<br />

파일: `Assets/01_Scripts/Manager/PoolingManager/PoolingManager.cs`

```csharp
private Dictionary<string, ObjectPool> pools = new Dictionary<string, ObjectPool>();

public void CreatePool(PoolEntry poolEntry, Transform parent)
{
    if (!pools.ContainsKey(poolEntry.name))
    {
        GameObject poolObj = new GameObject(poolEntry.name + " Pool");
        poolObj.transform.SetParent(parent != null ? parent : transform);

        poolEntry.parent = poolObj.transform;

        ObjectPool pool = poolObj.AddComponent<ObjectPool>();
        pool.Setting(poolEntry, parent);

        pools.Add(poolEntry.name, pool);
    }
}

public T GetObject<T>(string poolName) where T : Component
{
    if (pools.ContainsKey(poolName))
    {
        return pools[poolName].GetObject<T>();
    }
    return null;
}

public void ReturnObject(string poolName, GameObject obj)
{
    if (pools.ContainsKey(poolName))
    {
        pools[poolName].ReturnObject(obj);
    }
    else
    {
        Destroy(obj);
    }
}
```

</details>

<details>
<summary><strong>프로젝트 구조 보기</strong></summary>

<br />

## 프로젝트 구조

```text
Assets
├─ 01_Scripts
│  ├─ Gameplay
│  ├─ Manager
│  ├─ Spawn
│  ├─ Title
│  └─ UI
├─ 02_Scenes
│  ├─ Game.unity
│  └─ Title.unity
└─ 08.OtherAsset
```

</details>

## 실행 방법

1. Unity Hub에서 `6000.3.11f1` 버전을 설치합니다.
2. 이 저장소를 Unity 프로젝트로 엽니다.
3. 패키지 복원이 완료되면 `Assets/02_Scenes/Title.unity` 또는 `Assets/02_Scenes/Game.unity`를 실행합니다.

<details>
<summary><strong>회고 보기</strong></summary>

<br />

## 회고

`Shard`를 통해 가장 집중해서 다룬 것은 게임플레이 기능을 개별적으로 만드는 것이 아니라, 전투 루프 안에서 서로 연결되게 구현하는 일이었습니다.

액션 게임은 타격감만으로 설명되지 않습니다.  
플레이어가 왜 이동하는지, 왜 복귀하는지, 어떤 타이밍에 성장 선택을 해야 하는지가 시스템으로 연결되어야 루프가 살아납니다.

이 프로젝트에서는 그 흐름을 아래처럼 구현하려 했습니다.

- 채굴은 성장으로 이어진다.
- 방어는 동료 전력 확장으로 이어진다.
- 복귀는 다음 국면의 선택으로 이어진다.
- 보스전은 지금까지의 선택을 검증한다.
- 엔딩은 세션 전체를 보상한다.

결과적으로 `Shard`는 전투 기능 몇 개를 추가한 프로젝트라기보다,  
`플레이어의 선택이 다음 플레이 흐름을 바꾸도록 게임플레이 시스템을 구현한 프로젝트`로 정리할 수 있습니다.

</details>
