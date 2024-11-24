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
        //passiveAttributes[attribute] = baseAttributes[attribute];
        passiveAttributes[(int)attribute] = baseAttributes[(int)attribute];

        // �ش� �Ӽ��� ���� ���������� ����Ǵ� �������� ���� ��
        //if (passiveStatModifiers.ContainsKey(attribute))
        //{
        //    foreach (float value in passiveStatModifiers[attribute].Values)
        //    {
        //        if (CheckOperationType(attribute))  // �������� ���
        //            passiveAttributes[attribute] *= value;
        //        else  // �տ����� ���
        //            passiveAttributes[attribute] += value;
        //    }
        //}

        foreach (float value in passiveStatModifiers[(int)attribute].Values)
        {
            if (CheckOperationType(attribute))  // �������� ���
                //passiveAttributes[attribute] *= value;
                passiveAttributes[(int)attribute] *= value;
            else  // �տ����� ���
                //passiveAttributes[attribute] += value;
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
            if (CheckOperationType(attribute))  // �������� ���
                //currentAttributes[attribute] *= value;
                currentAttributes[(int)attribute] *= value;
            else  // �տ����� ���
                //currentAttributes[attribute] += value;
                currentAttributes[(int)attribute] += value;
        }

        UpdateAttributes(attribute);
    }
    void UpdateAttributes(Attribute attribute) // ���� ������ ó��
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
        // ��ȭ��Ų �Ӽ��� ���°� ���� ���� ���
        else if (attribute == Attribute.FlatDefense || attribute == Attribute.RateDefense)
        {
            currentAttributes[(int)Attribute.Defense] =
                currentAttributes[(int)Attribute.FlatDefense] * currentAttributes[(int)Attribute.RateDefense];
        }
        // ��ȭ��Ų �Ӽ��� ġ��Ÿ�� ���� ���� ���
        else if (attribute == Attribute.FlatCriticalChance || attribute == Attribute.RateCriticalChance)
        {
            currentAttributes[(int)Attribute.CriticalChance] =
                currentAttributes[(int)Attribute.FlatCriticalChance] * currentAttributes[(int)Attribute.RateCriticalChance];

            Debug.Log("ġȮ �� Ȯ�� : " + GetAttribute(Attribute.CriticalChance).ToString());

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
        // ��ȭ��Ų �Ӽ��� HP�� ���� ���� ���
        else if (attribute == Attribute.FlatHp || attribute == Attribute.RateHp)
        {
            currentAttributes[(int)Attribute.Hp] =
                currentAttributes[(int)Attribute.FlatHp] * currentAttributes[(int)Attribute.RateHp];

            health.SetMaxHp(currentAttributes[(int)Attribute.Hp]);
        }
        // ���� ��ų�� ������ �޶����� ��
        else if (attribute == Attribute.MainSkillStack)
        {
            // ��ų �ִ� ���� ���� ����
            mainSkill.SetStack((int)GetAttribute(Attribute.MainSkillStack));
        }
        // ���� ��ų�� ������ �޶����� ��
        else if (attribute == Attribute.MainSkillStack)
        {
            // ��ų �ִ� ���� ���� ����
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
    bool CheckOperationType(Attribute attribute) // true�� ��� ������ / false�� ��� �տ���
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

        // �нú� �������� Value�� �״�� �����Ѵ�
        //passiveStatModifiers[attribute][itemName] = value;

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
    MainSkillCoolDown,          // ���� ��ų ��ٿ�
    SubSkillStack,              // ���� ��ų ���� Ƚ��
    SubSkillCoolDown,           // ���� ��ų ��ٿ�
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