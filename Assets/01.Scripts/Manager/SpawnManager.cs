using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public Transform[] defaultSpawnPoints;
    public Transform[] bossSpawnPoints;
    public Transform[] endingSpawnPoints; // 엔딩 직전 나오는 몬스터 좌표
    public SpawnArea[] spawnAreas;
    public MonsterSet[] monsterSets; // 스테이지별 MonsterSet

    public float startSpawnDelay = 10f;
    public float defaultSpawnDelay = 10f;

    public float difficultyAmount = 0.05f;
    public float difficultyDelay = 10f;

    public float padding = 2f;

    public int monsterSpawnCount = 10;

    public ParticleSystem waveStartPointEffect;
    public ParticleSystem monsterSpawnEffect;

    [Range(0, 7568)] public int appearBossBlockCount;

    int currentStage = -1;

    float time = 0;
    public static int currentBlockCount;
    const int blockCount = 7569;
    private void Start()
    {
        StartMonsterSpawn();
    }
    public void StartMonsterSpawn()
    {
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

            if (currentBlockCount < appearBossBlockCount)
            {
                BossSpawn();
            }
        }
    }
    void BossSpawn()
    {
        // 스폰 중인 코루틴 및 시간 올리던 코루틴 종료 !
        StopAllCoroutines();

        StartCoroutine(BossSpawnCoroutine());
    }
    IEnumerator BossSpawnCoroutine()
    {
        // 보스 등장한다는 경고 !
        int spawnIndex = Random.Range(0, bossSpawnPoints.Length);

        waveStartPointEffect.transform.position = bossSpawnPoints[spawnIndex].position;
        waveStartPointEffect.Play();

        RealtimeCanvasUI.Notification(IconType.Boss, bossSpawnPoints[spawnIndex].position, "강력한 개체가 다가오고 있습니다");

        yield return new WaitForSeconds(1f);

        // 보스 스폰 !
        CustomMonster boss = PoolingManager.Instance.GetObject<CustomMonster>(monsterSets[currentStage].bossMonster.mobName);

        boss.transform.position = bossSpawnPoints[spawnIndex].position;
        boss.transform.rotation = bossSpawnPoints[spawnIndex].rotation;

        boss.Setting(DifficultyManager.Difficulty, DifficultyManager.Difficulty); // 보스 설정
    }
    IEnumerator SpawnCoroutine()
    {
        yield return new WaitForSeconds(startSpawnDelay);

        while (true)
        {
            // 스폰 시킬 위치 및 회전 값 가져오기
            // 회전 값은 진형을 만들 때 이용함 (SpawnPoint는 코어 방향을 쳐다보게 만들었음)
            // 몬스터끼리의 기본적인 위치를 진형으로 정한 후에 해당 위치와 회전을 적용해야 한다!


            Vector3 spawnPosition = defaultSpawnPoints[Random.Range(0, defaultSpawnPoints.Length)].position;
            Quaternion spawnRotation = defaultSpawnPoints[Random.Range(0, defaultSpawnPoints.Length)].rotation;

            RealtimeCanvasUI.Notification(IconType.Wave, spawnPosition, "적들이 몰려옵니다");

            // 웨이브 시작 지점 파티클 재생
            waveStartPointEffect.transform.position = spawnPosition;
            waveStartPointEffect.Play();

            yield return new WaitForSeconds(1f);

            // 진형 선택
            SpawnArea spawnArea = spawnAreas[Random.Range(0, spawnAreas.Length)];

            // 몬스터들 가져오기
            List<CustomMonster> monsters = GetMonsters();

            foreach (CustomMonster monster in monsters)
            {
                #region 몬스터의 소환 영역 선택 area
                List<Area> areaList;
                Area area = null;

                switch (monster.monsterType)
                {
                    case MonsterType.Front:
                        areaList = spawnArea.frontSpawnArea; break;
                    case MonsterType.Back:
                        areaList = spawnArea.backSpawnArea; break;
                    default: // MonsterType.Mid
                        areaList = spawnArea.midSpawnArea; break;
                }
                if (areaList.Count > 1)
                {
                    for (int i = 0; i < areaList.Count; i++)
                    {
                        if (areaList[i].weight > Random.value)
                        {
                            area = areaList[i];
                            break;
                        }
                    }

                    if (area == null)
                        area = areaList[Random.Range(0, areaList.Count)];
                }
                else
                {
                    area = spawnArea.frontSpawnArea[0];
                }
                #endregion

                monster.transform.position = area.area.center +
                    new Vector3(Random.Range(-area.area.size.x, area.area.size.x), 0, Random.Range(-area.area.size.z, area.area.size.z));

                monster.transform.position = (spawnRotation * monster.transform.position) + spawnPosition;

                monster.transform.rotation = spawnRotation;

                // 몬스터 스폰 파티클 재생
                monsterSpawnEffect.transform.position = monster.transform.position;
                monsterSpawnEffect.Play();

                monster.Setting(DifficultyManager.Difficulty, DifficultyManager.Difficulty);
            }

            yield return new WaitForSeconds(defaultSpawnDelay);
        }
    }
    List<CustomMonster> GetMonsters()
    {
        return monsterSets[currentStage].GetMonsterList(time);
    }
    Coroutine endingSpawnCoroutine = null;
    public void EndingSpawn()
    {
        endingSpawnCoroutine = StartCoroutine(EndingSpawnCoroutine());
    }
    public void ClearEnding()
    {
        StopCoroutine(endingSpawnCoroutine);
    }
    float endingTime = 0;
    float endingStartSpawnDealy = 0.6f;
    float endingEndSpawnDealy = 0.05f;
    IEnumerator EndingSpawnCoroutine()
    {
        endingTime = 0;

        MonsterSet set = monsterSets[3];

        while (true)
        {
            yield return new WaitForSeconds(Mathf.Lerp(endingStartSpawnDealy, endingEndSpawnDealy, Mathf.InverseLerp(0,62, endingTime))); // 몬스터 생성 주기

            CustomMonster monster = set.RandomGetMonster(); // 몬스터 생성 후 가져옴

            Transform spawnTransform = endingSpawnPoints[Random.Range(0, endingSpawnPoints.Length)];

            monster.transform.position = spawnTransform.position;

            monster.transform.rotation = spawnTransform.rotation;

            // 몬스터 스폰 파티클 재생
            monsterSpawnEffect.transform.position = monster.transform.position;
            monsterSpawnEffect.Play();

            monster.Setting(DifficultyManager.Difficulty, DifficultyManager.Difficulty);
        }
    }
    private void FixedUpdate()
    {
        endingTime += Time.fixedDeltaTime;
    }
}