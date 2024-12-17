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
    public string mobName; // ObjectPool�� �����ϱ� ���� ���� �̸�

    public float baseMaxHp; // ������ �⺻ �ִ� ü��
    public int reward = 3; // óġ ���� �� �����ϴ� ��ȥ�� ����

    public Rigidbody rb; // �̵��ϴ� ���� ����� Rigidbody

    public MeshRenderer meshRenderer; // �ڽ��� Material�� ã�� ���� MeshRenderer
    Material material; // �ڽ��� Material

    public List<MonsterAttack> monsterAttacks; // ���� ���Ͱ� ������ �ִ� ���ݵ�

    public bool isFastReturn = true;

    Collider cd = null;

    protected float maxHp; // �ִ� HP
    protected float hp; // ���� HP

    private void Awake()
    {
        material = meshRenderer.material; // ���� ������ ���� ��������

        cd = GetComponentInChildren<Collider>();
    }
    public void LateUpdate()
    {
        // ���Ͱ� ���� �浹�Ͽ��� ���� ���� ���ش�
        rb.angularVelocity = Vector3.zero;
        rb.linearVelocity = Vector3.zero;
    }
    public virtual void ReceiveHit(float damage)
    {
        if (!IsAlive()) return; // ������� �ʴٸ� ����

        // ���� ���� �ȿ� �ִٸ� �������� 2���� 1�� ���ҽ�Ų��
        if (inShard) damage *= 0.5f;

        hp -= damage;

        // ���������� �������� ���� Ÿ������ �����Ѵ�
        GameManager.SetLastHit(this); 

        // ������ �ؽ�Ʈ�� ����
        DamageTextController.OnDamageText(transform.position, damage); 

        // �׾���ȴٸ� Dead ȣ��
        if (!IsAlive()) Dead();
    }
    public void Heal(float heal)
    {
        if (IsAlive())
            hp += heal;
    }
    public virtual void Dead()
    {
        // RewardManager�� ������ ��ȥ�� ����(reward)�� �����Ѵ�
        RewardManager.MonsterDrop(reward);

        // ���� ��ġ�� ����Ѵ� [ ��� �Ǵ� �� Ȯ���ؾ� �� ! ]
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
        #region ���ݷ� �� ü�� ����
        maxHp = baseMaxHp * hpMultiplier;

        hp = maxHp;

        // ������ �ִ� ���ݿ� ���ؼ� ���ݷ� �� ü�� ��ġ ����
        foreach (MonsterAttack monsterAttack in monsterAttacks)
        {
            monsterAttack.Setting(hpMultiplier, damageMultiplier);
        }

        if (cd != null) cd.enabled = true;

        #endregion

        #region ���� ���ο� ���� �ʱ�ȭ
        shardAmount = 1;
        inShard = true;
        OutShard();
        #endregion
    }
    #region ��ġ Ȯ��
    Vector3 deadPosition; // ���� ��ġ (! ���ֵ� �������� ����� �ؾ��� !)
    public Vector3 GetPosition() // ��ġ �ޱ�
    {
        if (IsAlive())
        {
            return transform.position;
        }
        return deadPosition;
    }
    #endregion
    #region StatusEffect ����
    Coroutine[] statusEffects = new Coroutine[20] {
        null, null, null, null, null, null, null, null, null, null,
        null, null, null, null, null, null, null, null, null, null
    }; // ���� ���� ���� ���� ȿ�� Coroutine
    int[] statusEffectCount = new int[20]
    {
        0,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0
    }; // ���� ���� ���� ���� ȿ�� ��ø ��
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
        yield return null;
        
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
    IEnumerator StatEffectCoroutine()
    {
        yield return null;
    }
    #endregion
    #region ���� ���� �Լ�
    Coroutine shardCoroutine = null; // ���� �ڷ�ƾ�� ���� ���� �ڷ�ƾ
    float shardAmount = 1; // ������ Amount ��
    bool inShard = true; // ���� �ȿ� �ִ� ���� ���� ����
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
    Front,  // ���� (������ ���� ���ʿ� �켱������ ��ġ
    Back,   // �Ŀ� (������ ���� ���ʿ� �켱������ ��ġ
    Mid // �߿� or Ư���� ��� ��ġ (������ ����� �� ���������� ���Ե�/��� ���������� �׳� ��ġ�� �׷��� ����)
}*/