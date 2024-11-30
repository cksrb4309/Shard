using UnityEngine;

public class DamageOnCollision : MonsterAttack
{
    private void OnTriggerEnter(Collider other)
    {
        IDamageable damageable = other.GetComponentInParent<IDamageable>();

        damageable.TakeDamage(damage);
    }
}