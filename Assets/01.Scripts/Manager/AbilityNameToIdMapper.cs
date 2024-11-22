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