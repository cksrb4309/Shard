using TMPro;
using UnityEngine;

public class DamageTextController : MonoBehaviour
{
    static DamageTextController instance = null;

    public Canvas myCanvas;
    public Camera myCamera;

    public Transform initialParent;
    public Transform finalParent;

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

        TMP_Text text;

        if (isUser)
            text = PoolingManager.Instance.GetObject<TMP_Text>("UserHitDamageText");
        else
            text = PoolingManager.Instance.GetObject<TMP_Text>("MonsterHitDamageText");

        text.text = damage.ToString("F0");

        text.rectTransform.SetParent(initialParent);

        text.rectTransform.localPosition = pos - new Vector3(Screen.width*0.5f, Screen.height*0.5f, 0);

        text.rectTransform.SetParent(finalParent);
    }
}