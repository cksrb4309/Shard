using System.Collections.Generic;

public static class AbilityNameToIdMapper
{
    static Dictionary<string, int> abilityNameToId = new Dictionary<string, int>();
    static int abilityId = 0;
    // ������ ���
    public static void RegisterAbility(string abilityName)
    {
        if (!abilityNameToId.ContainsKey(abilityName))
        {
            abilityNameToId[abilityName] = abilityId++;
        }
    }

    // �̸����� ID ��������
    public static int GetId(string abilityName)
    {
        return abilityNameToId.TryGetValue(abilityName, out int id) ? id : -1;
    }

    // ��ü �ʱ�ȭ
    public static void Clear()
    {
        abilityNameToId.Clear();
    }
}

public static class PlayerStatusNameToIdMapper
{
    static Dictionary<string, int> statusNameToId = new Dictionary<string, int>();
    static int statusId = 0;
    // ������ ���
    public static void RegisterStatusEffect(string abilityName)
    {
        if (!statusNameToId.ContainsKey(abilityName))
        {
            statusNameToId[abilityName] = statusId++;
        }
    }

    // �̸����� ID ��������
    public static int GetId(string abilityName)
    {
        return statusNameToId.TryGetValue(abilityName, out int id) ? id : -1;
    }

    // ��ü �ʱ�ȭ
    public static void Clear()
    {
        statusNameToId.Clear();
    }
}
public static class MonsterStatusNameToIdMapper
{
    static Dictionary<string, int> statusNameToId = new Dictionary<string, int>();
    static int statusId = 0;
    // ������ ���
    public static void RegisterStatusEffect(string abilityName)
    {
        if (!statusNameToId.ContainsKey(abilityName))
        {
            statusNameToId[abilityName] = statusId++;
        }
    }

    // �̸����� ID ��������
    public static int GetId(string abilityName)
    {
        return statusNameToId.TryGetValue(abilityName, out int id) ? id : -1;
    }

    // ��ü �ʱ�ȭ
    public static void Clear()
    {
        statusNameToId.Clear();
    }
}