using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PassiveAbility", menuName = "Ability/PassiveAbility")]
public class PassiveAbility : Ability
{
    public Attribute attribute;
    public float startValue;
    public float stackValue;
    public override HashSet<AbilityEventType> SubscribedEvents => AttackData.PassiveEventType;

    // Passive abilities do not respond to events.
    // This method is intentionally left empty to satisfy the base class contract.
    public override void OnEvent(AttackData attackData)
    {

    }

    public override void SetCount(int count)
    {
        this.count = count;

        PlayerAttributes.Instance.ApplyPassiveAbilityAttribute(attribute, abilityName, startValue + stackValue * (count - 1));
    }
}
