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

            yield return new WaitForSeconds(0.1f);
        }
        yield return new WaitForSeconds(2f);

        messageCoroutine = null;

        text_1.text = "";
        text_2.text = "";
    }
    public void ExcutePositionIcon(IconType iconType, Vector3 position)
    {
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

        Vector2 point;

        while (t < 4f)
        {
            yield return null;

            t += Time.deltaTime;

            point = cam.WorldToScreenPoint(PlayerPosition + (position - PlayerPosition).normalized * range);

            point.x -= Screen.width * 0.5f;
            point.y -= Screen.height * 0.5f;

            iconImages[index].rectTransform.localPosition = point;
        }
        iconPositionCoroutine[index] = null;
    }
    IEnumerator IconImageFadeCoroutine(int index)
    {
        iconImages[index].gameObject.SetActive(false);
        yield return null;
        iconImages[index].gameObject.SetActive(true);

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