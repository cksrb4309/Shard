using UnityEngine;
public class DamageTextController : MonoBehaviour
{
    static DamageTextController instance = null;

    [SerializeField] DamageTextVfxBatchEmitter emitter;

    [SerializeField] DamageTextVfxBatchEmitter.TextEmitParams criticalTextParams;
    [SerializeField] DamageTextVfxBatchEmitter.TextEmitParams normalTextParams;
    [SerializeField] DamageTextVfxBatchEmitter.TextEmitParams maxDamageTextParams;

    Camera myCamera;

    Vector3 offset;
    Quaternion rotation;
    int damageTextSpawnCountThisFrame = 0;

    private void Awake()
    {
        instance = this;
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

        if (!isCritical)
        {
            if (damage == float.MaxValue)
            {
                maxDamageTextParams.Position = pos;
                emitter.EnqueueText(new DamageTextVfxBatchEmitter.DefaultTextRequest("DEAD", maxDamageTextParams));
            }
            else
            {
                normalTextParams.Position = pos + new Vector3(Random.Range(-0.5f, 0.5f), 0, Random.Range(-0.5f, 0.5f));
                emitter.EnqueueText(new DamageTextVfxBatchEmitter.DamageTextRequest((int)damage, normalTextParams));
            }
        }
        else
        {
            criticalTextParams.Position = pos + new Vector3(Random.Range(-0.5f, 0.5f), 0, Random.Range(-0.5f, 0.5f));

            emitter.EnqueueText(new DamageTextVfxBatchEmitter.DamageTextRequest((int)damage, criticalTextParams));
        }
    }
}