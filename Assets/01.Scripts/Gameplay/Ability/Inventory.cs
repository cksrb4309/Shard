using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    static Inventory instance = null;

    public PlayerAttributes playerAttributes;

    public List<PlayerSkill> playerSkillList;

    Dictionary<int, TempAbility> abilityInventory = new Dictionary<int, TempAbility>();

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
    public static void GetAbility(TempAbility ability)
    {
        instance.GetAbilityApply(ability);
    }
    public void GetAbilityApply(TempAbility ability)
    {
        // �����Ƽ ID ���
        AbilityNameToIdMapper.RegisterAbility(ability.abilityName);

        // ���ϰ� �ִ� �����Ƽ�� ���
        if (abilityInventory.ContainsKey(AbilityNameToIdMapper.GetId(ability.abilityName)))
        {
            abilityInventory[AbilityNameToIdMapper.GetId(ability.abilityName)].Add();
        }
        // ������ �ʴ� �����Ƽ�� ���
        else
        {
            abilityInventory.Add(AbilityNameToIdMapper.GetId(ability.abilityName), ability);
            abilityInventory[AbilityNameToIdMapper.GetId(ability.abilityName)].SetCount(1);
        }

        // Icon ����!
        //InventoryUI.SetItem(ability, abilityInventory[AbilityNameToIdMapper.GetId(ability.abilityName)]);

        // ���� �нú� �����Ƽ�� ���
        if (ability.GetCondition() is IPassive passiveAbility)
        {
            var passiveAttribute = passiveAbility.GetValue();

            playerAttributes.ApplyPassiveAbilityAttribute(passiveAttribute.Item1, passiveAttribute.Item2, passiveAttribute.Item3);
        }
        // ���� óġ �� �ߵ� �����Ƽ�� ���
        else if (ability.GetCondition() is IOnKill onKillItem)
        {
            // ������ ������ ���� 1���� ��
            if (abilityInventory[AbilityNameToIdMapper.GetId(ability.abilityName)].GetCount() == 1)
            {
                foreach (var skill in playerSkillList)
                {
                    skill.AddOnKillAbility(onKillItem);
                }
            }
        }
        // ���� ������ �� �� �������� ������� Ȯ���� �ߵ��ϴ� �����Ƽ�� ���
        else if (ability.GetCondition() is IOnHitChanceDamage onHitChanceDamage)
        {
            // ������ ������ ���� 1���� ��
            if (abilityInventory[AbilityNameToIdMapper.GetId(ability.abilityName)].GetCount() == 1)
            {
                foreach (var skill in playerSkillList)
                {
                    skill.AddOnHitChanceDamage(onHitChanceDamage);
                }
            }
        }
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