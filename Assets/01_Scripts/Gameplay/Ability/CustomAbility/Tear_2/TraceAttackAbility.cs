using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "TraceAttackAbility", menuName = "Ability/Tear2/TraceAttackAbility")]
public class TraceAttackAbility : Ability
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

    public override HashSet<AbilityEventType> SubscribedEvents => AttackData.DamageEventType;

    public override void OnEvent(AttackData attackData)
    {
        if (!attackData.CanApplyAttack(this)) return;

        if (Random.value > chance) return;

        attackData.Add(this);

        attackData = AttackData.GetAttackData(attackData);

        TraceAttackProjectile projectile = PoolingManager.Instance.GetObject<TraceAttackProjectile>("TraceProjectile");

        Vector3 pos = GameManager.GetUserTransform().position;
        
        pos += new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized * spawnRange;

        projectile.SetAttackProjectile(
            attackData,
            damage * attackData.Value,
            PlayerAttributes.Get(Attribute.ProjectileSpeed) * speed,
            PlayerAttributes.Get(Attribute.ProjectileDuration) * duration,
            pos);
    }
    public override void SetCount(int count)
    {
        this.count = count;

        chance = startChance + stackChance * (count - 1);
        damage = startDamage + stackDamage * (count - 1);
    }
}
