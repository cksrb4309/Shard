using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MonsterSet", menuName = "Spawn/MonsterSet")]
public class MonsterSet : ScriptableObject
{
    public MonsterSpawnAmount[] monsterSpawnAmounts;
    public Monster bossMonster;

    public List<Monster> GetMonsterList(float time)
    {
        List<Monster> monsterList = new List<Monster>();

        foreach (MonsterSpawnAmount amount in monsterSpawnAmounts)
        {
            int count = (int)amount.spawnAmountCurve.Evaluate(time);

            for (int i = 0; i < count; i++)
                monsterList.Add(PoolingManager.Instance.GetObject<Monster>(amount.monster.mobName));
        }
        return monsterList;
    }
}

[System.Serializable]
public class MonsterSpawnAmount
{
    public Monster monster;
    public AnimationCurve spawnAmountCurve;
}