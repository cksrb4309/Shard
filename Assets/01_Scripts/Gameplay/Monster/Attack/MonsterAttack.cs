using UnityEngine;

public abstract class MonsterAttack : MonoBehaviour
{
    public float baseDamage;
    public bool isNeedTarget = false;

    protected float damage;
    public virtual void Setting(float hpMultiplier, float damageMultiplier)
    {
        damage = baseDamage * damageMultiplier;
    }
    public virtual bool IsAttack() => false;
    public abstract void StartAttack();
    public abstract void StopAttack();
    public virtual void SetTarget(Transform target) { }
}
