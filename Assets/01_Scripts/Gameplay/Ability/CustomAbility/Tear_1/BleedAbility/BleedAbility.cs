using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BleedAbility", menuName = "Ability/Tear1/BleedAbility")]
public class BleedAbility : Ability
{
    public TickEffect bleedStatusEffect;
    const float probability = 0.1f;
    public override HashSet<AbilityEventType> SubscribedEvents => AttackData.DamageEventType;

    public override void SetCount(int count)
    {
        base.SetCount(count);

        bleedStatusEffect.SetCount(count);
    }
    public override void OnEvent(AttackData attackData)
    {
        if (LuckManager.Calculate(probability, true))
        {
            GameManager.GetLastHit().ReceiveDebuff(bleedStatusEffect);
        }
    }
}
