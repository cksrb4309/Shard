using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomMonster : MonoBehaviour, IAttackable
{
    // ������ ���� (����, �߿�, �Ŀ�)
    // ���� �̸� (��� ������Ʈ Ǯ���� ���ؼ����� ���)
    public MonsterType monsterType;
    public string mobName;

    // ������ �⺻ �ִ� ü��
    // óġ ���� �� �����ϴ� ��ȥ�� ����
    public float baseMaxHp;
    public int reward = 3;

    public float updateInterval; // ��ǥ ��ġ ���� �ֱ�

    public bool isBossMonster; // ���� ����

    // ������� �⺻ ��ũ��Ʈ ( �̵�, ����, Ÿ�� �˻�, ���� �˻� )
    public MonsterMove moveScript;
    public MonsterTracking traceScript;
    public SearchTarget searchTarget;
    public SearchShard searchShard;

    // ���̵��� ���� �� ������ �ʿ��� ���ݵ�
    public List<MonsterAttack> monsterAttacks;

    public bool isExplosion = false;
    public float explosionRange = 2;
    public LayerMask explosionLayerMask;
    public MeshRenderer explosionRenderer;
    float explosionDamage = 0;


    // ��ü
    Rigidbody rb = null;

    // �ִϸ�����
    Animator animator = null;

    // ����
    Material material = null;

    // �浹ü
    Collider cd = null;

    protected float maxHp; // �ִ� HP
    protected float hp; // ���� HP

    public void LateUpdate()
    {
        if (rb != null)
        {
            // ���Ͱ� ���� �浹�Ͽ��� ���� ���� ���ش�
            rb.angularVelocity = Vector3.zero;
            rb.linearVelocity = Vector3.zero;
        }
    }
    public virtual void ReceiveHit(float damage, bool isCritical = false)
    {
        if (!IsAlive()) return; // ������� �ʴٸ� ����

        // ���� ���� �ȿ� �ִٸ� �������� 2���� 1�� ���ҽ�Ų�� (������ ���� ����ٴ� ��������? �Ƹ���)
        if (inShard) damage *= 0.5f;

        // ü�� ���ҽ�Ŵ !
        hp -= damage;

        // ���������� �������� ���� Ÿ������ �����Ѵ�
        GameManager.SetLastHit(this);

        // ������ �ؽ�Ʈ�� ����
        if (isCritical)
        {
            DamageTextController.OnCriticalDamageText(transform.position, damage);
        }
        else
        {
            DamageTextController.OnDamageText(transform.position, damage);
        }

        // �׾���ȴٸ� Dead ȣ��
        if (!IsAlive()) Dead();
    }
    public void Heal(float heal)
    {
        if (IsAlive()) hp += heal;
    }
    public virtual void Dead()
    {
        // �������� �ʴ� ������ ���
        if (!isExplosion)
        {
            // ���� ����
            foreach (MonsterAttack attack in monsterAttacks)
            {
                attack.StopAttack();
            }

            animator.SetTrigger("Die"); // ��� �ִϸ��̼� ����

            moveScript.StopMove(); // �̵� ����

            cd.enabled = false; // �浹ü ��Ȱ��ȭ

            // ���� �ڷ�ƾ ���� ������ ���ؼ���
            // ���� ��ũ��Ʈ������ Coroutine��
            // ���� �ڷ�ƾ���� ���� �����Ƿ�
            // StopAllCoroutines�� ����Ͽ� �����Ѵ�
            StopAllCoroutines();
        }
        else
        {
            animator.SetTrigger("Explosion"); // ���� �ִϸ��̼� ����

            StartCoroutine(ExplosionCoroutine());
        }
        searchShard.StopSearch(); // ���� �˻� ��Ȱ��ȭ
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
        // RewardManager�� óġ ���� ���� ( ����ν�� ����ġ�� ��ȥ�� ���� ���� )
        RewardManager.MonsterDrop(reward);

        // ���� ������� StageManager ���� óġ �Լ� ����
        if (isBossMonster)
            StageManager.OnKillBoss();

        // ������Ʈ Ǯ ��ȯ
        PoolingManager.Instance.ReturnObject(mobName, gameObject);
    }
    public void SetTarget(Transform target)
    {
        // ���� Ÿ�� ����
        traceScript.SetTarget(target);

        // �̵� ���� ���� ( �� �κп� ���ؼ��� �ڿ��������� �ѹ� �� Ȯ���غ����� )
        // moveScript.SetDir(traceScript.GetDir());

        // Ÿ�� ������ �ʿ��� ��ũ��Ʈ ����
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
        // ������ ���Ͱ� ���ÿ� ���� ���� ��츦 ����ؼ� �����ϰ� �����̸� ���� [0 ~ 0.5]
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
        #region ���ݷ� �� ü�� ����
        maxHp = baseMaxHp * hpMultiplier;

        hp = maxHp;

        // ������ �ִ� ���ݿ� ���ؼ� ���ݷ� �� ü�� ��ġ ����
        foreach (MonsterAttack monsterAttack in monsterAttacks)
        {
            monsterAttack.Setting(hpMultiplier, damageMultiplier);
        }

        // ���� ������ ��ġ ����
        explosionDamage = damageMultiplier * 10 * 250f;
        jumpAttackDamage = damageMultiplier * 10f;
        #endregion

        #region �⺻ ��� �ʱ�ȭ ( ��ü, �ִϸ�����, ����, �浹ü )

        if (rb == null)
        {
            // ��ü
            rb = GetComponentInChildren<Rigidbody>();

            // �ִϸ�����
            animator = GetComponentInChildren<Animator>();

            // ����
            material = GetComponentInChildren<MeshRenderer>().material;

            // �浹ü
            cd = GetComponentInChildren<Collider>();
        }

        // �浹ü Ȱ��ȭ ( �׾��� �� ��Ȱ��ȭ�� �ϱ� ������
        cd.enabled = true;

        // �ִϸ����� Idle ���� Ȱ��ȭ
        animator.SetTrigger("Idle");
        #endregion

        #region ���� ���ο� ���� �ʱ�ȭ
        shardAmount = 1;
        inShard = true;
        OutShard();
        #endregion

        #region �⺻ ��ũ��Ʈ �ʱ�ȭ ( �̵�, ����, �˻� )

        searchTarget.Search(1f); // Search�� ���� �̵� ��ũ��Ʈ�� ���� ����

        moveScript.StartMove(); // �̵� Ȱ��ȭ

        searchShard.StartSearch(); // ���� �˻� Ȱ��ȭ

        StartCoroutine(TrackingCoroutine()); // ���� �ڷ�ƾ ����

        #endregion

        #region ���� Ȱ��ȭ
        foreach (MonsterAttack attack in monsterAttacks)
        {
            attack.StartAttack();
        }
        #endregion

        if (explosionRenderer != null)
            explosionRenderer.material.SetFloat("_Alpha", 0);
    }

    #region ��ġ Ȯ��
    // Vector3 deadPosition; // ���� ��ġ (! ���ֵ� �������� ����� �ؾ��� !)
    public Vector3 GetPosition() // ��ġ �ޱ�
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

    #region StatusEffect ����

    // ���� ���� ���� ���� ȿ�� Coroutine
    Coroutine[] statusEffects = new Coroutine[20] {
        null, null, null, null, null, null, null, null, null, null,
        null, null, null, null, null, null, null, null, null, null
    };

    // ���� ���� ���� ���� ȿ�� ��ø ��
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

        // ��ø ���� ( ��� �ʱ�ȭ�Ǽ� 0�̶�� �����ص� �������
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
                // ƽ ������
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

    #region ���� ���� �Լ�

    // ���� �ڷ�ƾ�� ���� ���� �ڷ�ƾ
    Coroutine shardCoroutine = null;

    // ������ Amount ��
    float shardAmount = 1;

    // ���� �ȿ� �ִ� ���� ���� ����
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
        // ���Ÿ� ���� ����
        animator.SetTrigger("RangedAttack");

        // ���Ÿ� ���� �׼� ����
        this.rangedAttackAction = rangedAttackAction;
    }
    public void RangedAttack()
    {
        Debug.Log(" ���� ���� Ȯ�� ");

        rangedAttackAction?.Invoke();

        rangedAttackAction = null;
    }
    public void Explosion()
    {
        // ���� ����
        foreach (MonsterAttack attack in monsterAttacks)
        {
            attack.StopAttack();
        }

        // ��ƼŬ ����
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

        cd.enabled = false; // �浹ü ��Ȱ��ȭ

        // �̵� ����
        moveScript.StopMove();

        // ���� �ڷ�ƾ ����
        StopAllCoroutines();

        // ��� ó��
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
        
        // ����Ʈ ���
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
    Front,  // ���� (������ ���� ���ʿ� �켱������ ��ġ
    Back,   // �Ŀ� (������ ���� ���ʿ� �켱������ ��ġ
    Mid // �߿� or Ư���� ��� ��ġ (������ ����� �� ���������� ���Ե�/��� ���������� �׳� ��ġ�� �׷��� ����)
}