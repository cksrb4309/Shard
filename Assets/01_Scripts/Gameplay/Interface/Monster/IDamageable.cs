using UnityEngine;

public interface IDamageable
{
    public void TakeDamage(float damage);
    public void TakeDebuff(float damage, StatusEffect statusEffect);
    public bool IsAlive();
}