using System.Collections;
using UnityEngine;

public class SmallCamoStellarJet : Teammate
{
    public Transform firePosition;
    public float cooltime;
    float currentCooltime = 0;

    protected override IEnumerator AttackCoroutine()
    {
        StartCoroutine(AttackMoveCoroutine());

        while (attackable.IsAlive())
        {
            currentCooltime += Time.deltaTime * attackSpeed;

            if (currentCooltime > cooltime)
            {
                currentCooltime -= cooltime;

                SmallCamoStellarJetProjectile projectile =
                    PoolingManager.Instance.GetObject<SmallCamoStellarJetProjectile>("SmallCamoStellarJetProjectile");

                projectile.transform.position = firePosition.position;

                projectile.transform.rotation = firePosition.rotation;

                projectile.Setting(attackDamage, firePosition.rotation);

                yield return null;
            }
            yield return null;
        }

        Patrol();
    }
}