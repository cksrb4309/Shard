using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class DamageOnCollision : MonsterAttack
{
    public float delay = 1f;
    Collider cd = null;
    WaitForSeconds waitDelay = null;
    bool isCool = false;
    private void Awake()
    {
        cd = GetComponent<Collider>();

        waitDelay = new WaitForSeconds(delay);
    }
    public override void StartAttack()
    {
        cd.enabled = true;
    }
    public override void StopAttack()
    {
        cd.enabled = false;
    }
    private void OnTriggerStay(Collider other)
    {
        if (!isCool)
        {
            isCool = true;

            IDamageable damageable = other.GetComponentInParent<IDamageable>();

            damageable.TakeDamage(damage);

            StartCoroutine(DelayCoroutine());
        }
    }
    IEnumerator DelayCoroutine()
    {
        yield return waitDelay;

        isCool = false;
    }
}