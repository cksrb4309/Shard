using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu(fileName = "SplashDamageAbility", menuName = "Ability/Tear3/SplashDamageAbility")]
public class SplashDamageAbility : Ability
{
    const float Damage = 0.6f;

    public float startRange;
    public float stackRange;

    float range;

    public override HashSet<AbilityEventType> SubscribedEvents => AttackData.DamageEventType;

    public override void SetCount(int count)
    {
        base.SetCount(count);

        range = stackRange + stackRange * (count - 1);
    }
    public override void OnEvent(AttackData attackData)
    {
        float attackDamage = Damage * attackData.Value;

        List<IAttackable> attackables = new List<IAttackable>();

        int count = AttackableTargetSelector.CollectTargetsInRadiusNonAlloc(attackData.Position, range, attackables);

        for (int i = 0; i < count; i++)

            if (attackables[i].IsAlive())
            
                attackables[i].ReceiveHit(attackDamage);
    }
}
