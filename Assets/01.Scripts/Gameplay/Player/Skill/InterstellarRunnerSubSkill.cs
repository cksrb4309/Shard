using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class InterstellarRunnerSubSkill : PlayerSkill
{
    public InterstellarRunnerNormalAttack normalAttack;

    public Transform firePosition;

    public Transform particlePosition;

    public float[] damageMultiplier;

    public float[] baseProjectileSpeed;

    public float[] baseProjectileDuration;

    public int[] attackCount;

    public ParticleSystem[] chargeParticle;
    public ParticleSystem[] attackParticle;

    public InputActionReference mousePointAction;

    public LayerMask groundLayer;

    bool isCharged = false;

    int attackPower = 0;

    Coroutine chargeCoroutine = null;
    public override void Start()
    {
        base.Start();
    }
    public override void UseSkill()
    {
        if (!isCharged)
        {
            if (StackCount == 0) return;

            // �����ϰ� ���� ���� ��
            chargeCoroutine = StartCoroutine(ChargeCoroutine());

            isCharged = true;
        }
        else if (isCharged)
        {
            StopCoroutine(chargeCoroutine);

            // �����ϰ� ���� ��
            Attack();

            isCharged = false;
        }
    }

    void Attack()
    {
        if (attackPower == -1) return;

        StackCount--;

        InterstellarRunner_Disk attackProjectile = PoolingManager.Instance.GetObject<InterstellarRunner_Disk>("InterstellarRunner_Disk");

        Vector2 mousePosition = mousePointAction.action.ReadValue<Vector2>();

        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        RaycastHit hit;

        Vector3 position = transform.position;

        // ���̰� groundLayer�� �¾��� ���� ����
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
        {
            position = hit.point;
        }

        attackProjectile.SetAttackProjectile(
            attackData: attackData,
            damage: damage * damageMultiplier[attackPower],
            speed: baseProjectileSpeed[attackPower] * projectileSpeed,
            criticalChance: criticalChance,
            criticalDamage: criticalDamage,
            destroyDelay: baseProjectileDuration[attackPower] * projectileDuration,
            attackCount: attackCount[attackPower],
            searchPosition: position,
            pos: firePosition.position,
            rotation: firePosition.rotation);

        SoundManager.Play("Runner_Sub");

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
            t += Time.deltaTime * normalAttack.cooldownSpeed; 

            yield return null;
        }
        t = 0;

        attackPower = 0;

        chargeParticle[attackPower + 1].Play();

        while (t < 1f)
        {
            t += Time.deltaTime * normalAttack.cooldownSpeed;

            yield return null;
        }
        t = 0;

        attackPower = 1;

        chargeParticle[attackPower + 1].Play();

        while (t < 1f)
        {
            t += Time.deltaTime * normalAttack.cooldownSpeed;

            yield return null;
        }
        t = 0;
        attackPower = 2;

        chargeParticle[attackPower + 1].Play();
    }
}
