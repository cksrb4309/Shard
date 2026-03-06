using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class AttackData
{
    private static readonly ObjectPool<AttackData> pool = new(() => new AttackData());

    public AbilityEventType Type = AbilityEventType.Attack;
    public float Value;   // 데미지나 회복량 등
    private List<int> dispatchedAbilities = new List<int>();

    public Vector3 Position = Vector3.zero;

    public AttackData() { }
    public AttackData(AbilityEventType type, float value, Vector3 position, List<int> dispatchedAbilities)
    {
        Type = type;
        Value = value;
        Position = position;

        this.dispatchedAbilities.Clear();
        this.dispatchedAbilities.AddRange(dispatchedAbilities);
    }
    public void Apply(AttackData attackData)
    {
        Type = attackData.Type;
        Value = attackData.Value;
        Position = attackData.Position;

        dispatchedAbilities.Clear();
        dispatchedAbilities.AddRange(attackData.dispatchedAbilities);
    }
    public void OnHit(Collider[] colliders, Ability ability, bool isRelease = false)
    {
        foreach (Collider collider in colliders)
        {
            IAttackable attackable = collider.GetComponent<IAttackable>();

            if (attackable != null)
            {
                ApplyAttack(attackable, ability, false, false);
            }
        }
        if (isRelease)
        {
            ReleaseAttackData(this);
        }
    }
    public void OnHit(IAttackable attackable, Ability ability, bool isRelease = false)
    {
        ApplyAttack(attackable, ability, true, isRelease);
    }
    private void ApplyAttack(IAttackable attackable, Ability ability, bool useAbilityCondition, bool isRelease = false)
    {
        if (attackable.IsAlive())
        {
            bool isCritical = LuckManager.Calculate(PlayerAttributes.Get(Attribute.CriticalChance), true);
            float damageAmount = isCritical ? PlayerAttributes.Get(Attribute.CriticalDamage) * Value : Value;

            Vector3 pos = attackable.GetPosition();

            attackable.ReceiveHit(damageAmount, isCritical);

            // 중복 방지
            Add(ability);

            // 해당 공격 이벤트 적용에 대해 이벤트 데이터 생성
            AttackData attackData = GetAttackData(
                isCritical ? AbilityEventType.Critical : AbilityEventType.Attack,
                damageAmount,
                pos,
                dispatchedAbilities);

            // 이벤트 전달
            AbilityManager.Instance.Dispatch(attackData);

            if (!attackable.IsAlive())
            {
                // 처치 이벤트로 변경
                attackData.Type = AbilityEventType.Kill;

                // 이벤트 전달
                AbilityManager.Instance.Dispatch(attackData);
            }

            // 이벤트 전달에 사용한 데이터 해제
            ReleaseAttackData(attackData);
        }

        if (isRelease)
        {
            ReleaseAttackData(this);
        }
    }
    public bool CanApplyAttack(Ability ability)
    {
        return !dispatchedAbilities.Contains(AbilityNameToIdMapper.GetId(ability.abilityName));

    }
    public AttackData Add(Ability ability)
    {
        if (ability == null)
        {
            return this;
        }

        dispatchedAbilities.Add(AbilityNameToIdMapper.GetId(ability.abilityName));

        return this;
    }
    public static AttackData GetAttackData(AbilityEventType type, float value, Vector3 position, List<int> dispatchedAbilities)
    {
        AttackData attackData = pool.Get();

        attackData.Type = type;
        attackData.Value = value;
        attackData.Position = position;

        attackData.dispatchedAbilities.Clear();
        attackData.dispatchedAbilities.AddRange(dispatchedAbilities);

        return attackData;
    }
    public static AttackData GetAttackData(AttackData attackData)
    {
        AttackData ret = pool.Get();

        ret.Apply(attackData);

        return ret;
    }
    public static void ReleaseAttackData(AttackData attackData)
    {
        pool.Release(attackData);
    }
    public static readonly HashSet<AbilityEventType> DamageEventType = new()
    {
        AbilityEventType.Attack,
        AbilityEventType.Critical
    };
    public static readonly HashSet<AbilityEventType> CriticalDamageEventType = new()
    {
        AbilityEventType.Critical
    };
    public static readonly HashSet<AbilityEventType> KillEventType = new()
    {
        AbilityEventType.Kill
    };
    public static readonly HashSet<AbilityEventType> PassiveEventType = new()
    {
        AbilityEventType.Passive
    };
}