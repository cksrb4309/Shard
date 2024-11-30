using System.Collections;
using UnityEngine;


[CreateAssetMenu(fileName = "TraceAttackAbility", menuName = "Ability/Tear2/TraceAttackAbility")]
public class TraceAttackAbility : Ability, IOnHitChanceDamage
{
    public float speed;
    public float duration;
    public float spawnRange;

    public float startChance;
    public float stackChance;

    public float startDamage;
    public float stackDamage;

    float chance;
    float damage;
    public bool OnHitChanceDamage(AttackData attackData, float damage)
    {
        if (Random.value > chance) return false;

        TraceAttackProjectile projectile = PoolingManager.Instance.GetObject<TraceAttackProjectile>("TraceProjectile");

        Vector3 pos = GameManager.GetPlayerTransform().position;

        pos += new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized * spawnRange;

        projectile.SetAttackProjectile(
            attackData,
            this.damage * damage,
            PlayerAttributes.Get(Attribute.ProjectileSpeed) * speed,
            PlayerAttributes.Get(Attribute.CriticalChance),
            PlayerAttributes.Get(Attribute.CriticalDamage),
            PlayerAttributes.Get(Attribute.ProjectileDuration) * duration,
            pos);

        return true;
    }
    public override void SetCount(int count)
    {
        this.count = count;

        chance = startChance + stackChance * (count - 1);
        damage = startDamage + stackDamage * (count - 1);
    }
    public override ICondition GetCondition() => this;
}
