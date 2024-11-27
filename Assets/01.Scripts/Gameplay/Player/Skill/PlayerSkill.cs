using System;
using System.Collections;
using UnityEngine;

public abstract class PlayerSkill : MonoBehaviour
{
    public PlayerAttributes attributes;

    public float attackDelay; // 공격 사이 딜레이
    public float baseCooltime; // 기본 스킬 충전 쿨타임

    int baseStackCount; // 기본 스킬 저장 횟수

    float currentCooltime; // 현재 스킬 충전 쿨타임
    // int currentStackCount; // 현재 스킬 저장 횟수

    public SkillSlot skillSlot = null;

    float cooltime = 0; // 현재 진행 중인 쿨타임

    int stackCount; // 현재 가지고 있는 스킬 저장 횟수
    protected float projectileDuration = 1; // 투사체 유지 시간
    protected float projectileSpeed; // 투사체 속도
    protected float cooldownSpeed = 1; // 쿨다운 속도
    protected float criticalChance; // 치명타 확률
    protected float criticalDamage; // 치명타 데미지
    protected float damage; // 현재 유저의 데미지

    protected bool isAttack = true; // ( 딜레이로 인한 ) 현재 공격 가능 여부 

    protected AttackData attackData = new AttackData(true);

    protected int StackCount
    {
        get
        {
            return stackCount;
        }
        set
        {
            stackCount = value;

            if (baseStackCount > 1) skillSlot?.SetCountText(stackCount);
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

            if (stackCount == 0)
                skillSlot?.SetFillAmount(1 - cooltime / currentCooltime);
            else
                skillSlot?.SetFillAmount(0);
        }
    }


    private void FixedUpdate() // 고정 프레임으로 갱신
    {
        // 현재 가지고 있는 충전 횟수가 최대 충전 개수보다 작을 때
        if (baseStackCount > stackCount)
        {
            // 쿨타임의 진행도를 올린다
            Cooltime += Time.fixedDeltaTime * cooldownSpeed;

            // 만약 쿨타임이 다 찼을 경우
            if (currentCooltime <= Cooltime)
            {
                // 스킬 충전 횟수를 늘린다
                stackCount++;

                // 쿨타임 적용
                Cooltime -= currentCooltime;
            }
        }
    }
    private void Start()
    {
        StackCount = baseStackCount;
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
    #region 데미지, 스택, 쿨다운 속도, 치명타 확률 설정 함수
    public void SetDamage(float damage)
    {
        this.damage = damage;
    }
    public void SetStack(int stackCount)
    {
        baseStackCount = stackCount;
    }
    public void SetCoolDown(float speed)
    {
        cooldownSpeed = speed;

        // 임시 !
        currentCooltime = baseCooltime;
    }
    public void SetCooltimeDecrease(float cooldownTime)
    {
        currentCooltime = baseCooltime -= cooldownTime;
    }
    public void SetCriticalChance(float criticalChance)
    {
        Debug.Log("크확 설정 " + criticalChance.ToString());
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
    #region 아이템의 Interface에 따라 Action 및 Func 수정
    public void AddOnKillAbility(IOnKill onKill)
    {
        attackData.onKillAction += onKill.OnKill;
    }
    public void RemoveOnKillItem(IOnKill onKill)
    {
        attackData.onKillAction -= onKill.OnKill;
    }
    public void AddOnHitItem(IOnHit onHit)
    {
        attackData.onHitAction += onHit.OnHit;
    }
    public void RemoveOnHitItem(IOnHit onHit)
    {
        attackData.onHitAction -= onHit.OnHit;
    }
    public void AddOnCriticalItem(IOnCritical onCritical)
    {
        attackData.onCriticalAction += onCritical.OnCritical;
    }
    public void RemoveCriticalItem(IOnCritical onCritical)
    {
        attackData.onCriticalAction -= onCritical.OnCritical;
    }
    public void AddOnHitDamageItem(IOnHitDamage onHitDamage)
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
