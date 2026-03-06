using System.Collections;
using UnityEngine;

public class HealingOrb : MonoBehaviour
{
    public TrailRenderer trail;

    bool isEnter = false;
    float heal;
    PlayerStatus status;
    public void Setting(float heal)
    {
        StopAllCoroutines();

        isEnter = false;

        this.heal = heal;

        status = null;

        trail.enabled = false;
    }
    public void OnTriggerEnter(Collider other)
    {
        if (isEnter) return;

        trail.enabled = true;

        isEnter = true;

        status = other.GetComponent<PlayerStatus>();

        StartCoroutine(MoveCoroutine());
    }
    IEnumerator MoveCoroutine()
    {
        float t = 0;

        Vector3 st = transform.position;
        Vector3 ed = st + (st - status.transform.position).normalized * 1.5f;

        while (t < 1f)
        {
            t += Time.deltaTime * 3f;

            transform.position = Vector3.Lerp(st, ed, Mathf.Pow(t, 2));

            yield return null;
        }
        transform.position = ed;

        t = 0;
        st = transform.position;

        while (t < 1f)
        {
            ed = status.transform.position;

            t += Time.deltaTime * 3f;

            transform.position = Vector3.Lerp(st, ed, Mathf.Pow(t, 2));

            yield return null;
        }

        trail.enabled = false;

        PlayerStatus.Healing(heal);

        PoolingManager.Instance.ReturnObject("HealingOrb", gameObject);
    }
}
