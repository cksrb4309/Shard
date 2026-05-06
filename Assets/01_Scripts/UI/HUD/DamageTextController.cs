using UnityEngine;
public class DamageTextController : MonoBehaviour
{
    static DamageTextController instance = null;

    private const string UserHitPoolName = "UserHitDamageText";
    private const string MonsterHitPoolName = "MonsterHitDamageText";
    private const string MonsterCriticalHitPoolName = "MonsterHitCriticalDamageText";

    [SerializeField] DamageTextVfxBatchEmitter emitter;
    [SerializeField] WebGLDamageTextParticleEmitter webglParticleEmitter;
    [SerializeField] bool forceWebGLParticleFallbackInEditor = false;

    [SerializeField] DamageTextVfxBatchEmitter.TextEmitParams criticalTextParams;
    [SerializeField] DamageTextVfxBatchEmitter.TextEmitParams normalTextParams;
    [SerializeField] DamageTextVfxBatchEmitter.TextEmitParams maxDamageTextParams;

    Camera myCamera;

    Vector3 offset;
    Quaternion rotation;
    int damageTextSpawnCountThisFrame = 0;
    bool usePooledTextFallback = false;
    bool useWebGLParticleFallback = false;

    private void Awake()
    {
        instance = this;

        bool shouldUseWebGLParticleFallback =
            Application.platform == RuntimePlatform.WebGLPlayer
            || (Application.isEditor && forceWebGLParticleFallbackInEditor);

        if (shouldUseWebGLParticleFallback && webglParticleEmitter == null)
        {
            webglParticleEmitter = WebGLDamageTextParticleEmitter.CreateRuntimeEmitter(transform);
        }

        useWebGLParticleFallback = shouldUseWebGLParticleFallback && webglParticleEmitter != null;
        usePooledTextFallback = emitter == null || !SystemInfo.supportsComputeShaders;
    }
    private void Start()
    {
        myCamera = Camera.main;

        rotation = myCamera.transform.rotation;

        offset = rotation * Vector3.back * 25f;
    }
    private void Update()
    {
        damageTextSpawnCountThisFrame = 0;
    }
    public static void OnDamageText(Vector3 pos, float damage, bool isUser = false)
    {
        instance.OnDamageTextExecute(pos, damage, isUser, false);
    }
    public static void OnCriticalDamageText(Vector3 pos, float damage)
    {
        instance.OnDamageTextExecute(pos, damage, false, true);
    }
    void OnDamageTextExecute(Vector3 pos, float damage, bool isUser, bool isCritical)
    {
        if (damageTextSpawnCountThisFrame > 400)
        {
            return;
        }

        damageTextSpawnCountThisFrame++;

        Vector3 randomOffset = useWebGLParticleFallback ? offset : GetRandomOffset();

        if (useWebGLParticleFallback && webglParticleEmitter.TryEmit(pos, damage, isUser, isCritical, randomOffset))
        {
            return;
        }

        if (usePooledTextFallback)
        {
            SpawnPooledDamageText(pos, damage, isUser, isCritical, randomOffset);
            return;
        }

        if (!isCritical)
        {
            if (damage == float.MaxValue)
            {
                maxDamageTextParams.Position = pos;
                emitter.EnqueueText(new DamageTextVfxBatchEmitter.DefaultTextRequest("DEAD", maxDamageTextParams));
            }
            else
            {
                normalTextParams.Position = pos + randomOffset;
                emitter.EnqueueText(new DamageTextVfxBatchEmitter.DamageTextRequest((int)damage, normalTextParams));
            }
        }
        else
        {
            criticalTextParams.Position = pos + randomOffset;

            emitter.EnqueueText(new DamageTextVfxBatchEmitter.DamageTextRequest((int)damage, criticalTextParams));
        }
    }

    void SpawnPooledDamageText(Vector3 pos, float damage, bool isUser, bool isCritical, Vector3 randomOffset)
    {
        if (PoolingManager.Instance == null)
        {
            return;
        }

        string poolName = GetPoolName(isUser, isCritical);
        DamageText pooledText = PoolingManager.Instance.GetObject<DamageText>(poolName);
        if (pooledText == null)
        {
            return;
        }

        pooledText.transform.position = pos + randomOffset;
        pooledText.transform.rotation = rotation;
        pooledText.SetText(damage == float.MaxValue ? "DEAD" : ((int)damage).ToString());
    }

    string GetPoolName(bool isUser, bool isCritical)
    {
        if (isUser)
        {
            return UserHitPoolName;
        }

        return isCritical ? MonsterCriticalHitPoolName : MonsterHitPoolName;
    }

    Vector3 GetRandomOffset()
    {
        return new Vector3(Random.Range(-0.5f, 0.5f), 0f, Random.Range(-0.5f, 0.5f)) + offset;
    }
}
