using Sirenix.OdinInspector;
using UnityEngine;
using System.Collections;

public class Test_Break_4 : MonoBehaviour
{
    static readonly Vector3 BaseScale = new Vector3(70.7f, 70.7f, 100f);
    static Vector3[] sharedPositions;
    static Vector3[] sharedMoveVectors;
    static int sharedShardCount = -1;
    static float sharedRange = -1f;

    [SerializeField] Transform[] shards;
    [SerializeField] Vector3[] cachedPositions;
    [SerializeField] Vector3[] cachedMoveVectors;
    [SerializeField] float range = 1f;
    [SerializeField] float duration = 0.5f;

    Coroutine breakCoroutine;

    [Button]
    public void Setting()
    {
        shards = new Transform[transform.childCount];

        // 자식들의 Transform 가져오기
        for (int i = 0; i < shards.Length; i++)
            shards[i] = transform.GetChild(i);

        EnsureSharedCache();

        cachedPositions = sharedPositions;
        cachedMoveVectors = sharedMoveVectors;
    }

    [Button]
    public void ResetShard()
    {
        if (shards == null || shards.Length == 0)
            Setting();

        if (breakCoroutine != null)
        {
            StopCoroutine(breakCoroutine);
            breakCoroutine = null;
        }

        for (int i = 0; i < shards.Length; i++)
        {
            shards[i].localPosition = sharedPositions[i];
            shards[i].localScale = BaseScale;
        }
    }

    public void Break()
    {
        if (shards == null || shards.Length == 0)
            Setting();

        EnsureSharedCache();

        if (breakCoroutine != null)
            StopCoroutine(breakCoroutine);

        if (duration <= 0f)
        {
            for (int i = 0; i < shards.Length; i++)
            {
                shards[i].localPosition = sharedPositions[i] + sharedMoveVectors[i];
                shards[i].localScale = Vector3.zero;
            }

            gameObject.SetActive(false);
            return;
        }

        breakCoroutine = StartCoroutine(TestCoroutine());
    }

    IEnumerator TestCoroutine()
    {
        float invDuration = 1f / duration;
        float elapsed = 0f;
        int count = shards.Length;
        Transform[] localShards = shards;
        Vector3[] startPositions = sharedPositions;
        Vector3[] moveVectors = sharedMoveVectors;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed * invDuration;
            if (progress > 1f) progress = 1f;
            Vector3 currentScale = BaseScale * (1f - progress);

            for (int i = 0; i < count; i++)
            {
                localShards[i].localPosition = startPositions[i] + (moveVectors[i] * progress);
                localShards[i].localScale = currentScale;
            }

            yield return null;
        }

        breakCoroutine = null;
        gameObject.SetActive(false);
    }

    void EnsureSharedCache()
    {
        if (shards == null || shards.Length == 0)
            return;

        bool shouldRebuild =
            sharedPositions == null ||
            sharedMoveVectors == null ||
            sharedShardCount != shards.Length ||
            !Mathf.Approximately(sharedRange, range);

        if (!shouldRebuild)
            return;

        sharedShardCount = shards.Length;
        sharedRange = range;
        sharedPositions = new Vector3[sharedShardCount];
        sharedMoveVectors = new Vector3[sharedShardCount];

        for (int i = 0; i < sharedShardCount; i++)
        {
            Vector3 position = shards[i].localPosition;
            float magnitude = position.magnitude;
            Vector3 direction = magnitude > 0f ? position / magnitude : Vector3.zero;
            float moveAmount = magnitude * range;

            sharedPositions[i] = position;
            sharedMoveVectors[i] = direction * moveAmount;
        }
    }
}
