using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RealtimeCanvasUI : MonoBehaviour
{
    static RealtimeCanvasUI instance = null;

    public TMP_Text text_1;
    public TMP_Text text_2; // [결정체가 공격 받고 있습니다 !] 등의 메세지 들을 입력할 텍스트

    public Image[] iconImages; // 아이콘 이미지들

    public TMP_ColorGradient[] gradients;

    public float range = 2f;

    public Camera cam;

    public Image thinCircle;

    Coroutine thinCircleCoroutine = null;

    Canvas realtimeCanvas;
    RectTransform rootCanvasRect;
    Camera uiCamera;

    IEnumerator ThinCircleCoroutine_1()
    {
        float t = 0;
        Color color = Color.white;

        while (t < 1f)
        {
            yield return null;

            t += Time.deltaTime;
            color.a = t;
            thinCircle.color= color;
        }
        t = 1;

        yield return new WaitForSeconds(2f);

        while (t > 0f)
        {
            yield return null;

            t -= Time.deltaTime;
            color.a = t;
            thinCircle.color = color;
        }
        color.a = 0;
        thinCircle.color = color;
        thinCircleCoroutine = null;
    }

    IEnumerator ThinCircleCoroutine_2()
    {
        float t = 1;
        Color color = Color.white;
        color.a = 1f;

        thinCircle.color = color;

        yield return new WaitForSeconds(3f);


        while (t > 0f)
        {
            yield return null;

            t -= Time.deltaTime;

            color.a = t;

            thinCircle.color = color;
        }
        color.a = 0;

        thinCircle.color = color;

        thinCircleCoroutine = null;
    }

    Transform playerTransform = null;

    Vector3 PlayerPosition
    {
        get => playerTransform.position; 
    }
    Coroutine messageCoroutine = null;

    Coroutine[] iconImageCoroutine = { null, null, null, null, null, null };
    Coroutine[] iconPositionCoroutine = { null, null, null, null, null, null };
    private void Awake()
    {
        instance = this;

        CacheCanvasReferences();
    }
    public static void Notification(IconType iconType, Vector3 position)
    {
        instance.ExcutePositionIcon(iconType, position);
    }
    public static void Notification(IconType iconType, Vector3 position, string message)
    {
        instance.ExcutePositionIcon(iconType, position);

        instance.ExcuteMessage(message);

        instance.text_1.colorGradientPreset = instance.gradients[(int)iconType];
    }
    public static void Notification(IconType iconType, string message)
    {
        instance.text_1.colorGradientPreset = instance.gradients[(int)iconType];

        instance.ExcuteMessage(message);
    }
    Coroutine notificationTextCoroutine = null;
    public TMP_Text notification_2_Text;
    public static void NotificationText(string message)
    {
        instance.NotificationTextExcute(message);
    }
    void NotificationTextExcute(string message)
    {
        if (notificationTextCoroutine != null) StopCoroutine(notificationTextCoroutine);

        notificationTextCoroutine = StartCoroutine(NotificationTextExcuteCoroutine(message));
    }
    IEnumerator NotificationTextExcuteCoroutine(string message)
    {
        notification_2_Text.text = message;

        Color color = Color.white;
        color.a = 0;
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime;

            color.a = t;

            notification_2_Text.color = color;

            yield return null;
        }

        yield return new WaitForSeconds(1f);

        while (t > 0f)
        {
            t -= Time.deltaTime;

            color.a = t;

            notification_2_Text.color = color;

            yield return null;
        }
        color.a = 0;
        notification_2_Text.color = color;
    }
    public void ExcuteMessage(string message)
    {
        if (messageCoroutine != null) StopCoroutine(messageCoroutine);

        SoundManager.Play("NotificationSFX");

        messageCoroutine = StartCoroutine(MessageCoroutine(message));
    }
    IEnumerator MessageCoroutine(string message)
    {
        text_1.text = "";
        text_2.text = "";
        foreach (char c in message)
        {
            text_1.text += c;
            text_2.text += c;

            yield return new WaitForSeconds(0.05f);
        }
        yield return new WaitForSeconds(4f);

        messageCoroutine = null;

        text_1.text = "";
        text_2.text = "";
    }
    public void ExcutePositionIcon(IconType iconType, Vector3 position)
    {
        if (playerTransform == null || cam == null)
            return;

        if ((PlayerPosition - position).magnitude < range) return;

        int index = (int)iconType;

        if (iconImageCoroutine[index] != null)
        {
            StopCoroutine(iconImageCoroutine[index]);
        }
        if (iconPositionCoroutine[index] != null)
        {
            StopCoroutine(iconPositionCoroutine[index]);
        }

        iconImageCoroutine[index] = StartCoroutine(IconImageFadeCoroutine(index));
        iconPositionCoroutine[index] = StartCoroutine(IconPositionCoroutine(index, position));
    }
    IEnumerator IconPositionCoroutine(int index, Vector3 position)
    {
        float t = 0;

        Vector2 iconPosition;
        Vector2 circlePosition;

        while (t < 4f)
        {
            yield return null;

            t += Time.deltaTime;

            CacheCanvasReferences();

            if (TryWorldToCanvasLocal(PlayerPosition, out circlePosition))
                thinCircle.rectTransform.anchoredPosition = circlePosition;

            Vector3 iconWorldPosition = PlayerPosition + (position - PlayerPosition).normalized * range;
            if (TryWorldToCanvasLocal(iconWorldPosition, out iconPosition))
            {
                iconImages[index].rectTransform.anchoredPosition = iconPosition;
            }
        }
        iconPositionCoroutine[index] = null;
    }
    IEnumerator IconImageFadeCoroutine(int index)
    {
        iconImages[index].gameObject.SetActive(false);
        yield return null;
        iconImages[index].gameObject.SetActive(true);

        if (thinCircleCoroutine != null)
        {
            StopCoroutine(thinCircleCoroutine);

            thinCircleCoroutine = StartCoroutine(ThinCircleCoroutine_2());
        }
        else
        {
            thinCircleCoroutine = StartCoroutine(ThinCircleCoroutine_1());
        }

        float t = 0;
        Color color = iconImages[index].color;
        color.a = t;
        iconImages[index].color = color;

        while (t < 1f)
        {
            yield return null;

            t += Time.deltaTime;
            color.a = t;
            iconImages[index].color = color;
        }
        t = 1;
        color.a = t;
        iconImages[index].color = color;
        yield return new WaitForSeconds(2f);

        while (t > 0)
        {
            yield return null;

            t -= Time.deltaTime;
            color.a = t;
            iconImages[index].color = color;
        }
        t -= 0;
        color.a = t;
        iconImages[index].color = color;

        iconImageCoroutine[index] = null;

        iconImages[index].gameObject.SetActive(false);
    }
    public void SetPlayerTransform(Transform playerTransform) => this.playerTransform = playerTransform;

    void CacheCanvasReferences()
    {
        if (realtimeCanvas == null)
            realtimeCanvas = GetComponentInParent<Canvas>();

        if (realtimeCanvas == null)
            return;

        rootCanvasRect = realtimeCanvas.rootCanvas.transform as RectTransform;
        uiCamera = realtimeCanvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : (realtimeCanvas.worldCamera != null ? realtimeCanvas.worldCamera : Camera.main);
    }

    bool TryWorldToCanvasLocal(Vector3 worldPosition, out Vector2 localPosition)
    {
        localPosition = default;

        if (cam == null)
            return false;

        Vector3 screenPosition = cam.WorldToScreenPoint(worldPosition);
        if (screenPosition.z < 0f)
            return false;

        return TryScreenToCanvasLocal(screenPosition, out localPosition);
    }

    bool TryScreenToCanvasLocal(Vector2 screenPosition, out Vector2 localPosition)
    {
        if (rootCanvasRect == null)
        {
            localPosition = default;
            return false;
        }

        return RectTransformUtility.ScreenPointToLocalPointInRectangle(rootCanvasRect, screenPosition, uiCamera, out localPosition);
    }
}
public enum IconType
{
    Warning, // 결정체가 공격 받을 때
    Boss, // 보스가 등장했을 때
    Wave, // 웨이스가 시작했을 때
    Charge, // 코어 충전 시작했을 때
    DeadEnding, // 게임 오버
    HappyEnding, // 굳 잡 ~
}
