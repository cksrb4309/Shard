using UnityEngine;

public class TraceAttackAbility : Ability, IOnHitChanceDamage
{
    public override ICondition GetCondition()
    {
        return this;
    }

    public bool OnHit(float damage)
    {
        return true;
    }
}
