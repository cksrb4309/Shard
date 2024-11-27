using UnityEngine;

public interface IOnHitChanceDamage : ICondition
{
    public bool OnHitChanceDamage(AttackData attackData, float damage);
}