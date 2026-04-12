# Shard

`Shard`는 중앙 결정체를 거점으로 삼아 파편을 채굴하고, 적의 공세를 막아내며, 성장 선택을 누적해 보스 사이클을 돌파하는 거점 방어형 액션 게임 프로젝트입니다.  
이 문서는 게임 소개보다, 제가 실제로 구현한 게임플레이 시스템과 그 시스템을 어떤 코드 구조로 연결했는지 보여주는 개발자 포트폴리오 중심으로 정리했습니다.

## 기술 스택

`Unity` `C#` 

## 구현한 핵심 기능

- 파편, 영혼, 경험치를 분리한 이중 재화 기반 성장 구조 구현
- 전투 이벤트에 반응하는 Ability 시스템과 레벨업 능력 선택 구조 구현
- 블록 파괴 진행도에 따라 보스가 등장하는 웨이브/보스 전환 구조 구현
- 보스 처치 후 결정체 복귀, 전장 복원, 다음 스테이지 전환 흐름 구현
- 최종 엔딩 페이즈와 반복 생성 객체를 위한 오브젝트 풀링 구조 구현

## 1. 이중 재화 기반 성장 구조

- 파편은 코어 및 플레이어 강화에, 영혼은 동료 전투기 소환과 강화에 사용되도록 구현했습니다.
- 경험치는 전투와 채굴 모두에서 획득되며, 레벨업과 능력 선택으로 이어지도록 연결했습니다.
- 재화별 사용처를 분리해 플레이어가 채굴, 방어, 복귀를 반복하도록 게임플레이 흐름을 구성했습니다.

파일:
- `Assets/01_Scripts/Gameplay/Player/Inventory.cs`
- `Assets/01_Scripts/UI/HUD/CoreInteractUI.cs`
- `Assets/01_Scripts/Manager/PlayerLevelManager.cs`

<details>
  <summary><strong>코드 보기</strong></summary>

```csharp
int energyCore = 0;
int soulShard = 0;

int EnergyCore
{
    get => energyCore;
    set
    {
        // 파편 재화가 바뀌면 인벤토리 UI 수치도 즉시 함께 갱신한다.
        energyCore = value;
        InventoryUI.SetEnergyCoreText(energyCore);
    }
}

int SoulShard
{
    get => soulShard;
    set
    {
        // 영혼 재화도 같은 방식으로 UI 표시 값과 동기화한다.
        soulShard = value;
        InventoryUI.SetSoulShardText(soulShard);
    }
}

public void GainEnergeCoreApply(int value)
{
    // 블록 파괴로 얻은 보상을 파편 재화에 누적 적용한다.
    EnergyCore += value;
}

public void GainSoulShardApply(int value)
{
    // 적 처치로 얻은 보상을 영혼 재화에 누적 적용한다.
    SoulShard += value;
}
```

```csharp
bool IsCoreUpgrade()
{
    if (inventory.GetEnergyCore() >= currentCoreCost)
    {
        // 강화가 가능한 경우 현재 비용만큼 파편을 차감하고 다음 비용을 계산한다.
        inventory.UseEnergyCore(currentCoreCost);
        currentCoreCost = (int)(currentCoreCost * nextCoreCostMultiplier);
        coreUpgradeCostText.text = "필요 파편 : " + currentCoreCost.ToString();
        return true;
    }

    // 파편이 부족하면 강화 처리를 중단하고 안내 문구만 출력한다.
    RealtimeCanvasUI.NotificationText("비용이 모자랍니다");
    return false;
}

bool IsSummonUpgrade()
{
    if (inventory.GetSoulShard() >= currentSummonCost)
    {
        // 소환 또는 동료 강화가 가능한 경우 영혼을 차감하고 다음 비용을 갱신한다.
        inventory.UseSoulShard(currentSummonCost);
        currentSummonCost = (int)(currentSummonCost * nextSummonCostMultiplier);
        summonCostText.text = "필요 영혼 : " + currentSummonCost.ToString();
        return true;
    }

    // 영혼이 부족하면 동일하게 처리를 중단하고 실패 메시지를 보여준다.
    RealtimeCanvasUI.NotificationText("비용이 모자랍니다");
    return false;
}
```

```csharp
public void AddExperienceApply(float xp)
{
    // 획득한 경험치를 더한 뒤 한 번에 여러 레벨업이 가능하도록 반복 처리한다.
    CurrentXP += xp;

    while (CurrentXP > XPToNextLevel)
    {
        CurrentXP -= XPToNextLevel;
        LevelUp();
    }
}

public void LevelUp()
{
    // 레벨업 시 기본 스탯 보정과 능력 선택 UI 호출을 함께 처리한다.
    CurrentLevel++;
    XPToNextLevel *= 1.2f;
    PlayerAttributes.LevepUp();
    AbilitySelector.ShowSelectAbility();
}
```

</details>

## 2. 이벤트 기반 능력 시스템

- 능력은 `Attack`, `Critical`, `Kill`, `Passive` 같은 이벤트 타입에 연결되도록 구현했습니다.
- 능력 중복 획득 시 스택이 누적되고, 발동 방식은 이벤트 맵을 통해 처리되도록 구성했습니다.
- 새로운 능력을 추가하더라도 기존 전투 흐름을 크게 바꾸지 않고 확장할 수 있는 구조를 만들었습니다.

파일:
- `Assets/01_Scripts/Gameplay/Ability/AbilityManager.cs`
- `Assets/01_Scripts/Gameplay/Player/Attack/AttackData.cs`

<details>
  <summary><strong>코드 보기</strong></summary>

```csharp
private readonly Dictionary<AbilityEventType, List<Ability>> abilityEventMap = new();

public void RegisterAbility(Ability ability)
{
    // 능력 이름을 런타임 ID로 변환해 같은 능력을 중복 획득했는지 추적한다.
    AbilityNameToIdMapper.RegisterAbility(ability.abilityName);

    int relicId = AbilityNameToIdMapper.GetId(ability.abilityName);

    if (!abilityDict.ContainsKey(relicId))
    {
        // 처음 획득한 능력이라면 딕셔너리에 등록하고 기본 스택 수치를 부여한다.
        abilityDict[relicId] = ability;
        ability.SetCount(1);
    }
    else
    {
        // 이미 가지고 있는 능력이라면 새로 등록하지 않고 스택만 누적한다.
        abilityDict[relicId].Add();
    }

    if (ability.SubscribedEvents.Contains(AbilityEventType.Passive))
    {
        // 패시브 능력은 이벤트 발동형이 아니므로 이벤트 맵 등록을 생략한다.
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
        // 현재 전투 이벤트 타입에 연결된 능력만 순회하며 순차적으로 발동시킨다.
        foreach (var relic in relics)
        {
            relic.OnEvent(attackData);
        }
    }
}
```

</details>

## 3. 웨이브와 보스 전환 구조

- 기본 웨이브 스폰과 보스 스폰을 `SpawnManager`에서 통합 관리하도록 구현했습니다.
- 파괴된 블록 수가 누적되면 보스 등장 조건이 충족되도록 연결했습니다.
- 웨이브 스폰, 보스 등장 연출, 최종 엔딩 스폰까지 하나의 전투 진행 구조로 이어지게 만들었습니다.

파일:
- `Assets/01_Scripts/Manager/SpawnManager.cs`

<details>
  <summary><strong>코드 보기</strong></summary>

```csharp
public void StartMonsterSpawn()
{
    // 스테이지가 시작되면 진행도와 타이머를 초기화한 뒤 웨이브 생성 루프를 시작한다.
    currentStage++;

    time = 0;
    currentBlockCount = blockCount;

    StartCoroutine(SpawnCoroutine());
    StartCoroutine(StartIncreaseTimeCoroutine());
}

IEnumerator StartIncreaseTimeCoroutine()
{
    while (true)
    {
        yield return new WaitForSeconds(1f);

        time++;

        // 남은 블록 수가 임계치 아래로 내려가면 일반 웨이브를 멈추고 보스전으로 전환한다.
        if (currentBlockCount < appearBossBlockCount)
        {
            BossSpawn();
        }
    }
}

void BossSpawn()
{
    // 진행 중이던 일반 웨이브 코루틴을 모두 중단하고 보스 등장 코루틴만 실행한다.
    StopAllCoroutines();
    StartCoroutine(BossSpawnCoroutine());
}
```

```csharp
IEnumerator BossSpawnCoroutine()
{
    // 보스 등장 위치를 먼저 선택한 뒤 경고 연출을 보여주고 실제 보스를 등장시킨다.
    int spawnIndex = Random.Range(0, bossSpawnPoints.Length);

    waveStartPointEffect.transform.position = bossSpawnPoints[spawnIndex].position;
    waveStartPointEffect.Play();

    RealtimeCanvasUI.Notification(IconType.Boss, bossSpawnPoints[spawnIndex].position, "강력한 개체가 다가오고 있습니다");

    yield return new WaitForSeconds(1f);

    // 보스도 오브젝트 풀에서 꺼내 위치와 회전, 난이도 값을 초기화한다.
    CustomMonster boss = PoolingManager.Instance.GetObject<CustomMonster>(monsterSets[currentStage].bossMonster.mobName);

    boss.transform.position = bossSpawnPoints[spawnIndex].position;
    boss.transform.rotation = bossSpawnPoints[spawnIndex].rotation;

    boss.Setting(DifficultyManager.Difficulty, DifficultyManager.Difficulty);
}
```

</details>

## 4. 스테이지 전환과 엔딩 페이즈

- 보스 처치 후 바로 다음 전투로 넘어가지 않고, 결정체 복귀와 전장 복원 단계를 거치도록 구현했습니다.
- 전장 복원 이후 다음 스테이지의 웨이브와 난이도가 다시 시작되도록 연결했습니다.
- 마지막 스테이지에서는 과부하 타이머, 대량 스폰, 정화 폭발이 이어지는 엔딩 페이즈를 구현했습니다.

파일:
- `Assets/01_Scripts/Manager/StageManager/StageManager.cs`
- `Assets/01_Scripts/EndingExplosion.cs`

<details>
  <summary><strong>코드 보기</strong></summary>

```csharp
IEnumerator WaitNextStageCoroutine()
{
    if (currentStage == 2)
    {
        // 마지막 일반 스테이지가 끝나면 다음 스테이지 대신 엔딩 전용 전투 페이즈로 전환한다.
        currentStage = 3;
        yield return new WaitForSeconds(1f);

        SoundManager.Play("EndingBattleBGM", SoundType.Background);

        // 결정체가 충전되었다는 상태를 파티클과 UI 메시지로 먼저 전달한다.
        coreChargeParticle_1.Play();
        coreChargeParticle_2.Play();

        RealtimeCanvasUI.Notification(IconType.Charge, coreTransform.position, "결정체에 충분한 에너지가 모였습니다");
        yield return new WaitForSeconds(4f);
        RealtimeCanvasUI.Notification(IconType.Charge, "결정체의 과부하가 시작합니다");

        // 엔딩용 적 스폰과 생존 타이머를 동시에 시작해 마지막 전투 흐름을 만든다.
        spawnManager.EndingSpawn();
        StartCoroutine(EndingTimerCoroutine());
    }
    else
    {
        // 일반 스테이지 클리어 후에는 복귀 조건 확인과 전장 복원 단계를 먼저 수행한다.
        StartCoroutine(NextLevelCoroutine());

        searchNearPlayersObj.SetActive(true);
        while (!isAroundPlayers) yield return null;

        blockGroup.StageClear();

        stageChangeParticle_1.Play();
        stageChangeParticle_2.Play();

        StartCoroutine(NextStageColorCoroutine());

        yield return new WaitForSeconds(1f);

        // 전장 복원이 끝나면 다음 스테이지의 웨이브와 난이도 증가를 다시 시작한다.
        spawnManager.StartMonsterSpawn();
        DifficultyManager.NextStageSetting();
    }
}
```

```csharp
IEnumerator EndingTimerCoroutine()
{
    // 제한 시간 동안 적 공세를 버티게 한 뒤 결정체 폭발로 엔딩을 마무리한다.
    yield return new WaitForSeconds(30f);
    RealtimeCanvasUI.Notification(IconType.Charge, "30초 남았습니다");

    yield return new WaitForSeconds(25f);
    RealtimeCanvasUI.Notification(IconType.Charge, "5....");

    yield return new WaitForSeconds(5f);

    endingExplosion.StartExplosion();
    spawnManager.ClearEnding();

    SoundManager.Play("HappyEndingBGM", SoundType.Background);
}
```

</details>

## 5. 오브젝트 풀링 구조

- 몬스터, 투사체, 이펙트처럼 반복 생성되는 객체를 풀링 기반으로 재사용하도록 구현했습니다.
- 이름 기반 조회와 제네릭 컴포넌트 조회를 모두 지원해 전투 코드에서 바로 사용할 수 있도록 구성했습니다.
- 풀에 없는 객체가 반환될 경우 안전하게 파기되도록 처리했습니다.

파일:
- `Assets/01_Scripts/Manager/PoolingManager/PoolingManager.cs`
- `Assets/01_Scripts/Manager/PoolingManager/ObjectPool.cs`

<details>
  <summary><strong>코드 보기</strong></summary>

```csharp
private Dictionary<string, ObjectPool> pools = new Dictionary<string, ObjectPool>();

public void CreatePool(PoolEntry poolEntry, Transform parent)
{
    if (!pools.ContainsKey(poolEntry.name))
    {
        // 풀 이름별 전용 부모 오브젝트를 만들고 ObjectPool 컴포넌트를 붙여 초기화한다.
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
        // 호출 측이 필요한 컴포넌트 타입으로 바로 받아 사용할 수 있게 제네릭 조회를 지원한다.
        return pools[poolName].GetObject<T>();
    }
    return null;
}

public void ReturnObject(string poolName, GameObject obj)
{
    if (pools.ContainsKey(poolName))
    {
        // 해당 풀이 존재하면 객체를 파기하지 않고 재사용 대기열로 되돌린다.
        pools[poolName].ReturnObject(obj);
    }
    else
    {
        // 등록되지 않은 객체가 반환되면 누수 방지를 위해 즉시 파기한다.
        Destroy(obj);
    }
}
```

</details>

## 기술 포인트

- 파편, 영혼, 경험치를 분리해 채굴과 방어가 각각 다른 성장 축으로 이어지도록 구성했습니다.
- 이벤트 기반 능력 구조를 적용해 전투 규칙을 확장 가능한 형태로 구현했습니다.
- 보스 등장을 블록 파괴 진행도와 연결해 채굴과 전투 흐름이 분리되지 않도록 만들었습니다.
- 전장 복원 구조를 통해 같은 공간에서도 스테이지 전환 감각이 살아나도록 구성했습니다.
- 오브젝트 풀링을 적용해 반복 생성이 많은 전투 상황을 안정적으로 처리했습니다.

## 실행 환경

- Unity `6000.3.11f1`
- Scene
  - `Assets/02_Scenes/Title.unity`
  - `Assets/02_Scenes/Game.unity`
