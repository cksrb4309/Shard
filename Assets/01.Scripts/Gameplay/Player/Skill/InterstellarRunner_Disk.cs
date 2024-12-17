using System.Collections;
using UnityEngine;

public class InterstellarRunner_Disk : MonoBehaviour
{
    public string projectileName;

    public HitEffectName hitEffectName;

    AttackData attackData = null;

    float criticalChance;
    float criticalDamage;
    float damage;
    float speed;

    int attackCount;

    IAttackable target = null; // ���� �Ѿư��� Ÿ��
    public void SetAttackProjectile(AttackData attackData, float damage, float speed, float criticalChance, float criticalDamage, float destroyDelay, int attackCount, Vector3 searchPosition, Vector3 pos, Quaternion rotation)
    {
        this.speed = speed;
        this.attackData = attackData;
        this.damage = damage;
        this.criticalChance = criticalChance;
        this.criticalDamage = criticalDamage;
        this.attackCount = attackCount;

        transform.position = pos;
        transform.rotation = rotation;

        StartCoroutine(TrackingCoroutine(searchPosition));

        StartCoroutine(WaitReturnCoroutine(destroyDelay));
    }
    IEnumerator WaitReturnCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        PoolingManager.Instance.ReturnObject(projectileName, gameObject);
    }
    IEnumerator TrackingCoroutine(Vector3 position)
    {
        // ���� ���콺 Ŀ���� ���� ����� ���� ��
        target = NearestAttackableSelector.GetAttackable(position);

        if (target == null) yield break;

        while (true)
        {
            yield return MoveCoroutine();

            if (target == null) // Ÿ���� �׾ �Ѿư��� �������� �ٽ� Ÿ���� ã�ƾ���
                target = NearestAttackableSelector.GetAttackable(transform.position);
            else // ���� Ÿ���� ����ִٸ� �ش� Ÿ���� �����ϰ� ã�ƾ���
                target = NearestAttackableSelector.GetAttackable(transform.position, 50f, target);
        }
    }
    IEnumerator MoveCoroutine()
    {
        Vector3 distance = Vector3.zero;

        while (true)
        {
            distance = (target.GetPosition() - transform.position);

            if (distance.magnitude < 0.3f) // ��Ҵٰ� ����
            {
                attackCount--; // ���� Ƚ�� 1 ����

                if (attackCount == 0)
                {
                    PoolingManager.Instance.ReturnObject(projectileName, gameObject);
                }

                yield break;
            }
            transform.position += distance.normalized * Time.deltaTime * speed; // �̵�

            yield return null;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        // Physics Layer ������ ���� ���� �� �� �ִ� �͸� �浹��
        IAttackable attackable = other.GetComponent<IAttackable>();

        if (!attackable.IsAlive()) return;

        float damage = this.damage;

        if (LuckManager.Calculate(criticalChance, true))
        {
            damage *= criticalDamage;

            attackData.OnCritical();

            attackable.ReceiveHit(damage, true);

            attackData.OnHit(damage, transform.position, transform.rotation.eulerAngles);

            ParticleManager.Play(transform.position, hitEffectName);

            if (!attackable.IsAlive())
            {
                attackData.OnKill();
            }
        }
        else
        {
            attackable.ReceiveHit(damage);

            attackData.OnHit(damage, transform.position, transform.rotation.eulerAngles);

            ParticleManager.Play(transform.position, hitEffectName);

            if (!attackable.IsAlive())
            {
                attackData.OnKill();
            }
        }
    }
}
