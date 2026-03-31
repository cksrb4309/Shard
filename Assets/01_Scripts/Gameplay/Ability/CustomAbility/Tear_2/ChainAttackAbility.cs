using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ChainAttackAbility", menuName = "Ability/Tear2/ChainAttackAbility")]
public class ChainAttackAbility : Ability
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

    public override HashSet<AbilityEventType> SubscribedEvents => AttackData.DamageEventType;

    public override void SetCount(int count)
    {
        base.SetCount(count);

        range = startRange + stackRange * (count - 1);
        damage = startDamage + stackDamage * (count - 1);
        maxCount = startMaxCount + stackMaxCount * (count - 1);
    }
    
    IEnumerator ChainAttackReadyCoroutine(AttackData attackData)
    {
        List<IAttackable> list = new List<IAttackable>();
        HashSet<IAttackable> set = new HashSet<IAttackable>();

        IAttackable lastAttackable = GameManager.GetLastHit();

        list.Add(lastAttackable);
        set.Add(lastAttackable);

        attackData.Value *= damage;

        yield return null;

        for (int i = 0; i < maxCount; i++)
        {
            IAttackable nextTarget = null;

            for (int j = 0; j < list.Count; j++)
            {
                // 공격했던 대상 제외 하고 검색함
                nextTarget = AttackableTargetSelector.GetRandomAliveTargetInRadiusExcludingSet(list[j].GetPosition(), range, set);

                // 만약 공격할 대상이 없다면 넘김
                if (nextTarget == null) continue; 

                // 공격한 대상 추가
                list.Add(nextTarget); 
                set.Add(nextTarget);

                // 현재 찾은 대상과 찾기 시작한 위치의 IAttackable을 넘겨서 공격을 재생한다
                GameManager.Instance.StartCoroutine(
                    AttackCoroutine(AttackData.GetAttackData(attackData), list[list.Count-2], nextTarget));

                // 공격 1회 진행했다면 반복 취소
                break;
            }
            if (nextTarget == null) // 만약 찾은 대상이 없다면
            {
                break; // 체인 공격 취소
            }
        }
        AttackData.ReleaseAttackData(attackData);
    }
    IEnumerator AttackCoroutine(AttackData attackData, IAttackable current, IAttackable next)
    {
        float t = 0;

        Vector3 midPosition = Vector3.Lerp(current.GetPosition(), next.GetPosition(), 0.5f);

        midPosition +=
            (Quaternion.Euler(new Vector3(0, 90 * (Random.value > 0.5f ? 1 : -1), 0)) *
            (next.GetPosition() - current.GetPosition()).normalized) *
            (Vector3.Magnitude(current.GetPosition() - next.GetPosition()) * Random.Range(0.3f, 0.8f));

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
        attackData.OnHit(next, this, true);
    }

    public override void OnEvent(AttackData attackData)
    {
        if (!attackData.CanApplyAttack(this)) return;

        attackData.Add(this);

        if (LuckManager.Calculate(probability, true))
        {
            GameManager.Instance.StartCoroutine(ChainAttackReadyCoroutine(AttackData.GetAttackData(attackData)));
        }
    }
}