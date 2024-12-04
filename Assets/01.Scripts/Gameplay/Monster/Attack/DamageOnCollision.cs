using UnityEngine;

public class DamageOnCollision : MonsterAttack
{
    Collider cd = null;
    private void Awake()
    {
        cd = GetComponent<Collider>();
    }
    public override void StartAttack()
    {
        cd.enabled = true;
    }
    public override void StopAttack()
    {
        cd.enabled = false;
    }
    private void OnTriggerEnter(Collider other)
    {
        IDamageable damageable = other.GetComponentInParent<IDamageable>();

        damageable.TakeDamage(damage);
    }
}