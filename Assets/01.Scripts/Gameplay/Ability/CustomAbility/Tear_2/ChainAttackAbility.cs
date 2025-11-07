using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "ChainAttackAbility", menuName = "Ability/Tear2/ChainAttackAbility")]
public class ChainAttackAbility : Ability, IOnHitChanceDamage
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

    public bool OnHitChanceDamage(AttackData attackData, float damage)
    {
        if (!LuckManager.Calculate(probability, true)) return false;

        GameManager.Instance.StartCoroutine(ChainAttackReadyCoroutine(attackData, damage));

        return true;
    }
    
    IEnumerator ChainAttackReadyCoroutine(AttackData attackData, float damage)
    {
        List<IAttackable> list = new List<IAttackable>();

        list.Add(GameManager.GetLastHit());

        yield return null;

        float startTime = Time.realtimeSinceStartup;

        for (int i = 0; i < maxCount; i++)
        {
            IAttackable nextTarget = null;

            for (int j = 0; j < list.Count; j++)
            {
                // 공격했던 대상 제외 하고 검색함
                nextTarget = NearestAttackableSelector.GetAttackable(list[j].GetPosition(), range, list);

                if (nextTarget == null) continue; // 만약 공격할 대상이 없다면 다음 반복으로 간다

                list.Add(nextTarget); // 공격한 대상으로 추가한다

                // 현재 찾은 대상과 찾기 시작한 위치의 IAttackable을 넘겨서 공격을 재생한다
                GameManager.Instance.StartCoroutine(
                    AttackCoroutine(attackData, damage * this.damage, list[list.Count-2], nextTarget));

                break;
            }
            if (nextTarget == null) // 만약 찾은 대상이 없다면
            {
                break; // 공격을 멈춘다
            }
        }
    }
    IEnumerator AttackCoroutine(AttackData attackData, float damage, IAttackable current, IAttackable next)
    {
        float t = 0;

        Vector3 midPosition = Vector3.Lerp(current.GetPosition(), next.GetPosition(), 0.5f);

        midPosition +=
            (Quaternion.Euler(new Vector3(0, 90 * (Random.value > 0.5f ? 1 : -1), 0)) * 
            (next.GetPosition() - current.GetPosition()).normalized) *
            (Vector3.Magnitude(current.GetPosition()-next.GetPosition()) * Random.Range(0.3f, 0.8f));

        Transform trail = PoolingManager.Instance.GetObject("LightningTrail").transform;

        while (t < 1f)
        {
            t += Time.deltaTime * 5f;

            trail.position = Vector3.Lerp(
                Vector3.Lerp(current.GetPosition(), midPosition, t),
                Vector3.Lerp(midPosition, next.GetPosition(), t),
                t);

            yield return null;
        }

        if (next.IsAlive())
        {
            if (LuckManager.Calculate(PlayerAttributes.Get(Attribute.CriticalChance), true))
            {
                damage *= PlayerAttributes.Get(Attribute.CriticalDamage);

                attackData.OnCritical();

                if (next.IsAlive())
                {
                    next.ReceiveHit(damage, true);

                    attackData.OnHit(damage, next.GetPosition(), (next.GetPosition() - current.GetPosition()).normalized);

                    if (!next.IsAlive()) attackData.OnKill();
                }
            }
            else
            {
                if (next.IsAlive())
                {
                    next.ReceiveHit(damage);

                    attackData.OnHit(damage, next.GetPosition(), (next.GetPosition() - current.GetPosition()).normalized);

                    if (!next.IsAlive()) attackData.OnKill();
                }
            }
        }
    }
}