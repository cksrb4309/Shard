using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : MonoBehaviour, IAttackable
{
    public void ReceiveHit(float damage, bool isCritical = false)
    {
        throw new System.NotImplementedException();
    }
    public MonsterType monsterType;
    public string mobName; // ObjectPool에 리턴하기 위한 몬스터 이름

    public float baseMaxHp; // 몬스터의 기본 최대 체력
    public int reward = 3; // 처치 했을 시 제공하는 영혼의 파편

    public Rigidbody rb; // 이동하는 곳에 사용할 Rigidbody

    public MeshRenderer meshRenderer; // 자신의 Material을 찾기 위한 MeshRenderer
    Material material; // 자신의 Material

    public List<MonsterAttack> monsterAttacks; // 현재 몬스터가 가지고 있는 공격들

    public bool isFastReturn = true;

    Collider cd = null;

    protected float maxHp; // 최대 HP
    protected float hp; // 현재 HP

    private void Awake()
    {
        material = meshRenderer.material; // 몬스터 개인의 재질 가져오기

        cd = GetComponentInChildren<Collider>();
    }
    public void LateUpdate()
    {
        // 몬스터가 서로 충돌하였을 때의 힘을 없앤다
        rb.angularVelocity = Vector3.zero;
        rb.linearVelocity = Vector3.zero;
    }
    public virtual void ReceiveHit(float damage)
    {
        if (!IsAlive()) return; // 살아있지 않다면 리턴

        // 만약 파편 안에 있다면 데미지를 2분의 1로 감소시킨다
        if (inShard) damage *= 0.5f;

        hp -= damage;

        // 마지막으로 데미지를 받은 타겟으로 지정한다
        GameManager.SetLastHit(this); 

        // 데미지 텍스트를 띄운다
        DamageTextController.OnDamageText(transform.position, damage); 

        // 죽어버렸다면 Dead 호출
        if (!IsAlive()) Dead();
    }
    public void Heal(float heal)
    {
        if (IsAlive())
            hp += heal;
    }
    public virtual void Dead()
    {
        // RewardManager에 몬스터의 영혼의 파편(reward)를 전달한다
        RewardManager.MonsterDrop(reward);

        // 죽은 위치를 기록한다 [ 없어도 되는 지 확인해야 함 ! ]
        deadPosition = transform.position;

        cd.enabled = false;

        if (isFastReturn) PoolingManager.Instance.ReturnObject(mobName, gameObject);
    }
    public bool IsAlive()
    {
        if (hp <= 0) return false;

        return true;
    }
    public virtual void Setting(float hpMultiplier, float damageMultiplier)
    {
        #region 공격력 및 체력 설정
        maxHp = baseMaxHp * hpMultiplier;

        hp = maxHp;

        // 가지고 있는 공격에 대해서 공격력 및 체력 수치 설정
        foreach (MonsterAttack monsterAttack in monsterAttacks)
        {
            monsterAttack.Setting(hpMultiplier, damageMultiplier);
        }

        if (cd != null) cd.enabled = true;

        #endregion

        #region 파편 내부에 대한 초기화
        shardAmount = 1;
        inShard = true;
        OutShard();
        #endregion
    }
    #region 위치 확인
    Vector3 deadPosition; // 죽은 위치 (! 없애도 괜찮은지 재검토 해야함 !)
    public Vector3 GetPosition() // 위치 받기
    {
        if (IsAlive())
        {
            return transform.position;
        }
        return deadPosition;
    }
    #endregion
    #region StatusEffect 적용
    Coroutine[] statusEffects = new Coroutine[20] {
        null, null, null, null, null, null, null, null, null, null,
        null, null, null, null, null, null, null, null, null, null
    }; // 현재 적용 중인 상태 효과 Coroutine
    int[] statusEffectCount = new int[20]
    {
        0,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0
    }; // 현재 적용 중인 상태 효과 중첩 수
    public void ReceiveDebuff(StatusEffect effect, float damage = 0)
    {
        if (damage != 0) ReceiveHit(damage);

        MonsterStatusNameToIdMapper.RegisterStatusEffect(effect.effectName);

        int id = MonsterStatusNameToIdMapper.GetId(effect.effectName);

        if (statusEffects[id] != null) StopCoroutine(statusEffects[id]);

        // 중첩 증가 ( 사실 초기화되서 0이라고 가정해도 상관없다
        statusEffectCount[id]++;

        if (effect is StatEffect statEffect)
        {
            Attribute attribute = statEffect.attribute;

            //switch (attribute)
            //{
            //    case Attribute.
            //}
        }
        else if (effect is TickEffect tickEffect)
        {
            if (IsAlive())
                statusEffects[id] = StartCoroutine(TickEffectCoroutine(tickEffect));
        }
    }
    IEnumerator TickEffectCoroutine(TickEffect tickEffect)
    {
        yield return null;
        
        float duration = tickEffect.duration;
        float interval = tickEffect.interval;

        int id = MonsterStatusNameToIdMapper.GetId(tickEffect.effectName);

        if (tickEffect.value > 0)
        {
            float damage = tickEffect.value * statusEffectCount[id] * PlayerAttributes.Get(Attribute.AttackDamage);

            while (true)
            {
                // 틱 데미지
                ReceiveHit(damage);

                yield return new WaitForSeconds(interval);

                duration -= interval;
                if (duration < 0) break;
            }
        }
        else
        {
            float heal = tickEffect.value;

            while (true)
            {
                Heal(heal);

                yield return new WaitForSeconds(interval);

                duration -= interval;

                if (duration < 0) break;
            }
        }

        statusEffects[id] = null;
        statusEffectCount[id] = 0;
    }
    IEnumerator StatEffectCoroutine()
    {
        yield return null;
    }
    #endregion
    #region 파편 적용 함수
    Coroutine shardCoroutine = null; // 파편 코루틴을 적용 중인 코루틴
    float shardAmount = 1; // 재질의 Amount 값
    bool inShard = true; // 파편 안에 있는 지에 대한 여부
    public virtual void InShard()
    {
        if (inShard) return;

        inShard = true;

        if (shardCoroutine != null) StopCoroutine(shardCoroutine);

        shardCoroutine = StartCoroutine(InShardCoroutine());
    }
    public virtual void OutShard()
    {
        if (!inShard) return;

        inShard = false;

        if (shardCoroutine != null) StopCoroutine(shardCoroutine);

        shardCoroutine = StartCoroutine(OutShardCoroutine());
    }
    IEnumerator InShardCoroutine()
    {
        while (shardAmount < 1f)
        {
            yield return null;

            shardAmount += Time.deltaTime * 3f;

            material.SetFloat("_Amount", shardAmount);
        }
        shardAmount = 1f;
        material.SetFloat("_Amount", shardAmount);
    }
    IEnumerator OutShardCoroutine()
    {
        while (shardAmount > 0f)
        {
            yield return null;

            shardAmount -= Time.deltaTime * 3f;

            material.SetFloat("_Amount", shardAmount);
        }
        shardAmount = 0f;
        material.SetFloat("_Amount", shardAmount);
    }

    
    #endregion
}
/*
public enum MonsterType
{
    Front,  // 전열 (진형의 가장 앞쪽에 우선적으로 배치
    Back,   // 후열 (진형의 가장 뒤쪽에 우선적으로 배치
    Mid // 중열 or 특이한 놈들 배치 (진형이 비었을 때 마지막으로 투입됨/라고 생각했지만 그냥 배치함 그런거 없음)
}*/