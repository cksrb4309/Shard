using Sirenix.OdinInspector;
using UnityEngine;
using System.Collections.Generic;

public class Test_Break_5 : MonoBehaviour
{
    const string BreakClipName = "TestBreak5_LegacyClip";

    static AnimationClip sharedBreakClip;
    static Vector3[] sharedPositions;
    static Vector3[] sharedScales;
    static Vector3[] sharedTargetPositions;
    static string[] sharedPaths;
    static int sharedShardCount = -1;
    static int sharedClipVersion;
    static float sharedRange;
    static float sharedDuration;

    [SerializeField] Transform[] shards;
    [SerializeField] float range = 1f;
    [SerializeField] float duration = 0.5f;

    Animation legacyAnimation;
    int registeredClipVersion = -1;
    bool isInitialized;

    void Start()
    {
        InitializeIfNeeded();
    }

    [Button]
    public void Setting()
    {
        CacheShards();
        EnsureAnimationComponent();
        EnsureSharedClip();
        RegisterSharedClipToAnimation();
        isInitialized = true;
    }

    [Button]
    public void ResetShard()
    {
        InitializeIfNeeded();

        if (legacyAnimation != null)
            legacyAnimation.Stop();

        CancelInvoke(nameof(DisableAfterPlayback));

        for (int i = 0; i < shards.Length; i++)
        {
            shards[i].localPosition = sharedPositions[i];
            shards[i].localScale = sharedScales[i];
        }
    }

    public void Break()
    {
        InitializeIfNeeded();

        legacyAnimation.Play(BreakClipName);
        CancelInvoke(nameof(DisableAfterPlayback));
        Invoke(nameof(DisableAfterPlayback), sharedDuration > 0f ? sharedDuration : duration);
    }

    void DisableAfterPlayback()
    {
        gameObject.SetActive(false);
    }

    void CacheShards()
    {
        shards = new Transform[transform.childCount];

        for (int i = 0; i < shards.Length; i++)
            shards[i] = transform.GetChild(i);
    }

    void EnsureSharedClip()
    {
        if (shards == null || shards.Length == 0 || duration <= 0f)
            return;

        bool shouldRebuild =
            sharedBreakClip == null ||
            sharedShardCount != shards.Length ||
            !Mathf.Approximately(sharedRange, range) ||
            !Mathf.Approximately(sharedDuration, duration);

        if (!shouldRebuild)
            return;

        sharedShardCount = shards.Length;
        sharedRange = range;
        sharedDuration = duration;
        sharedPositions = new Vector3[sharedShardCount];
        sharedScales = new Vector3[sharedShardCount];
        sharedTargetPositions = new Vector3[sharedShardCount];
        sharedPaths = new string[sharedShardCount];

        for (int i = 0; i < sharedShardCount; i++)
        {
            Vector3 position = shards[i].localPosition;
            float magnitude = position.magnitude;
            Vector3 direction = magnitude > 0f ? position / magnitude : Vector3.zero;
            float moveAmount = magnitude * range;

            sharedPositions[i] = position;
            sharedScales[i] = shards[i].localScale;
            sharedTargetPositions[i] = position + (direction * moveAmount);
            sharedPaths[i] = GetRelativePath(transform, shards[i]);
        }

        sharedBreakClip = new AnimationClip
        {
            legacy = true,
            wrapMode = WrapMode.ClampForever,
            frameRate = 60f,
            name = BreakClipName
        };

        for (int i = 0; i < sharedShardCount; i++)
        {
            string path = sharedPaths[i];

            sharedBreakClip.SetCurve(path, typeof(Transform), "m_LocalPosition.x", CreateLinearCurve(sharedPositions[i].x, sharedTargetPositions[i].x));
            sharedBreakClip.SetCurve(path, typeof(Transform), "m_LocalPosition.y", CreateLinearCurve(sharedPositions[i].y, sharedTargetPositions[i].y));
            sharedBreakClip.SetCurve(path, typeof(Transform), "m_LocalPosition.z", CreateLinearCurve(sharedPositions[i].z, sharedTargetPositions[i].z));

            sharedBreakClip.SetCurve(path, typeof(Transform), "m_LocalScale.x", CreateLinearCurve(sharedScales[i].x, 0f));
            sharedBreakClip.SetCurve(path, typeof(Transform), "m_LocalScale.y", CreateLinearCurve(sharedScales[i].y, 0f));
            sharedBreakClip.SetCurve(path, typeof(Transform), "m_LocalScale.z", CreateLinearCurve(sharedScales[i].z, 0f));
        }

        sharedClipVersion++;
    }

    void EnsureAnimationComponent()
    {
        if (legacyAnimation == null)
            legacyAnimation = GetComponent<Animation>();

        if (legacyAnimation == null)
            legacyAnimation = gameObject.AddComponent<Animation>();

        legacyAnimation.playAutomatically = false;
        legacyAnimation.cullingType = AnimationCullingType.AlwaysAnimate;
    }

    void RegisterSharedClipToAnimation()
    {
        if (legacyAnimation == null || sharedBreakClip == null)
            return;

        if (registeredClipVersion == sharedClipVersion)
            return;

        if (legacyAnimation.GetClip(BreakClipName) != null)
            legacyAnimation.RemoveClip(BreakClipName);

        legacyAnimation.AddClip(sharedBreakClip, BreakClipName);
        legacyAnimation.clip = sharedBreakClip;

        AnimationState state = legacyAnimation[BreakClipName];
        if (state != null)
            state.wrapMode = WrapMode.ClampForever;

        registeredClipVersion = sharedClipVersion;
    }

    AnimationCurve CreateLinearCurve(float from, float to)
    {
        return AnimationCurve.Linear(0f, from, sharedDuration, to);
    }

    string GetRelativePath(Transform root, Transform target)
    {
        if (target == root)
            return string.Empty;

        Stack<string> path = new Stack<string>();
        Transform current = target;

        while (current != null && current != root)
        {
            path.Push(current.name);
            current = current.parent;
        }

        return string.Join("/", path.ToArray());
    }

    void InitializeIfNeeded()
    {
        if (isInitialized && registeredClipVersion == sharedClipVersion)
            return;

        if (shards == null || shards.Length == 0)
            CacheShards();

        EnsureAnimationComponent();
        EnsureSharedClip();
        RegisterSharedClipToAnimation();
        isInitialized = true;
    }
}
