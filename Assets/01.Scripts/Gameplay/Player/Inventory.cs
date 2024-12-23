using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    static Inventory instance = null;

    public PlayerAttributes playerAttributes;

    public List<PlayerSkill> playerSkillList;

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
    }
    public static void GetAbility(Ability ability)
    {
        instance.GetAbilityApply(ability);
    }
    public void GetAbilityApply(Ability ability)
    {
        // 어빌리티 ID 등록
        AbilityNameToIdMapper.RegisterAbility(ability.abilityName);

        int id = AbilityNameToIdMapper.GetId(ability.abilityName);

        // 지니고 있던 어빌리티일 경우
        if (abilityInventory.ContainsKey(id))
        {
            abilityInventory[id].Add();
        }
        // 지니지 않던 어빌리티일 경우
        else
        {
            abilityInventory.Add(id, ability);
            abilityInventory[id].SetCount(1);
        }

        // Icon 셋팅!
        //InventoryUI.SetItem(ability, abilityInventory[AbilityNameToIdMapper.GetId(ability.abilityName)]);

        // 만약 패시브 어빌리티일 경우
        if (ability.GetCondition() is IPassive passiveAbility)
        {
            var passiveAttribute = passiveAbility.GetValue();

            playerAttributes.ApplyPassiveAbilityAttribute(passiveAttribute.Item1, passiveAttribute.Item2, passiveAttribute.Item3);
        }
        // 만약 때렸을 때 총 데미지를 기반으로 발동하는 어빌리티일 경우
        else if (ability.GetCondition() is IOnHitDamage onHitDamage)
        {
            // 아이템 개수가 만약 1개일 때
            if (abilityInventory[id].GetCount() == 1)
            {
                foreach (var skill in playerSkillList)
                {
                    skill.AddOnHitDamage(onHitDamage);
                }
            }
        }
        // 만약 처치 시 발동 어빌리티일 경우
        else if (ability.GetCondition() is IOnKill onKillItem)
        {
            // 아이템 개수가 만약 1개일 때
            if (abilityInventory[id].GetCount() == 1)
            {
                foreach (var skill in playerSkillList)
                {
                    skill.AddOnKillAbility(onKillItem);
                }
            }
        }
        // 만약 때렸을 때 총 데미지를 기반으로 확률적 발동하는 어빌리티일 경우
        else if (ability.GetCondition() is IOnHitChanceDamage onHitChanceDamage)
        {
            // 아이템 개수가 만약 1개일 때
            if (abilityInventory[id].GetCount() == 1)
            {
                foreach (var skill in playerSkillList)
                {
                    skill.AddOnHitChanceDamage(onHitChanceDamage);
                }
            }
        }
        // 만약 때렸을 때 무조곤 실행하는 어빌리티일 경우
        else if (ability.GetCondition() is IOnHit onHit)
        {
            // 아이템 개수가 만약 1개일 때
            if (abilityInventory[id].GetCount() == 1)
            {
                foreach (var skill in playerSkillList)
                {
                    skill.AddOnHit(onHit);
                }
            }
        }
        // 만약 크리티컬일 시 무조곤 실행하는 어빌리티일 경우
        else if (ability.GetCondition() is IOnCritical onCritical)
        {
            // 아이템 개수가 만약 1개일 때
            if (abilityInventory[id].GetCount() == 1)
            {
                foreach (var skill in playerSkillList)
                {
                    skill.AddOnCritical(onCritical);
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