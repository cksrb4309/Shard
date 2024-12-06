using System.Collections;
using UnityEngine;

public class CamoStellarJetMainSkill : PlayerSkill
{
    public PlayerInputAndMove playerInputAndMove;
    public Transform[] firePositions; // �߻� ��ġ �� ȸ�� ��
    public CamoStellarJetNormalAttack normalAttack;

    public ParticleSystem attackParticle;
    Transform FirePos
    {
        get
        {
            if (currentIndex > 2) currentIndex = 0;

            return firePositions[currentIndex++];
        }
    }

    int currentIndex = 0;

    public float damageMultiplier; // ������ ���
    public float baseProjectileDuration; // �⺻ ����ü �����ð�
    public float baseProjectileSpeed; // �⺻ ����ü �ӵ�

    bool isShooting = false;
    public override void UseSkill()
    {
        if (StackCount > 0)
        {
            if (isShooting) return;

            if (IsAttack())
            {
                StackCount--;

                StartCoroutine(ShootCoroutine());
            }
        }
    }
    IEnumerator ShootCoroutine()
    {
        float t = 0;

        float attackDelay = 0.1f / normalAttack.GetAttackSpeed();

        playerInputAndMove.StopRotation();

        isShooting = true;

        for (; t < 0.75f;)
        {
            Transform firePos = FirePos;

            AttackProjectile attackProjectile = PoolingManager.Instance.GetObject<AttackProjectile>("CamoStellarJetMainSkill");
               
            attackProjectile.SetAttackProjectile(
                attackData: attackData,
                damage: damage * damageMultiplier,
                speed: baseProjectileSpeed * projectileSpeed,
                criticalChance: criticalChance,
                criticalDamage: criticalDamage,
                destroyDelay: baseProjectileDuration * projectileDuration,
                pos: firePos.position,
                rotation: firePos.rotation);

            t += attackDelay;

            attackParticle.Play();

            yield return new WaitForSeconds(attackDelay);
        }

        isShooting = false;

        playerInputAndMove.PlayRotation();

        currentIndex = 0;
    }
}