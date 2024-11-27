using UnityEngine;

public interface IOnHitDamage : ICondition
{
    public void OnHit(AttackData attackData, float damage);
}