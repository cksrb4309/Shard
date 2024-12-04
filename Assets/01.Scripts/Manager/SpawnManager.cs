using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public Transform[] defaultSpawnPoints;
    public Transform[] bossSpawnPoints;

    public SpawnArea[] spawnAreas;
    public MonsterSet[] monsterSets; // 스테이지별 MonsterSet

    public float startSpawnDelay = 10f;
    public float defaultSpawnDelay = 10f;

    public float difficultyAmount = 0.05f;
    public float difficultyDelay = 10f;

    public float padding = 2f;

    public int monsterSpawnCount = 10;

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
    //IEnumerator BossSpawnCoroutine()
    //{
    //    // 보스 등장한다는 경고 !
    //    Debug.Log("보스 등장 ㅋㅋ !");

    //    yield return null;

    //    // 보스 스폰 !
    //    Monster boss = PoolingManager.Instance.GetObject<Monster>(monsterSets[currentStage].bossMonster.mobName);

    //    int spawnIndex = Random.Range(0, bossSpawnPoints.Length);

    //    boss.transform.position = bossSpawnPoints[spawnIndex].position;
    //    boss.transform.rotation = bossSpawnPoints[spawnIndex].rotation;

    //    boss.Setting(DifficultyManager.Difficulty, DifficultyManager.Difficulty); // 보스 설정
    //}
    //IEnumerator SpawnCoroutine()
    //{
    //    yield return new WaitForSeconds(startSpawnDelay);

    //    while (true)
    //    {
    //        // 스폰 시킬 위치 및 회전 값 가져오기
    //        // 회전 값은 진형을 만들 때 이용함 (SpawnPoint는 코어 방향을 쳐다보게 만들었음)
    //        // 몬스터끼리의 기본적인 위치를 진형으로 정한 후에 해당 위치와 회전을 적용해야 한다!

    //        Vector3 spawnPosition = defaultSpawnPoints[Random.Range(0, defaultSpawnPoints.Length)].position;
    //        Quaternion spawnRotation = defaultSpawnPoints[Random.Range(0, defaultSpawnPoints.Length)].rotation;

    //        // 진형 선택
    //        SpawnArea spawnArea = spawnAreas[Random.Range(0, spawnAreas.Length)];

    //        // 몬스터들 가져오기
    //        List<Monster> monsters = GetMonsters();

    //        foreach (Monster monster in monsters)
    //        {
    //            #region 몬스터의 소환 영역 선택 area
    //            List<Area> areaList;
    //            Area area = null;

    //            switch (monster.monsterType)
    //            {
    //                case MonsterType.Front:
    //                    areaList = spawnArea.frontSpawnArea; break;
    //                case MonsterType.Back:
    //                    areaList = spawnArea.backSpawnArea; break;
    //                default: // MonsterType.Mid
    //                    areaList = spawnArea.midSpawnArea; break;
    //            }
    //            if (areaList.Count > 1)
    //            {
    //                for (int i = 0; i < areaList.Count; i++)
    //                {
    //                    if (areaList[i].weight > Random.value)
    //                    {
    //                        area = areaList[i];
    //                        break;
    //                    }
    //                }

    //                if (area == null)
    //                    area = areaList[Random.Range(0, areaList.Count)];
    //            }
    //            else
    //            {
    //                area = spawnArea.frontSpawnArea[0];
    //            }
    //            #endregion

    //            monster.transform.position = area.area.center + 
    //                new Vector3(Random.Range(-area.area.size.x, area.area.size.x), 0, Random.Range(-area.area.size.z, area.area.size.z));

    //            monster.transform.position = (spawnRotation * monster.transform.position) + spawnPosition;

    //            monster.Setting(DifficultyManager.Difficulty, DifficultyManager.Difficulty);
    //        }

    //        yield return new WaitForSeconds(defaultSpawnDelay);
    //    }
    //}
    //List<Monster> GetMonsters()
    //{
    //    return monsterSets[currentStage].GetMonsterList(time);
    //}




    IEnumerator BossSpawnCoroutine()
    {
        // 보스 등장한다는 경고 !
        Debug.Log("보스 등장 ㅋㅋ !");

        yield return null;

        // 보스 스폰 !
        CustomMonster boss = PoolingManager.Instance.GetObject<CustomMonster>(monsterSets[currentStage].bossMonster.mobName);

        int spawnIndex = Random.Range(0, bossSpawnPoints.Length);

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

                monster.Setting(DifficultyManager.Difficulty, DifficultyManager.Difficulty);
            }

            yield return new WaitForSeconds(defaultSpawnDelay);
        }
    }
    List<CustomMonster> GetMonsters()
    {
        return monsterSets[currentStage].GetMonsterList(time);
    }

}