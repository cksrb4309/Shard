using System;
using System.Collections.Generic;

using UnityEngine;


public class PlayerAttributes : MonoBehaviour
{
    public static PlayerAttributes Instance => instance;

    static PlayerAttributes instance = null;

    const int AttributeCount = 24;

    public PlayerHealth health;
    public PlayerStatus status;
    public CustomPlayerInputAndMove move;

    public PlayerSkill normalAttack;
    public PlayerSkill mainSkill;
    public PlayerSkill subSkill;

    public Ability[] upgradeAbilityArray;

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

        LuckManager.SetLuck((int)GetAttribute(Attribute.Luck));
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
        passiveAttributes[(int)attribute] = baseAttributes[(int)attribute];

        foreach (float value in passiveStatModifiers[(int)attribute].Values)
        {
            if (CheckOperationType(attribute))  // 곱연산일 경우
                passiveAttributes[(int)attribute] *= value;
            else  // 합연산일 경우
                passiveAttributes[(int)attribute] += value;
        }

        UpdateBuffAttribute(attribute);
    }
    public void UpdateBuffAttribute(Attribute attribute) // 버프 적용
    {
        currentAttributes[(int)attribute] = passiveAttributes[(int)attribute];

        foreach (float value in activeBuffEffects[(int)attribute].Values)
        {
            if (CheckOperationType(attribute))  // 곱연산일 경우
                currentAttributes[(int)attribute] *= value;
            else  // 합연산일 경우
                currentAttributes[(int)attribute] += value;
        }

        UpdateAttributes(attribute);
    }
    void UpdateAttributes(Attribute attribute) // 적용 마무리 처리
    {
        StatusUI.Instance.UpdateAttribute(attribute);

        #region 공격력 적용
        if (attribute == Attribute.AttackDamage)
        {
            normalAttack.SetDamage(GetAttribute(attribute));
            mainSkill.SetDamage(GetAttribute(attribute));
            subSkill.SetDamage(GetAttribute(attribute));
        }
        #endregion
        #region 공격 속도 적용
        else if (attribute == Attribute.AttackSpeed)
        {
            normalAttack.SetCoolDown(GetAttribute(Attribute.AttackSpeed));
        }
        #endregion
        #region 방어력 적용
        else if (attribute == Attribute.FlatDefense || attribute == Attribute.RateDefense)
        {
            currentAttributes[(int)Attribute.Defense] =
                currentAttributes[(int)Attribute.FlatDefense] * currentAttributes[(int)Attribute.RateDefense];
        }
        #endregion
        #region 치명타 확률 적용
        else if (attribute == Attribute.FlatCriticalChance || attribute == Attribute.RateCriticalChance)
        {
            currentAttributes[(int)Attribute.CriticalChance] =
                currentAttributes[(int)Attribute.FlatCriticalChance] * currentAttributes[(int)Attribute.RateCriticalChance];
            normalAttack.SetCriticalChance(GetAttribute(Attribute.CriticalChance));
            mainSkill.SetCriticalChance(GetAttribute(Attribute.CriticalChance));
            subSkill.SetCriticalChance(GetAttribute(Attribute.CriticalChance));
        }
        #endregion
        #region 치명타 데미지 적용
        else if (attribute == Attribute.CriticalDamage)
        {
            normalAttack.SetCriticalDamage(GetAttribute(Attribute.CriticalDamage));
            mainSkill.SetCriticalDamage(GetAttribute(Attribute.CriticalDamage));
            subSkill.SetCriticalDamage(GetAttribute(Attribute.CriticalDamage));
        }
        #endregion
        #region 체력 적용
        else if (attribute == Attribute.FlatHp || attribute == Attribute.RateHp)
        {
            currentAttributes[(int)Attribute.Hp] =
                currentAttributes[(int)Attribute.FlatHp] * currentAttributes[(int)Attribute.RateHp];

            health.SetMaxHp(currentAttributes[(int)Attribute.Hp]);
        }
        #endregion
        #region 스킬 충전 스택 적용
        else if (attribute == Attribute.MainSkillStack)
        {
            // 스킬 최대 충전 스택 설정
            mainSkill.SetStack((int)GetAttribute(Attribute.MainSkillStack));
        }
        // 서브 스킬의 스택이 달라졌을 때
        else if (attribute == Attribute.SubSkillStack)
        {
            // 스킬 최대 충전 스택 설정
            subSkill.SetStack((int)GetAttribute(Attribute.SubSkillStack));
        }
        #endregion
        #region 스킬 쿨타임 시간 비율 감소
        else if (attribute == Attribute.MainSkillCooltimeRatio)
        {
            mainSkill.SetCooltimeRatio(GetAttribute(attribute));
        }
        else if (attribute == Attribute.SubSkillCooltimeRatio)
        {
            subSkill.SetCooltimeRatio(GetAttribute(attribute));
        }
        #endregion
        #region 투사체 적용
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
        #endregion
        #region 초당 체력 재생 적용
        else if (attribute == Attribute.HealthRegenRate)
        {
            health.SetHealthRegen(GetAttribute(attribute));
        }
        #endregion
        #region 행운 적용
        else if (attribute == Attribute.Luck)
        {
            LuckManager.SetLuck((int)GetAttribute(Attribute.Luck));
        }
        #endregion
        #region 체력 회복 증가
        else if (attribute == Attribute.Healing)
        {
            status.SetHealingMultiplier(GetAttribute(attribute));
        }
        #endregion
        #region 이동속도 적용
        else if (attribute == Attribute.MoveSpeed)
        {
            move.SetMoveSpeed(GetAttribute(attribute));
        }
        #endregion
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
            attribute == Attribute.Healing ||
            attribute == Attribute.ProjectileDuration ||
            attribute == Attribute.ProjectileSpeed) return true;
        return false;
    }
    public void ApplyPassiveAbilityAttribute(Attribute attribute, string abilityName, float value)
    {
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
    public static float Get(Attribute attribute) => instance.GetAttribute(attribute);
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
    MainSkillCoolDown,          // 메인 스킬 쿨다운 속도 증가
    MainSkillCooltimeRatio,          // 메인 스킬 쿨타임 초 감소
    SubSkillStack,              // 서브 스킬 누적 횟수
    SubSkillCoolDown,           // 서브 스킬 쿨다운 속도 증가
    SubSkillCooltimeRatio,           // 서브 스킬 쿨타임 초 감소
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