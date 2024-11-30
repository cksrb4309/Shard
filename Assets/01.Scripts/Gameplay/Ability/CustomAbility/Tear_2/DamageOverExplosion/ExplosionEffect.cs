using System;
using System.Collections;
using UnityEngine;

public class ExplosionEffect : MonoBehaviour
{
    public ParticleSystem smoothCircleParticle;
    public MeshRenderer meshRenderer;
    Material material = null;
    public void Setting(float range, AttackData attackData, float damage, Action<AttackData, float> action)
    {
        if (material == null) material = meshRenderer.material;

        transform.position = attackData.position;

        transform.localScale = Vector3.one * range * 2;

        material.SetFloat("_Progress", 0);
        material.SetFloat("_Alpha", 1);

        StartCoroutine(EffectCoroutine(damage, attackData, action));
    }
    IEnumerator EffectCoroutine(float damage, AttackData attackData, Action<AttackData, float> action)
    {
        float progress = 0;
        float alpha = 1;

        while (progress < 1f)
        {
            progress += Time.deltaTime * 5f;

            material.SetFloat("_Progress", progress);

            yield return null;
        }

        smoothCircleParticle.Play();

        action(attackData, damage);

        while (alpha > 0)
        {
            alpha -= Time.deltaTime * 2f;

            material.SetFloat("_Alpha", alpha);

            yield return null;
        }
    }
}
