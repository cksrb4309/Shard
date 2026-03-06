using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    static Inventory instance = null;

    public PlayerAttributes playerAttributes;

    public List<PlayerSkill> PlayerSkillList;

    Dictionary<int, Ability> abilityInventory = new Dictionary<int, Ability>();

    int energyCore = 0; // 에너지 결정체 [파편을 통해 얻을 수 있다]
    int soulShard = 0; // 영혼의 파편 [몬스터를 통해 얻을 수 있다]

    int EnergyCore
    {
        get
        {
            return energyCore;
        }
        set
        {
            energyCore = value;

            InventoryUI.SetEnergyCoreText(energyCore);
        }
    }
    int SoulShard
    {
        get
        {
            return soulShard;
        }
        set
        {
            soulShard = value;

            InventoryUI.SetSoulShardText(soulShard);
        }
    }

    private void Awake()
    {
        instance = this;

        AbilityManager.Instance.Init();
    }
    public static void GetAbility(Ability ability)
    {
        instance.GetAbilityApply(ability);
    }
    public void GetAbilityApply(Ability ability)
    {
        AbilityManager.Instance.RegisterAbility(ability);
    }
    public float GetEnergyCore() => EnergyCore;
    public void UseEnergyCore(int cost)
    {
        EnergyCore -= cost;
    }
    public float GetSoulShard() => SoulShard;
    public void UseSoulShard(int cost)
    {
        SoulShard -= cost;
    }
    public static void GainEnergeCore(int value)
    {
        instance.GainEnergeCoreApply(value);
    }
    public static void GainSoulShard(int value)
    {
        instance.GainSoulShardApply(value);
    }
    public void GainEnergeCoreApply(int value)
    {
        EnergyCore += value;
    }
    public void GainSoulShardApply(int value)
    {
        SoulShard += value;
    }
}