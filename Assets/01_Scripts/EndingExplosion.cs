using System.Collections;
using UnityEngine;

public class EndingExplosion : MonoBehaviour
{
    public Transform[] endingExplostionTransform;
    float goalSize = 137f;
    public void OnTriggerEnter(Collider other)
    {
        other.GetComponent<IAttackable>()?.ReceiveHit(float.MaxValue);
    }
    public void StartExplosion()
    {
        // È°¼ºÈ­
        endingExplostionTransform[0].gameObject.SetActive(true);
        endingExplostionTransform[1].gameObject.SetActive(true);

        StartCoroutine(ExplosionCoroutine());
    }
    IEnumerator ExplosionCoroutine()
    {
        float t = 0;

        while (t < 15f)
        {
            t += Time.deltaTime;

            float size = Mathf.Lerp(0, goalSize, Mathf.InverseLerp(0, 15f, t));

            endingExplostionTransform[0].localScale = new Vector3(size, endingExplostionTransform[0].localScale.y, size);
            endingExplostionTransform[1].localScale = new Vector3(size, size, 1f);

            yield return null;
        }
    }
}
