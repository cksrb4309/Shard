using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "ChainAttackAbility", menuName = "Ability/Tear2/ChainAttackAbility")]
public class ChainAttackAbility : TempAbility, IOnHitChanceDamage
{
    public float startRange;
    public float stackRange;

    public float startDamage;
    public float stackDamage;

    public int startMaxCount;
    public int stackMaxCount;

    float range;
    float damage;
    int maxCount;

    const float probability = 0.25f;

    public override ICondition GetCondition() => this;

    public override void SetCount(int count)
    {
        base.SetCount(count);

        range = startRange + stackRange * (count - 1);
        damage = startDamage + stackDamage * (count - 1);
        maxCount = startMaxCount + stackMaxCount * (count - 1);
    }
    public override void Add()
    {
        base.Add();

        range = startRange + stackRange * (count - 1);
        damage = startDamage + stackDamage * (count - 1);
        maxCount = startMaxCount + stackMaxCount * (count - 1);
    }

    public bool OnHitChanceDamage(AttackData attackData, float damage)
    {
        Debug.Log("Ȯ�� ��� ���� " + maxCount.ToString());
        if (!LuckManager.Calculate(probability, true)) return false;

        Debug.Log("Ȯ�� ��� ���");
        GameManager.Instance.StartCoroutine(ChainAttackReadyCoroutine(attackData, damage));

        return true;
    }
    
    IEnumerator ChainAttackReadyCoroutine(AttackData attackData, float damage)
    {
        Debug.Log("ü�� ���� �ǽ�");

        List<IAttackable> list = new List<IAttackable>();

        list.Add(GameManager.GetLastHit());

        for (int i = 0; i < maxCount; i++)
        {
            yield return new WaitForSeconds(0.01f);
            IAttackable nextTarget = null;

            for (int j = 0; j < list.Count; j++)
            {
                // �����ߴ� ��� ���� �ϰ� �˻���
                nextTarget = NearestAttackableSelector.GetAttackable(list[j].GetPosition(), range, list);

                if (nextTarget == null) continue; // ���� ������ ����� ���ٸ� ���� �ݺ����� ����

                list.Add(nextTarget); // ������ ������� �߰��Ѵ�

                // ���� ã�� ���� ã�� ������ ��ġ�� IAttackable�� �Ѱܼ� ������ ����Ѵ�
                GameManager.Instance.StartCoroutine(
                    AttackCoroutine(attackData, damage * this.damage, list[list.Count - 2], nextTarget));

                break;
            }
            if (nextTarget == null) // ���� ã�� ����� ���ٸ�
            {
                break; // ������ �����
            }
        }
    }
    IEnumerator AttackCoroutine(AttackData attackData, float damage, IAttackable current, IAttackable next)
    {
        Debug.Log("���� ����");

        float t = 0;

        Vector3 midPosition = Vector3.Lerp(current.GetPosition(), next.GetPosition(), 0.5f);

        midPosition +=
            (Quaternion.Euler(new Vector3(0, 90 * (Random.value > 0.5f ? 1 : -1), 0)) * 
            (next.GetPosition() - current.GetPosition()).normalized) *
            (Vector3.Magnitude(current.GetPosition()-next.GetPosition()) * 0.3f);


        Transform trail = PoolingManager.Instance.GetObject("LightningTrail").transform;

        while (t < 1f)
        {
            Debug.Log("��ġ ����");

            t += Time.deltaTime * 5;

            trail.position = Vector3.Lerp(
                Vector3.Lerp(current.GetPosition(), midPosition, t),
                Vector3.Lerp(midPosition, next.GetPosition(), t),
                t);

            yield return null;
        }

        if (next.IsAlive())
        {
            if (LuckManager.Calculate(PlayerAttributes.Get(Attribute.CriticalChance), true))
                damage *= PlayerAttributes.Get(Attribute.CriticalDamage);

            attackData.OnHit(damage, next.GetPosition(), (next.GetPosition() - current.GetPosition()).normalized);

            next.ReceiveHit(damage);

            Debug.Log("���� ����");
        }
    }
}