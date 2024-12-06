using System;
using System.Collections.Generic;

using UnityEngine;


public class PlayerAttributes : MonoBehaviour
{
    static PlayerAttributes instance = null;

    const int AttributeCount = 24;

    public PlayerHealth health;
    public PlayerStatus status;
    public PlayerInputAndMove move;

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
        // ��ų �ִ� ���� ���� ����
        mainSkill.SetStack((int)GetAttribute(Attribute.MainSkillStack));
        subSkill.SetStack((int)GetAttribute(Attribute.SubSkillStack));

        // �Ϲ� ������ ���� ���� [ �⺻���� 1�� �д� ]
        normalAttack.SetStack(1);

        // ��ų�� ��ٿ� ����
        mainSkill.SetCoolDown(GetAttribute(Attribute.MainSkillCoolDown));
        subSkill.SetCoolDown(GetAttribute(Attribute.SubSkillCoolDown));
        normalAttack.SetCoolDown(GetAttribute(Attribute.AttackSpeed));

        // ġ��Ÿ Ȯ�� ����
        normalAttack.SetCriticalChance(GetAttribute(Attribute.CriticalChance));
        mainSkill.SetCriticalChance(GetAttribute(Attribute.CriticalChance));
        subSkill.SetCriticalChance(GetAttribute(Attribute.CriticalChance));

        // �÷��̾� �ִ� ü�� ����
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
    public void LevelUpApply() // ���� ��
    {
        baseAttributes[(int)Attribute.AttackDamage] *= 1.05f; // ���ݷ� ��
        baseAttributes[(int)Attribute.FlatDefense] += 5f; // ���� ���� ��
        baseAttributes[(int)Attribute.RateDefense] += 0.01f; // ���� ���� ��
        baseAttributes[(int)Attribute.FlatHp] += 10; // ���� ü�� ��

        UpdatePassiveAttribute(Attribute.AttackDamage);
        UpdatePassiveAttribute(Attribute.FlatDefense);
        UpdatePassiveAttribute(Attribute.RateDefense);
        UpdatePassiveAttribute(Attribute.FlatHp);
    }
    public void UpdatePassiveAttribute(Attribute attribute) // �нú� ����
    {

        passiveAttributes[(int)attribute] = baseAttributes[(int)attribute];

        foreach (float value in passiveStatModifiers[(int)attribute].Values)
        {
            if (CheckOperationType(attribute))  // �������� ���
                passiveAttributes[(int)attribute] *= value;
            else  // �տ����� ���
                passiveAttributes[(int)attribute] += value;
        }

        UpdateBuffAttribute(attribute);
    }
    public void UpdateBuffAttribute(Attribute attribute) // ���� ����
    {
        //currentAttributes[attribute] = passiveAttributes[attribute];
        currentAttributes[(int)attribute] = passiveAttributes[(int)attribute];

        // �ش� �Ӽ��� ���� ���������� ����Ǵ� �������� ���� ��
        //if (activeBuffEffects.ContainsKey(attribute))
        //{
        //    foreach (float value in activeBuffEffects[attribute].Values)
        //    {
        //        if (CheckOperationType(attribute))  // �������� ���
        //            currentAttributes[attribute] *= value;

        //        else  // �տ����� ���
        //            currentAttributes[attribute] += value;
        //    }
        //}


        foreach (float value in activeBuffEffects[(int)attribute].Values)
        {
            if (attribute == Attribute.MainSkillCooltimeRatio || attribute == Attribute.SubSkillCooltimeRatio)
            {
                Debug.Log(attribute.ToString() + " BeforeB : " + passiveAttributes[(int)attribute].ToString());

                Debug.Log(" Value : " + value.ToString());
            }


            if (CheckOperationType(attribute))  // �������� ���
                //currentAttributes[attribute] *= value;
                currentAttributes[(int)attribute] *= value;
            else  // �տ����� ���
                //currentAttributes[attribute] += value;
                currentAttributes[(int)attribute] += value;


            if (attribute == Attribute.MainSkillCooltimeRatio || attribute == Attribute.SubSkillCooltimeRatio)
            {
                Debug.Log(attribute.ToString() + " AfterB : " + passiveAttributes[(int)attribute].ToString());

                Debug.Log(" Value : " + value.ToString());
            }
        }

        UpdateAttributes(attribute);
    }
    void UpdateAttributes(Attribute attribute) // ���� ������ ó��
    {
        #region ���ݷ� ����
        if (attribute == Attribute.AttackDamage)
        {
            normalAttack.SetDamage(GetAttribute(attribute));
            mainSkill.SetDamage(GetAttribute(attribute));
            subSkill.SetDamage(GetAttribute(attribute));
        }
        #endregion
        #region ���� �ӵ� ����
        else if (attribute == Attribute.AttackSpeed)
        {
            normalAttack.SetCoolDown(GetAttribute(Attribute.AttackSpeed));
        }
        #endregion
        #region ���� ����
        else if (attribute == Attribute.FlatDefense || attribute == Attribute.RateDefense)
        {
            currentAttributes[(int)Attribute.Defense] =
                currentAttributes[(int)Attribute.FlatDefense] * currentAttributes[(int)Attribute.RateDefense];
        }
        #endregion
        #region ġ��Ÿ Ȯ�� ����
        else if (attribute == Attribute.FlatCriticalChance || attribute == Attribute.RateCriticalChance)
        {
            currentAttributes[(int)Attribute.CriticalChance] =
                currentAttributes[(int)Attribute.FlatCriticalChance] * currentAttributes[(int)Attribute.RateCriticalChance];

            normalAttack.SetCriticalChance(GetAttribute(Attribute.CriticalChance));
            mainSkill.SetCriticalChance(GetAttribute(Attribute.CriticalChance));
            subSkill.SetCriticalChance(GetAttribute(Attribute.CriticalChance));
        }
        #endregion
        #region ġ��Ÿ ������ ����
        else if (attribute == Attribute.CriticalDamage)
        {
            normalAttack.SetCriticalDamage(GetAttribute(Attribute.CriticalDamage));
            mainSkill.SetCriticalDamage(GetAttribute(Attribute.CriticalDamage));
            subSkill.SetCriticalDamage(GetAttribute(Attribute.CriticalDamage));
        }
        #endregion
        #region ü�� ����
        else if (attribute == Attribute.FlatHp || attribute == Attribute.RateHp)
        {
            currentAttributes[(int)Attribute.Hp] =
                currentAttributes[(int)Attribute.FlatHp] * currentAttributes[(int)Attribute.RateHp];

            health.SetMaxHp(currentAttributes[(int)Attribute.Hp]);
        }
        #endregion
        #region ��ų ���� ���� ����
        else if (attribute == Attribute.MainSkillStack)
        {
            // ��ų �ִ� ���� ���� ����
            mainSkill.SetStack((int)GetAttribute(Attribute.MainSkillStack));

            Debug.Log("���� ��ų ���� Ȯ�� : " + (int)GetAttribute(Attribute.MainSkillStack));
        }
        // ���� ��ų�� ������ �޶����� ��
        else if (attribute == Attribute.SubSkillStack)
        {
            // ��ų �ִ� ���� ���� ����
            subSkill.SetStack((int)GetAttribute(Attribute.SubSkillStack));

            Debug.Log("���� ��ų ���� Ȯ�� : " + (int)GetAttribute(Attribute.SubSkillStack));
        }
        #endregion
        #region ��ų ��Ÿ�� �ð� ���� ����
        else if (attribute == Attribute.MainSkillCooltimeRatio)
        {
            mainSkill.SetCooltimeRatio(GetAttribute(attribute));
        }
        else if (attribute == Attribute.SubSkillCooltimeRatio)
        {
            subSkill.SetCooltimeRatio(GetAttribute(attribute));
        }
        #endregion
        #region ����ü ����
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
        #region �ʴ� ü�� ��� ����
        else if (attribute == Attribute.HealthRegenRate)
        {
            health.SetHealthRegen(GetAttribute(attribute));
        }
        #endregion
        #region ��� ����
        else if (attribute == Attribute.Luck)
        {
            LuckManager.SetLuck((int)GetAttribute(Attribute.Luck));
        }
        #endregion
        #region ü�� ȸ�� ����
        else if (attribute == Attribute.Healing)
        {
            status.SetHealingMultiplier(GetAttribute(attribute));
        }
        #endregion
        #region �̵��ӵ� ����
        else if (attribute == Attribute.MoveSpeed)
        {
            move.SetMoveSpeed(GetAttribute(attribute));
        }
        #endregion
    }
    public float GetAttribute(Attribute attribute) => currentAttributes[(int)attribute];
    bool CheckOperationType(Attribute attribute) // true�� ��� ������ / false�� ��� �տ���
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

        // ���� �����Ѵ�
        UpdatePassiveAttribute(attribute);
    }
    public void ApplyActiveBuffAttribute(Attribute attribute, string abilityName, float value)
    {
        //if (!activeBuffEffects.ContainsKey(attribute))
        //    activeBuffEffects.Add(attribute, new Dictionary<string, float>());

        // ���� �������� Value�� �״�� �����Ѵ�
        //activeBuffEffects[attribute][itemName] = value;

        activeBuffEffects[(int)attribute][AbilityNameToIdMapper.GetId(abilityName)] = value;

        // ���� �����Ѵ�
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
    AttackDamage,               // ������
    AttackSpeed,                // ���� �ӵ�
    MoveSpeed,                  // �̵� �ӵ�
    FlatDefense,                // ���� ����
    RateDefense,                // ���� ����
    Defense,                    // ������ ������ ����� ����
    CriticalDamage,             // ġ��Ÿ ������ 
    FlatCriticalChance,         // ���� ġ��Ÿ Ȯ��
    RateCriticalChance,         // ���� ġ��Ÿ Ȯ��
    CriticalChance,             // ������ ������ ����� ġ��Ÿ
    FlatHp,                     // ���� Hp
    RateHp,                     // ���� Hp
    Hp,                         // ������ ������ ����� Hp
    Healing,                    // ȸ�� ����
    Luck,                       // ���
    MainSkillStack,             // ���� ��ų ���� Ƚ��
    MainSkillCoolDown,          // ���� ��ų ��ٿ� �ӵ� ����
    MainSkillCooltimeRatio,          // ���� ��ų ��Ÿ�� �� ����
    SubSkillStack,              // ���� ��ų ���� Ƚ��
    SubSkillCoolDown,           // ���� ��ų ��ٿ� �ӵ� ����
    SubSkillCooltimeRatio,           // ���� ��ų ��Ÿ�� �� ����
    ProjectileDuration,         // ����ü �����ð�
    ProjectileSpeed,            // ����ü �ӵ�
    HealthRegenRate,            // �ʴ� �����

}

[System.Serializable]
public struct InputAttribute
{
    public Attribute Attribute;
    public float Value;
}