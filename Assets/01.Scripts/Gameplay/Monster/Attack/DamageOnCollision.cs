using UnityEngine;

public class DamageOnCollision : MonoBehaviour
{
    public float baseDamage;
    float damage;
    private void OnTriggerEnter(Collider other)
    {
        IDamageable damageable = other.GetComponentInParent<IDamageable>();

        damageable.TakeDamage(damage);
    }
    public void SetDamage(float damageMultiplier)
    {
        damage = baseDamage * damageMultiplier;
    }
}