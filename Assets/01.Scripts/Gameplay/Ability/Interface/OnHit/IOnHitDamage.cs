using UnityEngine;

public interface IOnHitDamage : ICondition
{
    public void OnHit(float damage);
}