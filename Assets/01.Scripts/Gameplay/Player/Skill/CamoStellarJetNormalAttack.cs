using System;
using UnityEngine;

public class CamoStellarJetNormalAttack : PlayerSkill
{
    public Transform[] firePositions; // �߻� ��ġ �� ȸ�� ��

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
    public override void UseSkill()
    {
        if (StackCount > 0)
        {
            if (IsAttack())
            {
                Transform firePos = FirePos;

                StackCount--;

                AttackProjectile attackProjectile = PoolingManager.Instance.GetObject<AttackProjectile>("CamoStellarJetNormalAttack");

                attackProjectile.SetAttackProjectile(
                    attackData: attackData,
                    damage: damage * damageMultiplier,
                    speed: baseProjectileSpeed * projectileSpeed,
                    criticalChance: criticalChance,
                    criticalDamage: criticalDamage,
                    destroyDelay: baseProjectileDuration * projectileDuration,
                    pos: firePos.position,
                    rotation: firePos.rotation);
            }
        }
    }
    public float GetAttackSpeed() => cooldownSpeed;
}