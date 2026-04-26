using Sirenix.OdinInspector;
using UnityEngine;
using System;
using System.Collections;

public class Test_Break_2 : MonoBehaviour
{
    [SerializeField] Transform[] shards;
    [SerializeField] Vector3[] positions;
    [SerializeField] Vector3[] dirs;
    [SerializeField] float range = 1f;
    [SerializeField] float duration = 0.5f;

    Vector3 scale = new Vector3(70.7f, 70.7f, 100f);

    [Button]
    public void Setting()
    {
        shards = new Transform[transform.childCount];

        // 자식들의 Transform 가져오기
        for (int i = 0; i < shards.Length; i++)
            shards[i] = transform.GetChild(i);

        positions = new Vector3[transform.childCount];
        dirs = new Vector3[transform.childCount];

        for (int i = 0; i < shards.Length; i++)
            positions[i] = shards[i].localPosition;

        for (int i = 0; i < shards.Length; i++)
            dirs[i] = (shards[i].localPosition).normalized;
    }
    [Button]
    public void ResetShard()
    {
        for (int i = 0; i < shards.Length; i++)
        {
            shards[i].localPosition = positions[i];
            shards[i].localScale = scale;
        }
    }
    public void Break()
    {
        StartCoroutine(TestCoroutine());
    }
    IEnumerator TestCoroutine()
    {
        float t = 0;

        while (t < duration)
        {
            t += Time.deltaTime;

            Vector3 s = scale * ((duration - t) / duration);

            for (int i = 0; i < shards.Length; i++)
            {
                shards[i].localPosition += dirs[i] * Time.deltaTime;

                shards[i].localScale = s;
            }

            yield return null;
        }

        gameObject.SetActive(false);
    }
}