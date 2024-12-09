using System;
using System.Collections;
using UnityEngine;

public abstract class PlayerSkill : MonoBehaviour
{
    public PlayerAttributes attributes;

    public float attackDelay; // 공격 사이 딜레이
    public float baseCooltime; // 기본 스킬 충전 쿨타임

    public Sprite skillIcon; // 스킬 아이콘

    protected int baseStackCount; // 기본 스킬 저장 횟수

    protected float currentCooltime; // 현재 스킬 충전 쿨타임
    // int currentStackCount; // 현재 스킬 저장 횟수

    public SkillSlot skillSlot = null;

    float cooltime = 0; // 현재 진행 중인 쿨타임

    int stackCount; // 현재 가지고 있는 스킬 저장 횟수
    protected float projectileDuration = 1; // 투사체 유지 시간
    protected float projectileSpeed; // 투사체 속도
    [HideInInspector] public float cooldownSpeed = 1; // 쿨다운 속도
    protected float criticalChance; // 치명타 확률
    protected float criticalDamage; // 치명타 데미지
    protected float damage; // 현재 유저의 데미지

    protected bool isAttack = true; // ( 딜레이로 인한 ) 현재 공격 가능 여부 

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

            // 현재 스택이 0보다 많을 때
            if (stackCount > 0)
            {
                // 현재 가질 수 있는 스택이 1보다 클 때
                if (baseStackCount > 1)
                {
                    // 스택 텍스트 표시
                    skillSlot?.SetCountText(stackCount);

                    // Fill Image 숨기기
                    skillSlot?.DisableFillAmount();

                    // 쿨타임 텍스트 표시 트리거 변수 ON
                    isCooltimeTextOn = true;

                    skillSlot?.EnableHandle();

                    // 만약 가질 수 있는 스택을 모두 채웠을 경우
                    if (stackCount == baseStackCount)
                    {
                        // 쿨타임 텍스트 정리
                        skillSlot?.ClearCooltimeText();

                        skillSlot?.DisableHandle();
                    }
                }
                // 현재 가질 수 있는 스택이 한개 밖에 없을 때
                else
                {
                    // 스택 텍스트 정리하기
                    skillSlot?.ClearCountText();

                    // Fill Image 숨기기
                    skillSlot?.DisableFillAmount();

                    // 쿨타임 텍스트 표시 트리거 변수 OFF
                    isCooltimeTextOn = false;

                    skillSlot?.DisableHandle();
                }
            }
            // 현재 스택이 0일 때
            else
            {
                // 스택 텍스트 정리
                skillSlot?.ClearCountText();

                // Fill Image 표시
                skillSlot?.EnableFillAmount();

                // 쿨타임 텍스트 정리
                skillSlot?.ClearCooltimeText();

                // 쿨타임 텍스트 표시 트리거 변수 OFF
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

            if (isCooltimeTextOn) // 쿨타임 텍스트가 필요할 때
            {
                skillSlot?.SetCooltimeText((int)Mathf.Ceil(currentCooltime - cooltime));
            }

            skillSlot?.SetFillAmount(cooltime / currentCooltime);
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
            if (Cooltime >= currentCooltime)
            {
                // 쿨타임 적용
                Cooltime -= currentCooltime;

                // 스킬 충전 횟수를 늘린다
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
    #region 데미지, 스택, 쿨다운 속도, 치명타 확률 설정 함수
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

        // 현재 스택이 지금 받은 설정된 스택 카운트보다 1 작을 때
        if (this.stackCount == stackCount - 1)
        {
            // 쿨타임 텍스트를 표시한다
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
    #region 아이템의 Interface에 따라 Action 및 Func 수정
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
