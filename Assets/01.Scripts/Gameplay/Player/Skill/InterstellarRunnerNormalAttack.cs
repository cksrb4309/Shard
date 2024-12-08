using System.Collections;
using UnityEngine;

public class InterstellarRunnerNormalAttack : PlayerSkill
{
    public Transform[][] firePosition;

    public Transform[] firePosition_1;
    public Transform[] firePosition_2;
    public Transform[] firePosition_3;

    public Transform particlePosition;

    public float[] damageMultiplier;

    public float[] baseProjectileSpeed;

    public float[] baseProjectileDuration;

    public ParticleSystem[] chargeParticle;
    public ParticleSystem[] attackParticle;

    bool isCharged = false;

    int attackPower = 0;

    Coroutine chargeCoroutine = null;
    public override void Start()
    {
        baseStackCount = 1;

        base.Start();

        firePosition = new Transform[3][];

        firePosition[0] = new Transform[firePosition_1.Length];
        firePosition[1] = new Transform[firePosition_2.Length];
        firePosition[2] = new Transform[firePosition_3.Length];

        for (int i = 0; i < firePosition_1.Length; i++)
            firePosition[0][i] = firePosition_1[i];
        for (int i = 0; i < firePosition_2.Length; i++)
            firePosition[1][i] = firePosition_2[i];
        for (int i = 0; i < firePosition_3.Length; i++)
            firePosition[2][i] = firePosition_3[i];
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

        for (int i = 0; i < firePosition[attackPower].Length; i++)
        {
            AttackProjectile attackProjectile = PoolingManager.Instance.GetObject<AttackProjectile>("InterstellarRunner_Normal");

            attackProjectile.SetAttackProjectile(
                attackData: attackData,
                damage: damage * damageMultiplier[attackPower],
                speed: baseProjectileSpeed[attackPower] * projectileSpeed,
                criticalChance: criticalChance,
                criticalDamage: criticalDamage,
                destroyDelay: baseProjectileDuration[attackPower] * projectileDuration,
                pos: firePosition[attackPower][i].position,
                rotation: firePosition[attackPower][i].rotation);

            SoundManager.Play("Runner_Normal");
        }

        attackParticle[attackPower].transform.rotation = particlePosition.rotation;
        attackParticle[attackPower].transform.position = particlePosition.position;
        attackParticle[attackPower].Play();
    }
    IEnumerator ChargeCoroutine()
    {
        float t = 0;

        attackPower = -1;

        chargeParticle[attackPower + 1].Play();

        while (t < 1f)
        {
            t += Time.deltaTime * cooldownSpeed;

            yield return null;
        }
        t = 0;

        attackPower = 0;

        chargeParticle[attackPower + 1].Play();

        while (t < 1f)
        {
            t += Time.deltaTime * cooldownSpeed;

            yield return null;
        }
        t = 0;

        attackPower = 1;

        chargeParticle[attackPower + 1].Play();

        while (t < 1f)
        {
            t += Time.deltaTime * cooldownSpeed;

            yield return null;
        }
        t = 0;
        attackPower = 2;

        chargeParticle[attackPower + 1].Play();
    }
}
