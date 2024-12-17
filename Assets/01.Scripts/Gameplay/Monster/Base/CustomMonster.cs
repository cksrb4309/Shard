using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomMonster : MonoBehaviour, IAttackable
{
    // 몬스터의 종류 (전열, 중열, 후열)
    // 몬스터 이름 (사실 오브젝트 풀링을 위해서만을 사용)
    public MonsterType monsterType;
    public string mobName;

    // 몬스터의 기본 최대 체력
    // 처치 했을 시 제공하는 영혼의 파편
    public float baseMaxHp;
    public int reward = 3;

    public float updateInterval; // 목표 위치 갱신 주기

    public bool isBossMonster; // 보스 여부

    // 갖춰야할 기본 스크립트 ( 이동, 추적, 타겟 검색, 파편 검색 )
    public MonsterMove moveScript;
    public MonsterTracking traceScript;
    public SearchTarget searchTarget;
    public SearchShard searchShard;

    // 난이도에 따른 값 조절이 필요한 공격들
    public List<MonsterAttack> monsterAttacks;

    public bool isExplosion = false;
    public float explosionRange = 2;
    public LayerMask explosionLayerMask;
    public MeshRenderer explosionRenderer;
    float explosionDamage = 0;


    // 강체
    Rigidbody rb = null;

    // 애니메이터
    Animator animator = null;

    // 재질
    Material material = null;

    // 충돌체
    Collider cd = null;

    protected float maxHp; // 최대 HP
    protected float hp; // 현재 HP

    public void LateUpdate()
    {
        if (rb != null)
        {
            // 몬스터가 서로 충돌하였을 때의 힘을 없앤다
            rb.angularVelocity = Vector3.zero;
            rb.linearVelocity = Vector3.zero;
        }
    }
    public virtual void ReceiveHit(float damage, bool isCritical = false)
    {
        if (!IsAlive()) return; // 살아있지 않다면 리턴

        // 만약 파편 안에 있다면 데미지를 2분의 1로 감소시킨다 (파편에서 힘을 얻었다는 느낌으로? 아몰랑)
        if (inShard) damage *= 0.5f;

        // 체력 감소시킴 !
        hp -= damage;

        // 마지막으로 데미지를 받은 타겟으로 지정한다
        GameManager.SetLastHit(this);

        // 데미지 텍스트를 띄운다
        if (isCritical)
        {
            DamageTextController.OnCriticalDamageText(transform.position, damage);
        }
        else
        {
            DamageTextController.OnDamageText(transform.position, damage);
        }

        // 죽어버렸다면 Dead 호출
        if (!IsAlive()) Dead();
    }
    public void Heal(float heal)
    {
        if (IsAlive()) hp += heal;
    }
    public virtual void Dead()
    {
        // 폭발하지 않는 몬스터의 경우
        if (!isExplosion)
        {
            // 공격 중지
            foreach (MonsterAttack attack in monsterAttacks)
            {
                attack.StopAttack();
            }

            animator.SetTrigger("Die"); // 사망 애니메이션 실행

            moveScript.StopMove(); // 이동 중지

            cd.enabled = false; // 충돌체 비활성화

            // 추적 코루틴 실행 중지에 대해서는
            // 현재 스크립트에서는 Coroutine을
            // 추적 코루틴만을 쓰고 있으므로
            // StopAllCoroutines를 사용하여 중지한다
            StopAllCoroutines();
        }
        else
        {
            animator.SetTrigger("Explosion"); // 폭발 애니메이션 실행

            StartCoroutine(ExplosionCoroutine());
        }
        searchShard.StopSearch(); // 파편 검색 비활성화
    }
    IEnumerator ExplosionCoroutine()
    {
        float t = 0;

        while (t < 1f)
        {
            t += Time.deltaTime * 0.3f;

            explosionRenderer.material.SetFloat("_Alpha", t);

            yield return null;
        }
    }

    public void CompleteDie()
    {
        // RewardManager에 처치 보상 전달 ( 현재로써는 경험치와 영혼의 파편 전달 )
        RewardManager.MonsterDrop(reward);

        // 만약 보스라면 StageManager 보스 처치 함수 실행
        if (isBossMonster)
            StageManager.OnKillBoss();

        // 오브젝트 풀 반환
        PoolingManager.Instance.ReturnObject(mobName, gameObject);
    }
    public void SetTarget(Transform target)
    {
        // 추적 타겟 설정
        traceScript.SetTarget(target);

        // 이동 방향 설정 ( 이 부분에 대해서는 자연스러운지 한번 더 확인해볼거임 )
        // moveScript.SetDir(traceScript.GetDir());

        // 타겟 설정이 필요한 스크립트 설정
        //foreach (INeedTarget needTarget in needTargets)
        //{
        //    needTarget.SetTarget(target);
        //}

        foreach (MonsterAttack monsterAttack in monsterAttacks)
        {
            if (monsterAttack.isNeedTarget)
            {
                monsterAttack.SetTarget(target);
            }
        }
    }
    public void StopMove()
    {
        moveScript.StopMove();
    }
    public void StartMove()
    {
        moveScript.StartMove();
    }
    IEnumerator TrackingCoroutine()
    {
        // 스폰된 몬스터가 동시에 고개를 돌릴 경우를 대비해서 랜덤하게 딜레이를 적용 [0 ~ 0.5]
        yield return new WaitForSeconds(UnityEngine.Random.value * 0.5f);

        while (true)
        {
            moveScript.SetDir(traceScript.GetDir());

            yield return new WaitForSeconds(updateInterval);
        }
    }
    public bool IsAlive()
    {
        if (hp <= 0)
            return false;

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

        // 폭발 데미지 수치 조절
        explosionDamage = damageMultiplier * 10 * 250f;
        jumpAttackDamage = damageMultiplier * 10f;
        #endregion

        #region 기본 요소 초기화 ( 강체, 애니메이터, 재질, 충돌체 )

        if (rb == null)
        {
            // 강체
            rb = GetComponentInChildren<Rigidbody>();

            // 애니메이터
            animator = GetComponentInChildren<Animator>();

            // 재질
            material = GetComponentInChildren<MeshRenderer>().material;

            // 충돌체
            cd = GetComponentInChildren<Collider>();
        }

        // 충돌체 활성화 ( 죽었을 때 비활성화를 하기 때문에
        cd.enabled = true;

        // 애니메이터 Idle 상태 활성화
        animator.SetTrigger("Idle");
        #endregion

        #region 파편 내부에 대한 초기화
        shardAmount = 1;
        inShard = true;
        OutShard();
        #endregion

        #region 기본 스크립트 초기화 ( 이동, 추적, 검색 )

        searchTarget.Search(1f); // Search를 통해 이동 스크립트에 방향 설정

        moveScript.StartMove(); // 이동 활성화

        searchShard.StartSearch(); // 파편 검색 활성화

        StartCoroutine(TrackingCoroutine()); // 추적 코루틴 실행

        #endregion

        #region 공격 활성화
        foreach (MonsterAttack attack in monsterAttacks)
        {
            attack.StartAttack();
        }
        #endregion

        if (explosionRenderer != null)
            explosionRenderer.material.SetFloat("_Alpha", 0);
    }

    #region 위치 확인
    // Vector3 deadPosition; // 죽은 위치 (! 없애도 괜찮은지 재검토 해야함 !)
    public Vector3 GetPosition() // 위치 받기
    {
        return transform.position;
        /*
        if (IsAlive())
        {
            return transform.position;
        }
        return deadPosition;*/
    }
    #endregion

    #region StatusEffect 적용

    // 현재 적용 중인 상태 효과 Coroutine
    Coroutine[] statusEffects = new Coroutine[20] {
        null, null, null, null, null, null, null, null, null, null,
        null, null, null, null, null, null, null, null, null, null
    };

    // 현재 적용 중인 상태 효과 중첩 수
    int[] statusEffectCount = new int[20]
    {
        0,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0
    }; 

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
    #endregion

    #region 파편 적용 함수

    // 파편 코루틴을 적용 중인 코루틴
    Coroutine shardCoroutine = null;

    // 재질의 Amount 값
    float shardAmount = 1;

    // 파편 안에 있는 지에 대한 여부
    bool inShard = true; 

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
        shardCoroutine = null;
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
        shardCoroutine = null;
        shardAmount = 0f;
        material.SetFloat("_Amount", shardAmount);
    }

    #endregion

    Action rangedAttackAction = null;
    public void TriggerRangedAttack(Action rangedAttackAction)
    {
        // 원거리 공격 실행
        animator.SetTrigger("RangedAttack");

        // 원거리 공격 액션 저장
        this.rangedAttackAction = rangedAttackAction;
    }
    public void RangedAttack()
    {
        Debug.Log(" 공격 시점 확인 ");

        rangedAttackAction?.Invoke();

        rangedAttackAction = null;
    }
    public void Explosion()
    {
        // 공격 중지
        foreach (MonsterAttack attack in monsterAttacks)
        {
            attack.StopAttack();
        }

        // 파티클 실행
        ParticleManager.Play(transform.position, OtherEffectName.MonsterExplosion, explosionRange);

        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRange, explosionLayerMask);

        foreach (Collider collider in colliders)
        {
            IAttackable attackable = collider.GetComponent<IAttackable>();

            if (attackable != null)
                attackable.ReceiveHit(explosionDamage);

            IDamageable damageable = collider.GetComponent<IDamageable>();

            if (damageable != null)
                damageable.TakeDamage(explosionDamage);
        }

        cd.enabled = false; // 충돌체 비활성화

        // 이동 종료
        moveScript.StopMove();

        // 추적 코루틴 종료
        StopAllCoroutines();

        // 사망 처리
        CompleteDie();
    }
    float jumpAttackRange;
    public void TriggerJumpAttack(float jumpAttackRange, bool isBoss = false)
    {
        this.jumpAttackRange = jumpAttackRange;

        Debug.Log("jumpAttackRange " + jumpAttackRange.ToString());

        if (isBoss)
            animator.SetTrigger("BossJumpAttack");
        else
            animator.SetTrigger("JumpAttack");
    }
    public LayerMask jumpAttackLayerMask;
    float jumpAttackDamage=0;
    public void JumpAttack()
    {
        Debug.Log("JumpAttack " + jumpAttackRange.ToString());
        
        // 이펙트 재생
        ParticleManager.Play(transform.position, OtherEffectName.JumpAttack, jumpAttackRange);

        Collider[] colliders = Physics.OverlapSphere(transform.position, jumpAttackRange, jumpAttackLayerMask);

        foreach (Collider collider in colliders)
        {
            IDamageable damageable = collider.GetComponent<IDamageable>();

            if (damageable != null)
                damageable.TakeDamage(jumpAttackDamage);
        }
    }
}

public enum MonsterType
{
    Front,  // 전열 (진형의 가장 앞쪽에 우선적으로 배치
    Back,   // 후열 (진형의 가장 뒤쪽에 우선적으로 배치
    Mid // 중열 or 특이한 놈들 배치 (진형이 비었을 때 마지막으로 투입됨/라고 생각했지만 그냥 배치함 그런거 없음)
}