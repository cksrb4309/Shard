using System.Collections.Generic;

public static class AbilityNameToIdMapper
{
    static Dictionary<string, int> abilityNameToId = new Dictionary<string, int>();
    static int abilityId = 0;
    // 아이템 등록
    public static void RegisterAbility(string abilityName)
    {
        if (!abilityNameToId.ContainsKey(abilityName))
        {
            abilityNameToId[abilityName] = abilityId++;
        }
    }

    // 이름으로 ID 가져오기
    public static int GetId(string abilityName)
    {
        return abilityNameToId.TryGetValue(abilityName, out int id) ? id : -1;
    }

    // 전체 초기화
    public static void Clear()
    {
        abilityNameToId.Clear();
    }
}

public static class PlayerStatusNameToIdMapper
{
    static Dictionary<string, int> statusNameToId = new Dictionary<string, int>();
    static int statusId = 0;
    // 아이템 등록
    public static void RegisterStatusEffect(string abilityName)
    {
        if (!statusNameToId.ContainsKey(abilityName))
        {
            statusNameToId[abilityName] = statusId++;
        }
    }

    // 이름으로 ID 가져오기
    public static int GetId(string abilityName)
    {
        return statusNameToId.TryGetValue(abilityName, out int id) ? id : -1;
    }

    // 전체 초기화
    public static void Clear()
    {
        statusNameToId.Clear();
    }
}
public static class MonsterStatusNameToIdMapper
{
    static Dictionary<string, int> statusNameToId = new Dictionary<string, int>();
    static int statusId = 0;
    // 아이템 등록
    public static void RegisterStatusEffect(string abilityName)
    {
        if (!statusNameToId.ContainsKey(abilityName))
        {
            statusNameToId[abilityName] = statusId++;
        }
    }

    // 이름으로 ID 가져오기
    public static int GetId(string abilityName)
    {
        return statusNameToId.TryGetValue(abilityName, out int id) ? id : -1;
    }

    // 전체 초기화
    public static void Clear()
    {
        statusNameToId.Clear();
    }
}