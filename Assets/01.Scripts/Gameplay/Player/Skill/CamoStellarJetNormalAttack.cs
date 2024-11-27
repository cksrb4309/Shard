using UnityEngine;

public class CamoStellarJetNormalAttack : PlayerSkill
{
    public Transform firePos;

    public float damageMultiplier; // 데미지 계수
    public float baseProjectileDuration; // 기본 투사체 유지시간
    public float baseProjectileSpeed; // 기본 투사체 속도
    public override void UseSkill()
    {
        if (StackCount > 0)
        {
            if (IsAttack())
            {
                StackCount--;

                AttackProjectile attackProjectile = PoolingManager.Instance.GetObject<AttackProjectile>("CamoStellarJetNormalAttack");

                Vector3 position = firePos.position + firePos.right * Random.Range(-0.1f, 0.1f);

                attackProjectile.SetAttackProjectile(
                    attackData:     attackData,
                    damage:         damage * damageMultiplier,
                    speed:          baseProjectileSpeed * projectileSpeed,
                    criticalChance: criticalChance,
                    criticalDamage: criticalDamage,
                    destroyDelay:   baseProjectileDuration * projectileDuration,
                    pos:            position,
                    rotation:       firePos.rotation);
            }
        }
    }
    public float GetAttackSpeed() => cooldownSpeed;
}