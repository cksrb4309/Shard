using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public Transform[] defaultSpawnPoints;
    public Transform[] bossSpawnPoints;

    public SpawnArea[] spawnAreas;
    public MonsterSet[] monsterSets; // ���������� MonsterSet

    public float startSpawnDelay = 10f;
    public float defaultSpawnDelay = 10f;

    public float difficultyAmount = 0.05f;
    public float difficultyDelay = 10f;

    public float padding = 2f;

    public int monsterSpawnCount = 10;

    int currentStage = 0;

    float time = 0;
    private void Start()
    {
        StartMonsterSpawn();

        
    }

    public void StartMonsterSpawn()
    {
        StartCoroutine(SpawnCoroutine());

        StartCoroutine(StartIncreaseTimeCoroutine());
    }
    IEnumerator StartIncreaseTimeCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);

            time++;
        }
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

            // ���� ����
            SpawnArea spawnArea = spawnAreas[Random.Range(0, spawnAreas.Length)];

            // ���͵� ��������
            List<Monster> monsters = GetMonsters();

            foreach (Monster monster in monsters)
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

                monster.Setting(DifficultyManager.Difficulty + 1, DifficultyManager.Difficulty + 1);
            }

            yield return new WaitForSeconds(defaultSpawnDelay);
        }
    }
    List<Monster> GetMonsters()
    {
        return monsterSets[currentStage].GetMonsterList(time);
    }
}