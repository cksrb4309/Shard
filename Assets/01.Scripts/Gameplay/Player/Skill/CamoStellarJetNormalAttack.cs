using UnityEngine;

public class CamoStellarJetNormalAttack : PlayerSkill
{
    public Transform firePos;

    public float damageMultiplier; // ������ ���
    public float baseProjectileDuration; // �⺻ ����ü �����ð�
    public float baseProjectileSpeed; // �⺻ ����ü �ӵ�
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