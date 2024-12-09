using System.Collections;
using UnityEngine;

public class RaycastHitAttack : MonsterAttack
{
    public CustomMonster monster;

    public string projectileName;

    public float maxDistance;
    public float projectileDuration;
    public float projectileSpeed;
    public float attackDelay;
    public float cooltimeLength;
    public int attackCount;

    public LayerMask layerMask;

    public Transform firePosition;

    bool isCool = false;

    bool isAttack = false;

    Transform target = null;

    RaycastHit hitInfo;

    Ray ray;

    private void FixedUpdate()
    {
        if (isCool) return;

        if (!isAttack) return;

        ray.origin = transform.position;

        ray.direction = firePosition.forward;

        if (Physics.Raycast(ray, out hitInfo, maxDistance, layerMask))
        {
            isCool = true;

            monster.TriggerRangedAttack(Attack);

            target = hitInfo.transform;

            StartCoroutine(CooltimeCoroutine());
        }
    }
    public void Attack()
    {
        StartCoroutine(AttackCoroutine());
    }
    IEnumerator AttackCoroutine()
    {
        for (int i = 0; i < attackCount; i++)
        {
            // ���� Projectile �����ͼ� �����ϱ�
            MonsterProjectile monsterProjectile = PoolingManager.Instance.GetObject<MonsterProjectile>(projectileName);

            // ����ü ����
            monsterProjectile.Setting(damage, projectileSpeed, projectileDuration);

            // ����ü ��ġ
            monsterProjectile.transform.position = firePosition.position;

            Vector3 temp = (target.position - firePosition.position);

            temp.y = 0;

            // ����ü ȸ��
            monsterProjectile.transform.forward = temp.normalized;

            // ���� ������
            yield return new WaitForSeconds(attackDelay);
        }
    }
    IEnumerator CooltimeCoroutine()
    {
        yield return new WaitForSeconds(cooltimeLength);

        isCool = false;
    }

    public override void StartAttack()
    {
        isAttack = true;
    }
    public override void StopAttack()
    {
        isAttack = false;
    }
}