using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu(fileName = "SplashDamageAbility", menuName = "Ability/Tear3/SplashDamageAbility")]
public class SplashDamageAbility : Ability, IOnHitDamage
{
    public float startRange;
    public float stackRange;

    float damage = 0.6f;
    float range;
    public override void SetCount(int count)
    {
        base.SetCount(count);

        range = stackRange + stackRange * (count - 1);
    }
    public void OnHit(AttackData attackData, float damage)
    {
        float attackDamage = this.damage * damage;

        List<IAttackable> list = NearestAttackableSelector.GetAttackable(attackData.position, range);

        foreach (IAttackable attackable in list)
        {
            if (attackable.IsAlive()) attackable.ReceiveHit(attackDamage);
        }
    }
    public override ICondition GetCondition() => this;
}
