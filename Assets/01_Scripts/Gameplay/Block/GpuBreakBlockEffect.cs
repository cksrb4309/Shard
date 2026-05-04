using Sirenix.OdinInspector;
using UnityEngine;

public class GpuBreakBlockEffect : MonoBehaviour
{
    const string ShaderName = "Shard/BreakBaseBlockGPU";
    const float BreakActiveValue = 1f;

    static readonly int MainTextureId = Shader.PropertyToID("_MainTexture");
    static readonly int BreakParamsId = Shader.PropertyToID("_BreakParams");

    static Shader sharedShader;
    static Material sharedGpuMaterial;
    static Texture sharedMainTexture;
    static bool loggedTextureMismatch;

    [SerializeField] Mesh combinedMeshAsset;
    [SerializeField] MeshFilter combinedMeshFilter;
    [SerializeField] MeshRenderer combinedMeshRenderer;
    [SerializeField] MeshCollider combinedMeshCollider;
    [SerializeField] Material sourceVisualMaterial;
    [SerializeField] float duration = 0.5f;
    [SerializeField] bool disableGameObjectOnBreakComplete = true;

    MaterialPropertyBlock propertyBlock;
    Texture sourceTexture;
    bool isInitialized;

    public float EffectDuration => duration;

    void Awake()
    {
        EnsurePropertyBlock();
    }

    void Start()
    {
        EnsureInitialized();
    }

    [Button]
    public void Setup()
    {
        EnsurePropertyBlock();
        ResolveReferences();
        if (!ValidateRequiredReferences())
            return;

        CacheSourceDefaults();
        EnsureSharedShader();
        EnsureSharedMaterial();
        BindCombinedComponents();
        ApplyIdleState();
        isInitialized = true;
    }

    [Button]
    public void ResetSharedState()
    {
        ResetSharedRenderingState();
        isInitialized = false;
        Setup();
    }

    [Button]
    public void ResetEffect()
    {
        EnsureInitialized();
        CancelInvoke(nameof(DisableAfterPlayback));
        ApplyIdleState();
    }

    public void PlayBreak()
    {
        EnsureInitialized();
        CancelInvoke(nameof(DisableAfterPlayback));

        float invDuration = duration > 0f ? 1f / duration : 0f;
        ApplyPerInstanceState(PackBreakParams(Time.time, invDuration, BreakActiveValue));

        if (duration > 0f)
        {
            Invoke(nameof(DisableAfterPlayback), duration);
            return;
        }

        DisableAfterPlayback();
    }

    void DisableAfterPlayback()
    {
        if (disableGameObjectOnBreakComplete)
            gameObject.SetActive(false);
    }

    void EnsureInitialized()
    {
        EnsurePropertyBlock();

        if (isInitialized && combinedMeshRenderer != null && combinedMeshRenderer.sharedMaterial != null)
            return;

        Setup();
    }

    void EnsurePropertyBlock()
    {
        if (propertyBlock == null)
            propertyBlock = new MaterialPropertyBlock();
    }

    void ResolveReferences()
    {
        if (combinedMeshFilter == null)
            combinedMeshFilter = GetComponent<MeshFilter>();

        if (combinedMeshFilter == null)
            combinedMeshFilter = gameObject.AddComponent<MeshFilter>();

        if (combinedMeshRenderer == null)
            combinedMeshRenderer = GetComponent<MeshRenderer>();

        if (combinedMeshRenderer == null)
            combinedMeshRenderer = gameObject.AddComponent<MeshRenderer>();

        if (combinedMeshCollider == null)
            combinedMeshCollider = GetComponent<MeshCollider>();
    }

    bool ValidateRequiredReferences()
    {
        if (combinedMeshAsset == null)
        {
            Debug.LogError("Combined mesh asset is missing. Assign 'combinedMeshAsset' on the prefab.", this);
            return false;
        }

        if (sourceVisualMaterial == null)
        {
            Debug.LogError("Source visual material is missing. Assign 'sourceVisualMaterial' on the prefab.", this);
            return false;
        }

        return true;
    }

    void CacheSourceDefaults()
    {
        sourceTexture = GetSourceTexture(sourceVisualMaterial);
    }

    void EnsureSharedShader()
    {
        if (sharedShader != null)
            return;

        sharedShader = Shader.Find(ShaderName);

        if (sharedShader == null)
        {
            Debug.LogError($"Shader '{ShaderName}' not found.", this);
        }
    }

    void EnsureSharedMaterial()
    {
        if (sharedShader == null)
            return;

        if (sharedGpuMaterial == null || sharedGpuMaterial.shader != sharedShader)
        {
            sharedGpuMaterial = new Material(sharedShader)
            {
                name = "BreakBaseBlockGPU_SharedMaterial",
                enableInstancing = true
            };
        }

        ConfigureSharedMaterial();
    }

    void BindCombinedComponents()
    {
        combinedMeshFilter.sharedMesh = combinedMeshAsset;
        combinedMeshRenderer.sharedMaterial = sharedGpuMaterial;
        combinedMeshRenderer.enabled = true;

        if (combinedMeshCollider != null)
        {
            combinedMeshCollider.sharedMesh = combinedMeshAsset;
        }
    }

    void ApplyIdleState()
    {
        if (combinedMeshRenderer == null)
            return;

        combinedMeshRenderer.enabled = true;
        ConfigureSharedMaterial();
        ApplyPerInstanceState(PackBreakParams(0f, 0f, 0f));
    }

    void ConfigureSharedMaterial()
    {
        if (sharedGpuMaterial == null)
            return;

        if (sourceTexture != null && sharedMainTexture == null)
        {
            sharedMainTexture = sourceTexture;
        }
        else if (sharedMainTexture != sourceTexture && !loggedTextureMismatch)
        {
            loggedTextureMismatch = true;
            Debug.LogWarning("GpuBreakBlockEffect uses one shared GPU material. Different source textures across instances will split batching or render incorrectly.", this);
        }

        if (sharedMainTexture != null)
            sharedGpuMaterial.SetTexture(MainTextureId, sharedMainTexture);
    }

    static Vector4 PackBreakParams(float breakStartTime, float breakInvDuration, float breakActive)
    {
        // x = start time, y = inverse duration, z = active flag, w = reserved
        return new Vector4(breakStartTime, breakInvDuration, breakActive, 0f);
    }

    void ApplyPerInstanceState(Vector4 breakParams)
    {
        if (combinedMeshRenderer == null)
            return;

        propertyBlock.Clear();
        propertyBlock.SetVector(BreakParamsId, breakParams);
        combinedMeshRenderer.SetPropertyBlock(propertyBlock);
    }

    Texture GetSourceTexture(Material sourceMaterial)
    {
        if (sourceMaterial == null)
            return null;

        if (sourceMaterial.HasProperty(MainTextureId))
            return sourceMaterial.GetTexture(MainTextureId);

        if (sourceMaterial.HasProperty("_BaseMap"))
            return sourceMaterial.GetTexture("_BaseMap");

        if (sourceMaterial.HasProperty("_MainTex"))
            return sourceMaterial.GetTexture("_MainTex");

        return null;
    }

    static void ResetSharedRenderingState()
    {
        sharedShader = null;
        sharedGpuMaterial = null;
        sharedMainTexture = null;
        loggedTextureMismatch = false;
    }
}
