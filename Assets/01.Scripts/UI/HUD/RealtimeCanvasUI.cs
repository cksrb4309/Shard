using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RealtimeCanvasUI : MonoBehaviour
{
    static RealtimeCanvasUI instance = null;

    public TMP_Text text_1;
    public TMP_Text text_2; // [����ü�� ���� �ް� �ֽ��ϴ� !] ���� �޼��� ���� �Է��� �ؽ�Ʈ

    public Image[] iconImages; // ������ �̹�����

    public TMP_ColorGradient[] gradients;

    public float range = 2f;

    public Camera cam;

    public Image thinCircle;

    Coroutine thinCircleCoroutine = null;

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

            Vector2 thinCirclePos = cam.WorldToScreenPoint(PlayerPosition);

            thinCirclePos.x -= Screen.width * 0.5f;
            thinCirclePos.y -= Screen.height * 0.5f;

            thinCircle.rectTransform.localPosition = thinCirclePos;

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
}
public enum IconType
{
    Warning, // ����ü�� ���� ���� ��
    Boss, // ������ �������� ��
    Wave, // ���̽��� �������� ��
    Charge, // �ھ� ���� �������� ��
    DeadEnding, // ���� ����
    HappyEnding, // �� �� ~
}