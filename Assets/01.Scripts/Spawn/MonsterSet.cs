using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MonsterSet", menuName = "Spawn/MonsterSet")]
public class MonsterSet : ScriptableObject
{
    public MonsterSpawnAmount[] monsterSpawnAmounts;
    public CustomMonster bossMonster;

    public List<CustomMonster> GetMonsterList(float time)
    {
        List<CustomMonster> monsterList = new List<CustomMonster>();

        foreach (MonsterSpawnAmount amount in monsterSpawnAmounts)
        {
            int count = (int)amount.spawnAmountCurve.Evaluate(time);

            for (int i = 0; i < count; i++)
                monsterList.Add(PoolingManager.Instance.GetObject<CustomMonster>(amount.monster.mobName));
        }
        return monsterList;
    }
}

[System.Serializable]
public class MonsterSpawnAmount
{
    public CustomMonster monster;
    public AnimationCurve spawnAmountCurve;
}