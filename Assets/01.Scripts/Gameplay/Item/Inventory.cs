using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    static Inventory instance = null;

    public PlayerAttributes playerAttributes;

    public List<PlayerSkill> playerSkillList;

    Dictionary<int, int> AbilityInventory = new Dictionary<int, int>();

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
    }
    public static void GetAbility(Ability ability)
    {
        instance.GetAbilityApply(ability);
    }
    public void GetAbilityApply(Ability ability)
    {
        // 지니고 있던 어빌리티일 경우
        if (AbilityInventory.ContainsKey(AbilityNameToIdMapper.GetId(ability.abilityName)))
        {
            AbilityInventory[AbilityNameToIdMapper.GetId(ability.abilityName)]++;
        }
        // 지니지 않던 어빌리티일 경우
        else
        {
            AbilityInventory.Add(AbilityNameToIdMapper.GetId(ability.abilityName), 1);
        }

        // 어빌리티 ID 등록
        AbilityNameToIdMapper.RegisterAbility(ability.abilityName);

        // Icon 셋팅!
        InventoryUI.SetItem(ability, AbilityInventory[AbilityNameToIdMapper.GetId(ability.abilityName)]);

        // 만약 패시브 어빌리티일 경우
        if (ability.GetCondition() is IPassive passiveAbility)
        {
            var passiveAttribute = passiveAbility.GetValue(AbilityInventory[AbilityNameToIdMapper.GetId(ability.abilityName)]);

            playerAttributes.ApplyPassiveAbilityAttribute(passiveAttribute.Item1, passiveAttribute.Item2, passiveAttribute.Item3);
        }
        // 만약 처치 시 발동 어빌리티일 경우
        else if (ability.GetCondition() is IOnKill onKillItem)
        {
            foreach (var skill in playerSkillList)
            {
                skill.AddOnKillAbility(onKillItem);
            }
        }
    }
    public float GetEnergyCore() => EnergyCore;
    public void UseEnergyCore(int cost)
    {
        EnergyCore -= cost;
    }
    public float GetSoulShard() => SoulShard;
    public void UseSoulShar(int cost)
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