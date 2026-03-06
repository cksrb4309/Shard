using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ChanceBombAbility", menuName = "Ability/Tear1/ChanceBombAbility")]
public class ChanceBombAbility : Ability
{
    public LayerMask layerMask;

    public float startProbability;
    public float stackProbability;

    float damage = 1.5f;

    float probability;
    const float range = 1.5f;

    public override HashSet<AbilityEventType> SubscribedEvents => AttackData.DamageEventType;

    public override void SetCount(int count)
    {
        base.SetCount(count);

        probability = startProbability + stackProbability * (count - 1);
    }
    public override void OnEvent(AttackData attackData)
    {
        // 이미 작동한 공격 데이터일 경우 반복 방지
        if (!attackData.CanApplyAttack(this)) return;

        if (!LuckManager.Calculate(probability, true)) return;

        // 작동했다면 해당 능력을 공격 데이터에 추가하여 이후 중복 작동 방지
        attackData.Add(this);

        // 공격 데이터 복사하여 폭발 효과에 전달
        attackData = AttackData.GetAttackData(attackData);

        BombEffect bombEffect = PoolingManager.Instance.GetObject<BombEffect>("BombExplosion");

        bombEffect.Setting(range, attackData, damage * attackData.Value, DamageApply);
    }
    void DamageApply(AttackData attackData, float damage)
    {
        List<IAttackable> attackables = new List<IAttackable>();
        int count = AttackableTargetSelector.CollectTargetsInRadiusNonAlloc(attackData.Position, range, attackables);

        for (int i = 0; i < count; i++)
            attackData.OnHit(attackables[i], this, false);

        AttackData.ReleaseAttackData(attackData);
    }
}
