using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "RecoveryHitAbility", menuName = "Ability/Tear2/RecoveryHitAbility")]
public class RecoveryHitAbility : Ability
{
    int healingValue = 0;

    public override HashSet<AbilityEventType> SubscribedEvents => AttackData.DamageEventType;

    public override void SetCount(int count)
    {
        base.SetCount(count);

        healingValue = count;
    }
    public override void OnEvent(AttackData attackData)
    {
        PlayerStatus.Healing((float)healingValue);
    }
}
