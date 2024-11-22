using System;
using UnityEngine;

public class CamoStellarJetNormalAttack : PlayerSkill
{
    public Transform[] firePositions; // 발사 위치 및 회전 값

    Transform FirePos
    {
        get
        {
            if (currentIndex > 2) currentIndex = 0;

            return firePositions[currentIndex++];
        }
    }

    int currentIndex = 0;

    public float damageMultiplier; // 데미지 계수
    public float baseProjectileDuration; // 기본 투사체 유지시간
    public float baseProjectileSpeed; // 기본 투사체 속도
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