using System.Collections;
using UnityEngine;

public class AttackProjectile : MonoBehaviour
{
    public string projectileName;

    public bool isPiercing = false;

    AttackData attackData = null;

    float criticalChance;
    float criticalDamage;
    float damage;
    float speed;

    bool isAttacked = false;

    public void SetAttackProjectile(AttackData attackData, float damage, float speed, float criticalChance, float criticalDamage, float destroyDelay, Vector3 pos, Quaternion rotation)
    { 
        this.speed = speed;
        this.attackData = attackData;
        this.damage = damage;
        this.criticalChance = criticalChance;
        this.criticalDamage = criticalDamage;

        transform.position = pos;
        transform.rotation = rotation;

        isAttacked = false;

        StartCoroutine(WaitReturnCoroutine(destroyDelay));
    }
    IEnumerator WaitReturnCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        PoolingManager.Instance.ReturnObject(projectileName, gameObject);
    }
    private void FixedUpdate()
    {
        transform.position += transform.forward * Time.fixedDeltaTime * speed;
    }
    private void OnTriggerEnter(Collider other)
    {
        float damage = this.damage;
        if (!isPiercing)
        {
            if (isAttacked) return;

            isAttacked = true;
        }

        // Physics Layer 설정을 통해 공격 할 수 있는 것만 충돌함
        IAttackable attackable = other.GetComponent<IAttackable>();

        if (criticalChance >= Random.value)
        {
            damage *= criticalDamage;

            attackData.OnCritical();
        }
        attackable.ReceiveHit(damage);

        attackData.OnHit(damage, transform.position, transform.rotation.eulerAngles);

        if (!attackable.IsAlive())
        {
            attackData.OnKill();
        }
        if (!isPiercing) PoolingManager.Instance.ReturnObject(projectileName, gameObject);
    }
}
