using UnityEngine;

public class MonsterAttack : MonoBehaviour
{
    public float baseDamage;
    protected float damage;
    public virtual void Setting(float hpMultiplier, float damageMultiplier)
    {
        damage = baseDamage * damageMultiplier;
    }
}
