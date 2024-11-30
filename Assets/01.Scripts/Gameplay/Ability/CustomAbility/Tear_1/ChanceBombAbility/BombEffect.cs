using System.Collections;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class BombEffect : MonoBehaviour
{
    public MeshRenderer meshRenderer;

    public ParticleSystem particle_1;
    public ParticleSystem particle_2;

    float startSpeed = 1.5f;
    float endSpeed = 3f;

    Vector3 dir;

    public void Setting(float range, AttackData attackData, float damage, System.Action<AttackData, float> action)
    {
        meshRenderer.enabled = true;

        dir = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;

        transform.position = attackData.position;

        StartCoroutine(EffectCoroutine(damage, attackData, action));
    }
    IEnumerator EffectCoroutine(float damage, AttackData attackData, System.Action<AttackData, float> action)
    {
        float t = 0;
        float term = 0.5f;
        float currentTerm = term;

        while (t < 1f)
        {
            currentTerm -= Mathf.Lerp(startSpeed, endSpeed, t) * Time.deltaTime;

            if (currentTerm < 0)
            {
                particle_1.Play();
                currentTerm = term;
            }

            t += Time.deltaTime * 0.5f;

            yield return null;
        }
        particle_2.Play();

        action(attackData, damage);

        meshRenderer.enabled = false;

        yield return new WaitForSeconds(0.5f);

        PoolingManager.Instance.ReturnObject("BombEffect", gameObject);
    }
    private void FixedUpdate()
    {
        transform.Rotate(dir *  Time.deltaTime * 90f);
    }
}
