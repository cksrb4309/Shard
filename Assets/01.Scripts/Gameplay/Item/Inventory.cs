using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    static Inventory instance = null;

    public PlayerAttributes playerAttributes;

    public List<PlayerSkill> playerSkillList;

    Dictionary<int, int> AbilityInventory = new Dictionary<int, int>();

    int energyCore = 0; // ������ ����ü [������ ���� ���� �� �ִ�]
    int soulShard = 0; // ��ȥ�� ���� [���͸� ���� ���� �� �ִ�]

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
        // ���ϰ� �ִ� �����Ƽ�� ���
        if (AbilityInventory.ContainsKey(AbilityNameToIdMapper.GetId(ability.abilityName)))
        {
            AbilityInventory[AbilityNameToIdMapper.GetId(ability.abilityName)]++;
        }
        // ������ �ʴ� �����Ƽ�� ���
        else
        {
            AbilityInventory.Add(AbilityNameToIdMapper.GetId(ability.abilityName), 1);
        }

        // �����Ƽ ID ���
        AbilityNameToIdMapper.RegisterAbility(ability.abilityName);

        // Icon ����!
        InventoryUI.SetItem(ability, AbilityInventory[AbilityNameToIdMapper.GetId(ability.abilityName)]);

        // ���� �нú� �����Ƽ�� ���
        if (ability.GetCondition() is IPassive passiveAbility)
        {
            var passiveAttribute = passiveAbility.GetValue(AbilityInventory[AbilityNameToIdMapper.GetId(ability.abilityName)]);

            playerAttributes.ApplyPassiveAbilityAttribute(passiveAttribute.Item1, passiveAttribute.Item2, passiveAttribute.Item3);
        }
        // ���� óġ �� �ߵ� �����Ƽ�� ���
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