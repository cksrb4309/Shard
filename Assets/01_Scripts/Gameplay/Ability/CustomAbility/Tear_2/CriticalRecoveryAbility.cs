using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "CriticalRecoveryAbility", menuName = "Ability/Tear2/CriticalRecoveryAbility")]
public class CriticalRecoveryAbility : Ability
{
    public float startValue;
    public float stackValue;

    float value;

    public override HashSet<AbilityEventType> SubscribedEvents => AttackData.CriticalDamageEventType;

    public override void SetCount(int count)
    {
        base.SetCount(count);

        value = startValue + stackValue * (count - 1);
    }
    public override void OnEvent(AttackData attackData)
    {
        PlayerStatus.Healing(value);
    }
}
