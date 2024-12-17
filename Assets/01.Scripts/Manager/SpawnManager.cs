using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public Transform[] defaultSpawnPoints;
    public Transform[] bossSpawnPoints;
    public Transform[] endingSpawnPoints; // ���� ���� ������ ���� ��ǥ
    public SpawnArea[] spawnAreas;
    public MonsterSet[] monsterSets; // ���������� MonsterSet

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
        // ���� ���� �ڷ�ƾ �� �ð� �ø��� �ڷ�ƾ ���� !
        StopAllCoroutines();

        StartCoroutine(BossSpawnCoroutine());
    }
    IEnumerator BossSpawnCoroutine()
    {
        // ���� �����Ѵٴ� ��� !
        int spawnIndex = Random.Range(0, bossSpawnPoints.Length);

        waveStartPointEffect.transform.position = bossSpawnPoints[spawnIndex].position;
        waveStartPointEffect.Play();

        RealtimeCanvasUI.Notification(IconType.Boss, bossSpawnPoints[spawnIndex].position, "������ ��ü�� �ٰ����� �ֽ��ϴ�");

        yield return new WaitForSeconds(1f);

        // ���� ���� !
        CustomMonster boss = PoolingManager.Instance.GetObject<CustomMonster>(monsterSets[currentStage].bossMonster.mobName);

        boss.transform.position = bossSpawnPoints[spawnIndex].position;
        boss.transform.rotation = bossSpawnPoints[spawnIndex].rotation;

        boss.Setting(DifficultyManager.Difficulty, DifficultyManager.Difficulty); // ���� ����
    }
    IEnumerator SpawnCoroutine()
    {
        yield return new WaitForSeconds(startSpawnDelay);

        while (true)
        {
            // ���� ��ų ��ġ �� ȸ�� �� ��������
            // ȸ�� ���� ������ ���� �� �̿��� (SpawnPoint�� �ھ� ������ �Ĵٺ��� �������)
            // ���ͳ����� �⺻���� ��ġ�� �������� ���� �Ŀ� �ش� ��ġ�� ȸ���� �����ؾ� �Ѵ�!


            Vector3 spawnPosition = defaultSpawnPoints[Random.Range(0, defaultSpawnPoints.Length)].position;
            Quaternion spawnRotation = defaultSpawnPoints[Random.Range(0, defaultSpawnPoints.Length)].rotation;

            RealtimeCanvasUI.Notification(IconType.Wave, spawnPosition, "������ �����ɴϴ�");

            // ���̺� ���� ���� ��ƼŬ ���
            waveStartPointEffect.transform.position = spawnPosition;
            waveStartPointEffect.Play();

            yield return new WaitForSeconds(1f);

            // ���� ����
            SpawnArea spawnArea = spawnAreas[Random.Range(0, spawnAreas.Length)];

            // ���͵� ��������
            List<CustomMonster> monsters = GetMonsters();

            foreach (CustomMonster monster in monsters)
            {
                #region ������ ��ȯ ���� ���� area
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

                // ���� ���� ��ƼŬ ���
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
            yield return new WaitForSeconds(Mathf.Lerp(endingStartSpawnDealy, endingEndSpawnDealy, Mathf.InverseLerp(0,62, endingTime))); // ���� ���� �ֱ�

            CustomMonster monster = set.RandomGetMonster(); // ���� ���� �� ������

            Transform spawnTransform = endingSpawnPoints[Random.Range(0, endingSpawnPoints.Length)];

            monster.transform.position = spawnTransform.position;

            monster.transform.rotation = spawnTransform.rotation;

            // ���� ���� ��ƼŬ ���
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