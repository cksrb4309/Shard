using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public class Test_Break_6 : MonoBehaviour
{
    const string ShaderName = "Shard/TestBreak6GPU";

    static readonly int BaseMapId = Shader.PropertyToID("_BaseMap");
    static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    static readonly int BreakMoveVectorId = Shader.PropertyToID("_BreakMoveVectorWS");
    static readonly int BreakStartTimeId = Shader.PropertyToID("_BreakStartTime");
    static readonly int BreakInvDurationId = Shader.PropertyToID("_BreakInvDuration");
    static readonly int BreakActiveId = Shader.PropertyToID("_BreakActive");

    static Shader sharedShader;
    static Vector3[] sharedLocalPositions;
    static Vector3[] sharedLocalMoveVectors;
    static int sharedShardCount = -1;
    static float sharedRange = -1f;
    static readonly Dictionary<Material, Material> MaterialCache = new Dictionary<Material, Material>();

    [SerializeField] Transform[] shards;
    [SerializeField] Renderer[] shardRenderers;
    [SerializeField] Vector3[] cachedWorldMoveVectors;
    [SerializeField] float range = 1f;
    [SerializeField] float duration = 0.5f;

    readonly MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
    bool isInitialized;

    void Start()
    {
        InitializeIfNeeded();
    }

    [Button]
    public void Setting()
    {
        CacheShards();
        EnsureSharedCache();
        CacheWorldMoveVectors();
        PrepareRenderers();
        ApplyResetState();
        isInitialized = true;
    }

    [Button]
    public void ResetShard()
    {
        InitializeIfNeeded();
        CancelInvoke(nameof(DisableAfterPlayback));
        ApplyResetState();
    }

    public void Break()
    {
        InitializeIfNeeded();
        CancelInvoke(nameof(DisableAfterPlayback));

        float startTime = Time.time;
        float invDuration = duration > 0f ? 1f / duration : 0f;

        for (int i = 0; i < shardRenderers.Length; i++)
        {
            Renderer renderer = shardRenderers[i];
            if (renderer == null)
                continue;

            renderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetVector(BreakMoveVectorId, cachedWorldMoveVectors[i]);
            propertyBlock.SetFloat(BreakStartTimeId, startTime);
            propertyBlock.SetFloat(BreakInvDurationId, invDuration);
            propertyBlock.SetFloat(BreakActiveId, 1f);
            renderer.SetPropertyBlock(propertyBlock);
        }

        if (duration > 0f)
        {
            Invoke(nameof(DisableAfterPlayback), duration);
            return;
        }

        gameObject.SetActive(false);
    }

    void DisableAfterPlayback()
    {
        gameObject.SetActive(false);
    }

    void InitializeIfNeeded()
    {
        if (isInitialized && cachedWorldMoveVectors != null && cachedWorldMoveVectors.Length == transform.childCount)
            return;

        Setting();
    }

    void CacheShards()
    {
        int childCount = transform.childCount;
        shards = new Transform[childCount];
        shardRenderers = new Renderer[childCount];

        for (int i = 0; i < childCount; i++)
        {
            Transform shard = transform.GetChild(i);
            shards[i] = shard;
            shardRenderers[i] = shard.GetComponent<Renderer>();
        }
    }

    void EnsureSharedCache()
    {
        if (shards == null || shards.Length == 0)
            return;

        bool shouldRebuild =
            sharedLocalPositions == null ||
            sharedLocalMoveVectors == null ||
            sharedShardCount != shards.Length ||
            !Mathf.Approximately(sharedRange, range);

        if (!shouldRebuild)
            return;

        sharedShardCount = shards.Length;
        sharedRange = range;
        sharedLocalPositions = new Vector3[sharedShardCount];
        sharedLocalMoveVectors = new Vector3[sharedShardCount];

        for (int i = 0; i < sharedShardCount; i++)
        {
            Vector3 localPosition = shards[i].localPosition;
            float magnitude = localPosition.magnitude;
            Vector3 direction = magnitude > 0f ? localPosition / magnitude : Vector3.zero;
            float moveAmount = magnitude * range;

            sharedLocalPositions[i] = localPosition;
            sharedLocalMoveVectors[i] = direction * moveAmount;
        }
    }

    void CacheWorldMoveVectors()
    {
        cachedWorldMoveVectors = new Vector3[shards.Length];

        for (int i = 0; i < shards.Length; i++)
            cachedWorldMoveVectors[i] = transform.TransformVector(sharedLocalMoveVectors[i]);
    }

    void PrepareRenderers()
    {
        if (sharedShader == null)
            sharedShader = Shader.Find(ShaderName);

        if (sharedShader == null)
        {
            Debug.LogError($"Shader '{ShaderName}' not found.", this);
            return;
        }

        for (int i = 0; i < shardRenderers.Length; i++)
        {
            Renderer renderer = shardRenderers[i];
            if (renderer == null)
                continue;

            Material[] sharedMaterials = renderer.sharedMaterials;

            for (int j = 0; j < sharedMaterials.Length; j++)
                sharedMaterials[j] = GetOrCreateGpuMaterial(sharedMaterials[j]);

            renderer.sharedMaterials = sharedMaterials;
        }
    }

    void ApplyResetState()
    {
        for (int i = 0; i < shardRenderers.Length; i++)
        {
            Renderer renderer = shardRenderers[i];
            shards[i].localPosition = sharedLocalPositions[i];

            if (renderer == null)
                continue;

            renderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetVector(BreakMoveVectorId, cachedWorldMoveVectors[i]);
            propertyBlock.SetFloat(BreakStartTimeId, 0f);
            propertyBlock.SetFloat(BreakInvDurationId, duration > 0f ? 1f / duration : 0f);
            propertyBlock.SetFloat(BreakActiveId, 0f);
            renderer.SetPropertyBlock(propertyBlock);
        }
    }

    Material GetOrCreateGpuMaterial(Material sourceMaterial)
    {
        if (sourceMaterial == null)
            return null;

        if (sourceMaterial.shader == sharedShader)
            return sourceMaterial;

        if (MaterialCache.TryGetValue(sourceMaterial, out Material cachedMaterial))
            return cachedMaterial;

        Material gpuMaterial = new Material(sharedShader)
        {
            name = $"{sourceMaterial.name}_TestBreak6GPU"
        };

        if (sourceMaterial.HasProperty(BaseMapId))
            gpuMaterial.SetTexture(BaseMapId, sourceMaterial.GetTexture(BaseMapId));
        else if (sourceMaterial.HasProperty("_MainTexture"))
            gpuMaterial.SetTexture(BaseMapId, sourceMaterial.GetTexture("_MainTexture"));
        else if (sourceMaterial.HasProperty("_MainTex"))
            gpuMaterial.SetTexture(BaseMapId, sourceMaterial.GetTexture("_MainTex"));

        if (sourceMaterial.HasProperty(BaseColorId))
            gpuMaterial.SetColor(BaseColorId, sourceMaterial.GetColor(BaseColorId));
        else if (sourceMaterial.HasProperty("_Color"))
            gpuMaterial.SetColor(BaseColorId, sourceMaterial.GetColor("_Color"));
        else
            gpuMaterial.SetColor(BaseColorId, Color.white);

        gpuMaterial.enableInstancing = true;
        MaterialCache[sourceMaterial] = gpuMaterial;
        return gpuMaterial;
    }
}
