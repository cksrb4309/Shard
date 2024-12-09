using System;
using System.Collections;
using UnityEngine;

public abstract class PlayerSkill : MonoBehaviour
{
    public PlayerAttributes attributes;

    public float attackDelay; // ���� ���� ������
    public float baseCooltime; // �⺻ ��ų ���� ��Ÿ��

    public Sprite skillIcon; // ��ų ������

    protected int baseStackCount; // �⺻ ��ų ���� Ƚ��

    protected float currentCooltime; // ���� ��ų ���� ��Ÿ��
    // int currentStackCount; // ���� ��ų ���� Ƚ��

    public SkillSlot skillSlot = null;

    float cooltime = 0; // ���� ���� ���� ��Ÿ��

    int stackCount; // ���� ������ �ִ� ��ų ���� Ƚ��
    protected float projectileDuration = 1; // ����ü ���� �ð�
    protected float projectileSpeed; // ����ü �ӵ�
    [HideInInspector] public float cooldownSpeed = 1; // ��ٿ� �ӵ�
    protected float criticalChance; // ġ��Ÿ Ȯ��
    protected float criticalDamage; // ġ��Ÿ ������
    protected float damage; // ���� ������ ������

    protected bool isAttack = true; // ( �����̷� ���� ) ���� ���� ���� ���� 

    protected AttackData attackData = new AttackData(true);

    bool isCooltimeTextOn = false;

    protected int StackCount
    {
        get
        {
            return stackCount;
        }
        set
        {
            stackCount = value;

            // ���� ������ 0���� ���� ��
            if (stackCount > 0)
            {
                // ���� ���� �� �ִ� ������ 1���� Ŭ ��
                if (baseStackCount > 1)
                {
                    // ���� �ؽ�Ʈ ǥ��
                    skillSlot?.SetCountText(stackCount);

                    // Fill Image �����
                    skillSlot?.DisableFillAmount();

                    // ��Ÿ�� �ؽ�Ʈ ǥ�� Ʈ���� ���� ON
                    isCooltimeTextOn = true;

                    skillSlot?.EnableHandle();

                    // ���� ���� �� �ִ� ������ ��� ä���� ���
                    if (stackCount == baseStackCount)
                    {
                        // ��Ÿ�� �ؽ�Ʈ ����
                        skillSlot?.ClearCooltimeText();

                        skillSlot?.DisableHandle();
                    }
                }
                // ���� ���� �� �ִ� ������ �Ѱ� �ۿ� ���� ��
                else
                {
                    // ���� �ؽ�Ʈ �����ϱ�
                    skillSlot?.ClearCountText();

                    // Fill Image �����
                    skillSlot?.DisableFillAmount();

                    // ��Ÿ�� �ؽ�Ʈ ǥ�� Ʈ���� ���� OFF
                    isCooltimeTextOn = false;

                    skillSlot?.DisableHandle();
                }
            }
            // ���� ������ 0�� ��
            else
            {
                // ���� �ؽ�Ʈ ����
                skillSlot?.ClearCountText();

                // Fill Image ǥ��
                skillSlot?.EnableFillAmount();

                // ��Ÿ�� �ؽ�Ʈ ����
                skillSlot?.ClearCooltimeText();

                // ��Ÿ�� �ؽ�Ʈ ǥ�� Ʈ���� ���� OFF
                isCooltimeTextOn = false;

                skillSlot?.EnableHandle();
            }
        }
    }

    float Cooltime
    {
        get
        {
            return cooltime;
        }
        set
        {
            cooltime = value;

            if (isCooltimeTextOn) // ��Ÿ�� �ؽ�Ʈ�� �ʿ��� ��
            {
                skillSlot?.SetCooltimeText((int)Mathf.Ceil(currentCooltime - cooltime));
            }

            skillSlot?.SetFillAmount(cooltime / currentCooltime);
        }
    }


    private void FixedUpdate() // ���� ���������� ����
    {
        // ���� ������ �ִ� ���� Ƚ���� �ִ� ���� �������� ���� ��
        if (baseStackCount > stackCount)
        {
            // ��Ÿ���� ���൵�� �ø���
            Cooltime += Time.fixedDeltaTime * cooldownSpeed;

            // ���� ��Ÿ���� �� á�� ���
            if (Cooltime >= currentCooltime)
            {
                // ��Ÿ�� ����
                Cooltime -= currentCooltime;

                // ��ų ���� Ƚ���� �ø���
                StackCount++;
            }
        }
    }
    public virtual void Start()
    {
        StackCount = baseStackCount;
    }
    public void Connect(SkillSlot skillSlot)
    {
        this.skillSlot = skillSlot;

        skillSlot.SetSkillIcon(skillIcon);
    }
    public abstract void UseSkill();
    IEnumerator AttackDelayCoroutine()
    {
        yield return new WaitForSeconds(attackDelay);

        isAttack = true;
    }
    protected bool IsAttack()
    {
        if (isAttack)
        {
            isAttack = false;

            StartCoroutine(AttackDelayCoroutine());

            return true;
        }
        else
        {
            return false;
        }
    }
    public void ResetCooltime()
    {
        Cooltime = 0;

        StackCount = baseStackCount;
    }
    #region ������, ����, ��ٿ� �ӵ�, ġ��Ÿ Ȯ�� ���� �Լ�
    public void SetDamage(float damage)
    {
        this.damage = damage;
    }
    public void SetStack(int stackCount)
    {
        baseStackCount = stackCount;

        if (this.stackCount > 0)
        {
            skillSlot?.SetCountText(this.stackCount);
        }

        // ���� ������ ���� ���� ������ ���� ī��Ʈ���� 1 ���� ��
        if (this.stackCount == stackCount - 1)
        {
            // ��Ÿ�� �ؽ�Ʈ�� ǥ���Ѵ�
            isCooltimeTextOn = true;
        }
    }
    public void SetCoolDown(float speed)
    {
        cooldownSpeed = speed;
    }
    public void SetCooltimeRatio(float ratio)
    {
        currentCooltime = baseCooltime - (baseCooltime * ratio);
    }
    public void SetCriticalChance(float criticalChance)
    {
        this.criticalChance = criticalChance;
    }
    public void SetCriticalDamage(float criticalDamage)
    {
        this.criticalDamage = criticalDamage;
    }
    public void SetProjectileDuration(float projectileDuration)
    {
        this.projectileDuration = projectileDuration;
    }
    public void SetProjectileSpeed(float projectileSpeed)
    {
        this.projectileSpeed = projectileSpeed;
    }
    #endregion
    #region �������� Interface�� ���� Action �� Func ����
    public void AddOnKillAbility(IOnKill onKill)
    {
        attackData.onKillAction += onKill.OnKill;
    }
    public void RemoveOnKillItem(IOnKill onKill)
    {
        attackData.onKillAction -= onKill.OnKill;
    }
    public void AddOnHit(IOnHit onHit)
    {
        attackData.onHitAction += onHit.OnHit;
    }
    public void RemoveOnHitItem(IOnHit onHit)
    {
        attackData.onHitAction -= onHit.OnHit;
    }
    public void AddOnCritical(IOnCritical onCritical)
    {
        attackData.onCriticalAction += onCritical.OnCritical;
    }
    public void RemoveCriticalItem(IOnCritical onCritical)
    {
        attackData.onCriticalAction -= onCritical.OnCritical;
    }
    public void AddOnHitDamage(IOnHitDamage onHitDamage)
    {
        attackData.onHitDamageAction += onHitDamage.OnHit;
    }
    public void RemoveOnHitDamageItem(IOnHitDamage onHitDamage)
    {
        attackData.onHitDamageAction -= onHitDamage.OnHit;
    }
    public void AddOnHitChance(IOnHitChance onHitChance)
    {
        attackData.onHitChanceAction += onHitChance.OnHit;
    }
    public void RemoveOnHitChance(IOnHitChance onHitChance)
    {
        attackData.onHitChanceAction -= onHitChance.OnHit;
    }
    public void AddOnHitChanceDamage(IOnHitChanceDamage onHitChanceDamage)
    {
        attackData.onHitDamageChanceAction += onHitChanceDamage.OnHitChanceDamage;
    }
    public void RemoveOnHitChanceDamage(IOnHitChanceDamage onHitChanceDamage)
    {
        attackData.onHitDamageChanceAction -= onHitChanceDamage.OnHitChanceDamage;
    }
    #endregion
}
