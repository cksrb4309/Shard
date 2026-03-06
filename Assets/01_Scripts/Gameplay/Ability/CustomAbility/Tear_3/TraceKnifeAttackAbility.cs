using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TraceKnifeAttackAbility", menuName = "Ability/Tear3/TraceKnifeAttackAbility")]
public class TraceKnifeAttackAbility : Ability
{
    public float speed;
    public float duration;

    public float startDamage;
    public float stackDamage;

    float damage;

    public override HashSet<AbilityEventType> SubscribedEvents => AttackData.KillEventType;

    public override void SetCount(int count)
    {
        base.SetCount(count);

        damage = (startDamage + stackDamage * (count - 1)) * PlayerAttributes.Get(Attribute.AttackDamage);
    }

    public override void OnEvent(AttackData attackData)
    {
        Vector3 randomPos = Vector3.zero;


        for (int i = 0; i < 2; i++)
        {
            randomPos.Set(Random.Range(-0.5f, 0.5f), 0, Random.Range(-0.5f, 0.5f));
            TraceAttackProjectile projectile = PoolingManager.Instance.GetObject<TraceAttackProjectile>("TraceKnifeProjectile");

            attackData = AttackData.GetAttackData(attackData);

            projectile.SetAttackProjectile(
                attackData,
                damage,
                PlayerAttributes.Get(Attribute.ProjectileSpeed) * speed,
                PlayerAttributes.Get(Attribute.ProjectileDuration) * duration,
                attackData.Position + randomPos);
        }
    }
}
