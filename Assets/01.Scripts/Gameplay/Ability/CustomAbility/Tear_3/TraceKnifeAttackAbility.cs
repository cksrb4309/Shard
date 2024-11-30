using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "TraceKnifeAttackAbility", menuName = "Ability/Tear3/TraceKnifeAttackAbility")]
public class TraceKnifeAttackAbility : Ability, IOnKill
{
    public float speed;
    public float duration;

    public float startDamage;
    public float stackDamage;

    float damage;

    public void OnKill(AttackData attackData)
    {
        for (int i = 0; i < 2; i++)
            GameManager.Instance.StartCoroutine(CreateCoroutine(attackData, 0.1f * i));
    }
    IEnumerator CreateCoroutine(AttackData attackData, float delay)
    {
        yield return new WaitForSeconds(delay);

        TraceAttackProjectile projectile = PoolingManager.Instance.GetObject<TraceAttackProjectile>("TraceKnifeProjectile");

        projectile.SetAttackProjectile(
            attackData,
            damage,
            PlayerAttributes.Get(Attribute.ProjectileSpeed) * speed,
            PlayerAttributes.Get(Attribute.CriticalChance),
            PlayerAttributes.Get(Attribute.CriticalDamage),
            PlayerAttributes.Get(Attribute.ProjectileDuration) * duration,
            attackData.position);
    }
    public override void SetCount(int count)
    {
        base.SetCount(count);

        damage = (startDamage + stackDamage * (count - 1)) * PlayerAttributes.Get(Attribute.AttackDamage);
    }
    public override ICondition GetCondition() => this;
}
