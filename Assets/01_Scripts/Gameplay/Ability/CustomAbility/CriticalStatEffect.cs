using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CriticalStatEffectAbility", menuName = "Ability/CriticalStatEffectAbility")]
public class CriticalStatEffect : Ability
{
    public StatEffect statEffect;

    public override HashSet<AbilityEventType> SubscribedEvents => AttackData.CriticalDamageEventType;

    public override void SetCount(int count)
    {
        base.SetCount(count);

        statEffect.SetCount(count);
    }
    public override void OnEvent(AttackData attackData)
    {
        PlayerStatus.GetStatusEffect(statEffect);
    }
}
