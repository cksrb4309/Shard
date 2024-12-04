using TMPro;
using UnityEngine;

public class DamageTextController : MonoBehaviour
{
    static DamageTextController instance = null;

    public Canvas myCanvas;
    public Camera myCamera;

    public Transform initialParent;
    public Transform finalParent;

    public Transform targetTransform;

    private void Awake()
    {
        instance = this;
    }
    public static void OnDamageText(Vector3 pos, float damage, bool isUser = false)
    {
        instance.OnDamageTextExecute(pos, damage, isUser);
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

        //TMP_Text text = obj.GetComponentInChildren<TMP_Text>();

        //text.text = damage.ToString("F0");

        obj.GetComponentInChildren<TMP_Text>().text = damage.ToString("F0");

        //obj.SetParent(initialParent);

        targetTransform.localPosition = pos - new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0);

        obj.position = targetTransform.position;

        //obj.SetParent(initialParent);
    }
}