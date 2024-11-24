using UnityEngine;

public interface IOnHitChanceDamage : ICondition
{
    public bool OnHit(float damage);
}