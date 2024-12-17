using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BombAttackProjectile : MonoBehaviour
{
    public string projectileName;
    public Collider cd;

    float rotateSpeed = 30f;

    AttackData attackData = null;

    float criticalChance;
    float criticalDamage;
    float damage;
    float speed;
    float range;

    bool isAttack = false;

    private void OnEnable()
    {
        speed = 0;
        rotateSpeed = 0;
        cd.enabled = false;
    }

    public void SetAttackProjectile(AttackData attackData, float damage, float speed, float criticalChance, float criticalDamage, float destroyDelay, float range, Vector3 pos, Quaternion rotation)
    {
        this.speed = speed;
        this.attackData = attackData;
        this.damage = damage;
        this.criticalChance = criticalChance;
        this.criticalDamage = criticalDamage;
        this.range = range;

        transform.position = pos;
        transform.rotation = rotation;

        cd.enabled = true;

        isAttack = false;

        StartCoroutine(WaitReturnCoroutine(destroyDelay));
    }
    public void SetRotateSpeed(float speed)
    {
        rotateSpeed = speed;
    }
    IEnumerator WaitReturnCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        cd.enabled = false;

        PoolingManager.Instance.ReturnObject(projectileName, gameObject);
    }
    private void FixedUpdate()
    {
        transform.position += transform.forward * Time.fixedDeltaTime * speed;

        // 로컬 Z축 기준으로 회전
        transform.Rotate(0, 0, rotateSpeed * Time.fixedDeltaTime, Space.Self);
    }
    private void OnTriggerEnter(Collider other)
    {
        // Physics Layer 설정을 통해 공격 할 수 있는 것만 충돌함
        IAttackable attackable_1 = other.GetComponent<IAttackable>();

        if (isAttack) return;

        isAttack = true;

        cd.enabled = false;

        float damage = this.damage;

        if (LuckManager.Calculate(criticalChance, true))
        {
            damage *= criticalDamage;

            attackData.OnCritical();
               
            attackable_1.ReceiveHit(damage, true);

            attackData.OnHit(damage, transform.position, transform.rotation.eulerAngles);

            if (!attackable_1.IsAlive())
            {
                attackData.OnKill();
            }

            List<IAttackable> list = NearestAttackableSelector.GetAttackable(transform.position, range);

            foreach (IAttackable attackable_2 in list)
            {
                if (attackable_2.IsAlive())
                {
                    attackable_2.ReceiveHit(damage, true);
                }
            }
        }
        else
        {
            attackable_1.ReceiveHit(damage);

            attackData.OnHit(damage, transform.position, transform.rotation.eulerAngles);

            if (!attackable_1.IsAlive())
            {
                attackData.OnKill();
            }
            List<IAttackable> list = NearestAttackableSelector.GetAttackable(transform.position, range);

            foreach (IAttackable attackable_2 in list)
            {
                if (attackable_2.IsAlive())
                {
                    attackable_2.ReceiveHit(damage);
                }
            }
        }
       

       

        PoolingManager.Instance.ReturnObject(projectileName, gameObject);
    }
}
