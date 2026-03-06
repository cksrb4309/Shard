using UnityEngine;

public class RewardManager : MonoBehaviour
{
    static RewardManager instance;

    private void Awake()
    {
        instance = this;
    }

    public static void BaseBlockDrop(int value)
    {
        Inventory.GainEnergeCore(value);

        PlayerLevelManager.AddExperience(10f);
    }
    public static void MonsterDrop(int value)
    {
        PlayerLevelManager.AddExperience((int)value * 10);

        Inventory.GainSoulShard(value);
    }
}