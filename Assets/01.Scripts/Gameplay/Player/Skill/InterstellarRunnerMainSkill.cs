using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class InterstellarRunnerMainSkill : PlayerSkill
{
    public InterstellarRunnerNormalAttack normalAttack;

    public Transform firePosition;

    public Transform particlePosition;

    public float[] damageMultiplier;

    public float[] baseProjectileSpeed;

    public float[] baseProjectileDuration;

    public float[] rotateSpeed;

    public float[] range;

    public ParticleSystem[] chargeParticle;
    public ParticleSystem[] attackParticle;

    bool isCharged = false;

    int attackPower = 0;

    Coroutine chargeCoroutine = null;

    BombAttackProjectile bombProjectile = null;

    public override void Start()
    {
        base.Start();
    }
    public override void UseSkill()
    {
        if (!isCharged)
        {
            if (StackCount == 0) return;

            // 충전하고 있지 않을 때
            chargeCoroutine = StartCoroutine(ChargeCoroutine());

            isCharged = true;
        }
        else if (isCharged)
        {
            StopCoroutine(chargeCoroutine);

            // 충전하고 있을 때
            Attack();

            isCharged = false;
        }
    }

    void Attack()
    {

        if (attackPower == -1) return;

        StackCount--;

        bombProjectile.transform.SetParent(null);

        bombProjectile.SetAttackProjectile(
            attackData: attackData,
            damage: damage * damageMultiplier[attackPower],
            speed: baseProjectileSpeed[attackPower] * projectileSpeed,
            criticalChance: criticalChance,
            criticalDamage: criticalDamage,
            destroyDelay: baseProjectileDuration[attackPower] * projectileDuration,
            range: range[attackPower],
            pos: firePosition.position,
            rotation: firePosition.rotation);

        attackParticle[attackPower].transform.rotation = particlePosition.rotation;
        attackParticle[attackPower].transform.position = particlePosition.position;
        attackParticle[attackPower].Play();

        SoundManager.Play("Runner_Main");
    }

    IEnumerator ChargeCoroutine()
    {
        float t = 0;

        attackPower = -1;

        chargeParticle[attackPower + 1].Play();

        while (t < 1f)
        {
            t += Time.deltaTime * normalAttack.cooldownSpeed;

            yield return null;
        }

        t = 0;

        attackPower = 0;

        chargeParticle[attackPower + 1].Play();

        bombProjectile = PoolingManager.Instance.GetObject<BombAttackProjectile>("InterstellarRunner_MainSkill");

        bombProjectile.transform.SetParent(transform);

        bombProjectile.transform.position = firePosition.position;
        bombProjectile.transform.rotation = firePosition.rotation;

        bombProjectile.SetRotateSpeed(rotateSpeed[attackPower]);

        while (t < 1f)
        {
            t += Time.deltaTime * normalAttack.cooldownSpeed;

            yield return null;
        }

        t = 0;

        attackPower = 1;

        chargeParticle[attackPower + 1].Play();

        bombProjectile.SetRotateSpeed(rotateSpeed[attackPower]);

        while (t < 1f)
        {
            t += Time.deltaTime * normalAttack.cooldownSpeed;

            yield return null;
        }

        t = 0;

        attackPower = 2;

        bombProjectile.SetRotateSpeed(rotateSpeed[attackPower]);

        chargeParticle[attackPower + 1].Play();
    }
}
