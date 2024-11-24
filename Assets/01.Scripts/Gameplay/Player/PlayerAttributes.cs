using System;
using System.Collections.Generic;

using UnityEngine;


public class PlayerAttributes : MonoBehaviour
{
    static PlayerAttributes instance = null;

    const int AttributeCount = 22;

    public PlayerHealth health;

    public PlayerSkill normalAttack;
    public PlayerSkill mainSkill;
    public PlayerSkill subSkill;

    //Dictionary<Attribute, Dictionary<string, float>> passiveStatModifiers = new Dictionary<Attribute, Dictionary<string, float>>();
    //Dictionary<Attribute, Dictionary<string, float>> activeBuffEffects = new Dictionary<Attribute, Dictionary<string, float>>();
    //Dictionary<Attribute, float> baseAttributes = new Dictionary<Attribute, float>();
    //Dictionary<Attribute, float> passiveAttributes = new Dictionary<Attribute, float>();
    //Dictionary<Attribute, float> currentAttributes = new Dictionary<Attribute, float>();

    Dictionary<int, float>[] passiveStatModifiers = new Dictionary<int, float>[AttributeCount];
    Dictionary<int, float>[] activeBuffEffects = new Dictionary<int, float>[AttributeCount];

    public InputAttribute[] inputAttributes;

    float[] baseAttributes = new float[AttributeCount];
    float[] passiveAttributes = new float[AttributeCount];
    float[] currentAttributes = new float[AttributeCount];

    private void Awake()
    {
        instance = this;

        Initialize();

        for (int i = 0; i < inputAttributes.Length; i++)
        {
            //baseAttributes[inputAttributes[i].Attribute] = inputAttributes[i].Value;
            baseAttributes[(int)inputAttributes[i].Attribute] = inputAttributes[i].Value;
            UpdatePassiveAttribute(inputAttributes[i].Attribute);
        }
        // 스킬 최대 충전 스택 설정
        mainSkill.SetStack((int)GetAttribute(Attribute.MainSkillStack));
        subSkill.SetStack((int)GetAttribute(Attribute.SubSkillStack));

        // 일반 공격의 스택 설정 [ 기본값인 1로 둔다 ]
        normalAttack.SetStack(1);

        // 스킬의 쿨다운 설정
        mainSkill.SetCoolDown(GetAttribute(Attribute.MainSkillCoolDown));
        subSkill.SetCoolDown(GetAttribute(Attribute.SubSkillCoolDown));
        normalAttack.SetCoolDown(GetAttribute(Attribute.AttackSpeed));

        // 치명타 확률 설정
        normalAttack.SetCriticalChance(GetAttribute(Attribute.CriticalChance));
        mainSkill.SetCriticalChance(GetAttribute(Attribute.CriticalChance));
        subSkill.SetCriticalChance(GetAttribute(Attribute.CriticalChance));

        // 플레이어 최대 체력 설정
        health.InitializeHp(GetAttribute(Attribute.Hp));
    }
    void Initialize()
    {
        for (int i = 0; i < AttributeCount; i++) baseAttributes[i] = 0;
        for (int i = 0; i < AttributeCount; i++) passiveAttributes[i] = 0;
        for (int i = 0; i < AttributeCount; i++) currentAttributes[i] = 0;

        for (int i = 0; i < AttributeCount; i++) passiveStatModifiers[i] = new Dictionary<int, float>();
        for (int i = 0; i < AttributeCount; i++) activeBuffEffects[i] = new Dictionary<int, float>();
    }
    public static void LevepUp()
    {
        instance.LevelUpApply();
    }
    public void LevelUpApply() // 레벨 업
    {
        baseAttributes[(int)Attribute.AttackDamage] *= 1.05f; // 공격력 업
        baseAttributes[(int)Attribute.FlatDefense] += 5f; // 고정 방어력 업
        baseAttributes[(int)Attribute.RateDefense] += 0.01f; // 고정 방어력 업
        baseAttributes[(int)Attribute.FlatHp] += 10; // 고정 체력 업

        UpdatePassiveAttribute(Attribute.AttackDamage);
        UpdatePassiveAttribute(Attribute.FlatDefense);
        UpdatePassiveAttribute(Attribute.RateDefense);
        UpdatePassiveAttribute(Attribute.FlatHp);
    }
    public void UpdatePassiveAttribute(Attribute attribute) // 패시브 적용
    {
        //passiveAttributes[attribute] = baseAttributes[attribute];
        passiveAttributes[(int)attribute] = baseAttributes[(int)attribute];

        // 해당 속성에 대해 지속적으로 적용되는 아이템이 있을 때
        //if (passiveStatModifiers.ContainsKey(attribute))
        //{
        //    foreach (float value in passiveStatModifiers[attribute].Values)
        //    {
        //        if (CheckOperationType(attribute))  // 곱연산일 경우
        //            passiveAttributes[attribute] *= value;
        //        else  // 합연산일 경우
        //            passiveAttributes[attribute] += value;
        //    }
        //}

        foreach (float value in passiveStatModifiers[(int)attribute].Values)
        {
            if (CheckOperationType(attribute))  // 곱연산일 경우
                //passiveAttributes[attribute] *= value;
                passiveAttributes[(int)attribute] *= value;
            else  // 합연산일 경우
                //passiveAttributes[attribute] += value;
                passiveAttributes[(int)attribute] += value;

        }

        UpdateBuffAttribute(attribute);
    }
    public void UpdateBuffAttribute(Attribute attribute) // 버프 적용
    {
        //currentAttributes[attribute] = passiveAttributes[attribute];
        currentAttributes[(int)attribute] = passiveAttributes[(int)attribute];

        // 해당 속성에 대해 지속적으로 적용되는 아이템이 있을 때
        //if (activeBuffEffects.ContainsKey(attribute))
        //{
        //    foreach (float value in activeBuffEffects[attribute].Values)
        //    {
        //        if (CheckOperationType(attribute))  // 곱연산일 경우
        //            currentAttributes[attribute] *= value;

        //        else  // 합연산일 경우
        //            currentAttributes[attribute] += value;
        //    }
        //}


        foreach (float value in activeBuffEffects[(int)attribute].Values)
        {
            if (CheckOperationType(attribute))  // 곱연산일 경우
                //currentAttributes[attribute] *= value;
                currentAttributes[(int)attribute] *= value;
            else  // 합연산일 경우
                //currentAttributes[attribute] += value;
                currentAttributes[(int)attribute] += value;
        }

        UpdateAttributes(attribute);
    }
    void UpdateAttributes(Attribute attribute) // 적용 마무리 처리
    {
        if (attribute == Attribute.AttackDamage)
        {
            normalAttack.SetDamage(GetAttribute(Attribute.AttackDamage));
            mainSkill.SetDamage(GetAttribute(Attribute.AttackDamage));
            subSkill.SetDamage(GetAttribute(Attribute.AttackDamage));
        }
        else if (attribute == Attribute.AttackSpeed)
        {
            normalAttack.SetCoolDown(GetAttribute(Attribute.AttackSpeed));
        }
        // 변화시킨 속성이 방어력과 관련 있을 경우
        else if (attribute == Attribute.FlatDefense || attribute == Attribute.RateDefense)
        {
            currentAttributes[(int)Attribute.Defense] =
                currentAttributes[(int)Attribute.FlatDefense] * currentAttributes[(int)Attribute.RateDefense];
        }
        // 변화시킨 속성이 치명타와 관련 있을 경우
        else if (attribute == Attribute.FlatCriticalChance || attribute == Attribute.RateCriticalChance)
        {
            currentAttributes[(int)Attribute.CriticalChance] =
                currentAttributes[(int)Attribute.FlatCriticalChance] * currentAttributes[(int)Attribute.RateCriticalChance];

            Debug.Log("치확 값 확인 : " + GetAttribute(Attribute.CriticalChance).ToString());

            normalAttack.SetCriticalChance(GetAttribute(Attribute.CriticalChance));
            mainSkill.SetCriticalChance(GetAttribute(Attribute.CriticalChance));
            subSkill.SetCriticalChance(GetAttribute(Attribute.CriticalChance));
        }
        else if (attribute == Attribute.CriticalDamage)
        {
            normalAttack.SetCriticalDamage(GetAttribute(Attribute.CriticalDamage));
            mainSkill.SetCriticalDamage(GetAttribute(Attribute.CriticalDamage));
            subSkill.SetCriticalDamage(GetAttribute(Attribute.CriticalDamage));
        }
        // 변화시킨 속성이 HP와 관련 있을 경우
        else if (attribute == Attribute.FlatHp || attribute == Attribute.RateHp)
        {
            currentAttributes[(int)Attribute.Hp] =
                currentAttributes[(int)Attribute.FlatHp] * currentAttributes[(int)Attribute.RateHp];

            health.SetMaxHp(currentAttributes[(int)Attribute.Hp]);
        }
        // 메인 스킬의 스택이 달라졌을 때
        else if (attribute == Attribute.MainSkillStack)
        {
            // 스킬 최대 충전 스택 설정
            mainSkill.SetStack((int)GetAttribute(Attribute.MainSkillStack));
        }
        // 서브 스킬의 스택이 달라졌을 때
        else if (attribute == Attribute.MainSkillStack)
        {
            // 스킬 최대 충전 스택 설정
            subSkill.SetStack((int)GetAttribute(Attribute.SubSkillStack));
        }
        else if (attribute == Attribute.ProjectileDuration)
        {
            normalAttack.SetProjectileDuration(GetAttribute(Attribute.ProjectileDuration));
            mainSkill.SetProjectileDuration(GetAttribute(Attribute.ProjectileDuration));
            subSkill.SetProjectileDuration(GetAttribute(Attribute.ProjectileDuration));
        }
        else if (attribute == Attribute.ProjectileSpeed)
        {
            normalAttack.SetProjectileSpeed(GetAttribute(Attribute.ProjectileSpeed));
            mainSkill.SetProjectileSpeed(GetAttribute(Attribute.ProjectileSpeed));
            subSkill.SetProjectileSpeed(GetAttribute(Attribute.ProjectileSpeed));
        }
        else if (attribute == Attribute.HealthRegenRate)
        {
            health.SetHealthRegen(GetAttribute(Attribute.HealthRegenRate));
        }
    }
    public float GetAttribute(Attribute attribute) => currentAttributes[(int)attribute];
    bool CheckOperationType(Attribute attribute) // true일 경우 곱연산 / false일 경우 합연산
    {
        if (attribute == Attribute.AttackDamage ||
            attribute == Attribute.AttackSpeed ||
            attribute == Attribute.MoveSpeed ||
            attribute == Attribute.CriticalDamage ||
            attribute == Attribute.RateCriticalChance ||
            attribute == Attribute.RateHp ||
            attribute == Attribute.Healing) return true;
        return false;
    }
    public void ApplyPassiveAbilityAttribute(Attribute attribute, string abilityName, float value)
    {
        //if (!passiveStatModifiers.ContainsKey(attribute)) passiveStatModifiers.Add(attribute, new Dictionary<string, float>());

        // 패시브 아이템의 Value를 그대로 적용한다
        //passiveStatModifiers[attribute][itemName] = value;

        passiveStatModifiers[(int)attribute][AbilityNameToIdMapper.GetId(abilityName)] = value;

        // 값을 갱신한다
        UpdatePassiveAttribute(attribute);
    }
    public void ApplyActiveBuffAttribute(Attribute attribute, string abilityName, float value)
    {
        //if (!activeBuffEffects.ContainsKey(attribute))
        //    activeBuffEffects.Add(attribute, new Dictionary<string, float>());

        // 버프 아이템의 Value를 그대로 적용한다
        //activeBuffEffects[attribute][itemName] = value;

        activeBuffEffects[(int)attribute][AbilityNameToIdMapper.GetId(abilityName)] = value;

        // 값을 갱신한다
        UpdateBuffAttribute(attribute);
    }
    public void RemovePassiveAbilityAttribute(Attribute attribute, string abilityName)
    {
        //passiveStatModifiers[attribute].Remove(itemName);

        passiveStatModifiers[(int)attribute].Remove(AbilityNameToIdMapper.GetId(abilityName));

        UpdatePassiveAttribute(attribute);
    }
    public void RemoveActiveBuffAttribute(Attribute attribute, string abilityName)
    {
        //activeBuffEffects[attribute].Remove(itemName);

        activeBuffEffects[(int)attribute].Remove(AbilityNameToIdMapper.GetId(abilityName));

        UpdateBuffAttribute(attribute);
    }
}
public enum Attribute {
    AttackDamage,               // 데미지
    AttackSpeed,                // 공격 속도
    MoveSpeed,                  // 이동 속도
    FlatDefense,                // 고정 방어력
    RateDefense,                // 비율 방어력
    Defense,                    // 고정과 비율로 산출된 방어력
    CriticalDamage,             // 치명타 데미지 
    FlatCriticalChance,         // 고정 치명타 확률
    RateCriticalChance,         // 비율 치명타 확률
    CriticalChance,             // 고정과 비율로 산출된 치명타
    FlatHp,                     // 고정 Hp
    RateHp,                     // 비율 Hp
    Hp,                         // 고정과 비율로 산출된 Hp
    Healing,                    // 회복 증가
    Luck,                       // 행운
    MainSkillStack,             // 메인 스킬 누적 횟수
    MainSkillCoolDown,          // 메인 스킬 쿨다운
    SubSkillStack,              // 서브 스킬 누적 횟수
    SubSkillCoolDown,           // 서브 스킬 쿨다운
    ProjectileDuration,         // 투사체 유지시간
    ProjectileSpeed,            // 투사체 속도
    HealthRegenRate,            // 초당 재생력

}

[System.Serializable]
public struct InputAttribute
{
    public Attribute Attribute;
    public float Value;
}