using System.Collections;
using UnityEngine;

public class SmallInterStellarRunner : Teammate
{
    public Transform[] firePosition;

    public float chargeLength = 2f;

    float currentCooltime = 0;

    int attackPower = 0;

    public ParticleSystem[] chargeParticles;

    protected override IEnumerator AttackCoroutine()
    {
        StartCoroutine(AttackMoveCoroutine());

        while (attackable.IsAlive())
        {
            currentCooltime += Time.deltaTime * attackSpeed;

            if (attackPower == 0)
            {
                chargeParticles[attackPower].Play();

                attackPower = 1;
            }

            if (attackPower == 1 && currentCooltime > (chargeLength * 0.33f))
            {
                chargeParticles[attackPower].Play();

                attackPower = 2;
            }
            if (attackPower == 2 && currentCooltime > (chargeLength * 0.66f))
            {
                chargeParticles[attackPower].Play();

                attackPower = 3;
            }
            if (attackPower == 3 && currentCooltime > chargeLength)
            {
                chargeParticles[attackPower].Play();
            }

            if (currentCooltime > chargeLength)
            {
                currentCooltime -= chargeLength;

                attackPower = 0;

                for (int i = 0; i < firePosition.Length; i++)
                {
                    SmallCamoStellarJetProjectile attackProjectile = PoolingManager.Instance.GetObject<SmallCamoStellarJetProjectile>("SmallInterstellarRunnerProjectile");

                    attackProjectile.Setting(attackDamage, firePosition[i].rotation);

                    attackProjectile.transform.position = firePosition[i].position;

                    attackProjectile.transform.rotation = firePosition[i].rotation;

                    SoundManager.Play("Runner_Normal");
                }
                yield return null;
            }
            yield return null;
        }
        Patrol(); // 순찰로 전환
    }
}
