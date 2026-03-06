using System.Collections.Generic;

public class AbilityManager
{
    public static AbilityManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new AbilityManager();
            }

            return _instance;
        }
    }
    private static AbilityManager _instance = null;

    private readonly Dictionary<AbilityEventType, List<Ability>> abilityEventMap = new();

    Dictionary<int, Ability> abilityDict = new Dictionary<int, Ability>();

    public void RegisterAbility(Ability ability)
    {
        AbilityNameToIdMapper.RegisterAbility(ability.abilityName);

        int relicId = AbilityNameToIdMapper.GetId(ability.abilityName);

        // 최초 획득 시, 또는 유물 누적 획득 시의 처리
        if (!abilityDict.ContainsKey(relicId))
        {
            abilityDict[relicId] = ability;

            ability.SetCount(1);
        }
        else
        {
            abilityDict[relicId].Add();
        }

        // 만약 패시브 아이템일 경우
        if (ability.SubscribedEvents.Contains(AbilityEventType.Passive))
        {
            return; // TODO : Handle passive items with additional effects
        }

        foreach (var type in ability.SubscribedEvents)
        {
            if (!abilityEventMap.ContainsKey(type))
            {
                abilityEventMap[type] = new List<Ability>();
            }
            if (!abilityEventMap[type].Contains(ability))
            {
                abilityEventMap[type].Add(ability);
            }
        }
    }
    
    public void Dispatch(AttackData attackData)
    {
        if (abilityEventMap.TryGetValue(attackData.Type, out var relics))
        {
            foreach (var relic in relics)
            {
                relic.OnEvent(attackData);
            }
        }
    }
    public void Init()
    {
        abilityEventMap.Clear();
        abilityDict.Clear();
    }
}
