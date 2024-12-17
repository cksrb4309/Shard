using UnityEngine;

[CreateAssetMenu(fileName = "ChanceBombAbility", menuName = "Ability/Tear1/ChanceBombAbility")]
public class ChanceBombAbility : Ability, IOnHitDamage
{
    public LayerMask layerMask;

    public float startProbability;
    public float stackProbability;

    float damage = 1.5f;

    float probability;
    const float range = 1.5f;
    public override void SetCount(int count)
    {
        base.SetCount(count);

        probability = startProbability + stackProbability * (count - 1);
    }

    public void OnHit(AttackData attackData, float damage)
    {
        if (!LuckManager.Calculate(probability, true)) return;

        BombEffect bombEffect = PoolingManager.Instance.GetObject<BombEffect>("BombExplosion");

        bombEffect.Setting(range, attackData, this.damage * damage, DamageApply);
    }
    void DamageApply(AttackData attackData, float damage)
    {
        float tempDamage;

        Collider[] colliders = Physics.OverlapSphere(attackData.position, range, layerMask);

        foreach (Collider collider in colliders)
        {
            if (LuckManager.Calculate(PlayerAttributes.Get(Attribute.CriticalChance), true))
            {
                tempDamage = PlayerAttributes.Get(Attribute.CriticalDamage) * damage;

                attackData.OnCritical();

                IAttackable next = collider.GetComponent<IAttackable>();

                if (next.IsAlive())
                {
                    next.ReceiveHit(damage, true);

                    if (!next.IsAlive()) attackData.OnKill();
                }
            }
            else
            {
                IAttackable next = collider.GetComponent<IAttackable>();

                if (next.IsAlive())
                {
                    next.ReceiveHit(damage);

                    if (!next.IsAlive()) attackData.OnKill();
                }
            }

            
        }
    }
    public override ICondition GetCondition() => this;

}
