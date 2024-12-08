using UnityEngine;

public class CamoStellarJetSubSkill : PlayerSkill
{
    public Transform firePos;

    int currentIndex = 0;

    public float damageMultiplier; // ������ ���
    public float baseProjectileDuration; // �⺻ ����ü �����ð�
    public float baseProjectileSpeed; // �⺻ ����ü �ӵ�

    public ParticleSystem attackParticle;

    public override void UseSkill()
    {
        if (StackCount > 0)
        {
            if (IsAttack())
            {
                StackCount--;

                AttackProjectile attackProjectile = PoolingManager.Instance.GetObject<AttackProjectile>("CamoStellarJetSubSkill");

                attackProjectile.SetAttackProjectile(
                    attackData: attackData,
                    damage: damage * damageMultiplier,
                    speed: baseProjectileSpeed * projectileSpeed,
                    criticalChance: criticalChance,
                    criticalDamage: criticalDamage,
                    destroyDelay: baseProjectileDuration * projectileDuration,
                    pos: firePos.position,
                    rotation: firePos.rotation);

                attackParticle.Play();

                SoundManager.Play("Camo_Sub");
            }
        }
    }
}