using TMPro;
using UnityEngine;

public class DamageTextController : MonoBehaviour
{
    static DamageTextController instance = null;

    public Canvas myCanvas;
    Camera myCamera;

    public Transform initialParent;
    public Transform finalParent;

    public Transform targetTransform;

    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        myCamera = Camera.main;
    }
    public static void OnDamageText(Vector3 pos, float damage, bool isUser = false)
    {
        instance.OnDamageTextExecute(pos, damage, isUser);
    }
    public static void OnCriticalDamageText(Vector3 pos, float damage)
    {
        instance.OnCriticalDamageTextExcute(pos, damage);
    }
    void OnCriticalDamageTextExcute(Vector3 pos, float damage)
    {
        pos = myCamera.WorldToScreenPoint(pos);

        if (pos.x < -100f || pos.x > Screen.width + 100f ||
            pos.y < -50f || pos.y > Screen.height + 50f) return;

        RectTransform obj;

        obj = PoolingManager.Instance.GetObject("MonsterHitCriticalDamageText").GetComponent<RectTransform>();

        obj.GetComponentInChildren<TMP_Text>().text = damage.ToString("F0");

        targetTransform.localPosition = pos - new Vector3(Screen.width * Random.Range(0.48f, 0.52f), Screen.height * Random.Range(0.49f, 0.51f), 0);

        obj.position = targetTransform.position;
    }
    void OnDamageTextExecute(Vector3 pos, float damage, bool isUser)
    {
        pos = myCamera.WorldToScreenPoint(pos);

        if (pos.x < -100f || pos.x > Screen.width + 100f ||
            pos.y < -50f || pos.y > Screen.height + 50f) return;

        RectTransform obj;

        if (isUser)
            obj = PoolingManager.Instance.GetObject("UserHitDamageText").GetComponent<RectTransform>();
        else
            obj = PoolingManager.Instance.GetObject("MonsterHitDamageText").GetComponent<RectTransform>();

        obj.GetComponentInChildren<TMP_Text>().text = damage.ToString("F0");

        targetTransform.localPosition = pos - new Vector3(Screen.width * Random.Range(0.48f, 0.52f), Screen.height * Random.Range(0.49f, 0.51f), 0);

        obj.position = targetTransform.position;
    }
}