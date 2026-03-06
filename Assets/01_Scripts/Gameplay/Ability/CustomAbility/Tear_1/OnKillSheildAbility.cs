using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "OnKillSheildAbility", menuName = "Ability/Tear1/OnKillSheildAbility")]
public class OnKillSheildAbility : Ability
{
    public float startValue;
    public float stackValue;

    float value;

    public override HashSet<AbilityEventType> SubscribedEvents => AttackData.KillEventType;

    public override void SetCount(int count)
    {
        base.SetCount(count);

        value = startValue + stackValue * (count - 1);
    }
    public override void OnEvent(AttackData attackData)
    {
        PlayerHealth.GetSheild(value);
    }
}
