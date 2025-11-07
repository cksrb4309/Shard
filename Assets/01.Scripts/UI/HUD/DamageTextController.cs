using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class DamageTextController : MonoBehaviour
{
    static DamageTextController instance = null;

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
        if (damageTextSpawnCountThisFrame > 50)
        {
            return;
        }

        damageTextSpawnCountThisFrame++;

        DamageText damageText;

        if (isUser) damageText = PoolingManager.Instance.GetObject<DamageText>("UserHitDamageText");

        else if (isCritical) damageText = PoolingManager.Instance.GetObject<DamageText>("MonsterHitCriticalDamageText");

        else damageText = PoolingManager.Instance.GetObject<DamageText>("MonsterHitDamageText");

        damageText.SetText(damage.ToString("F0"));

        Vector3 dirToCamera = (myCamera.transform.position - pos).normalized;
        damageText.transform.position = pos + offset;

        damageText.transform.rotation = rotation;
    }
}