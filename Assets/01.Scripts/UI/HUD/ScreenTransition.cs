using Coffee.UIExtensions;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ScreenTransition : MonoBehaviour
{
    static ScreenTransition instance = null;

    public Transition[] transitions;

    public Image fadeImage;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;

            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public static void Play(string startTransition, string endTransition, Color fadeImageColor, Color transitionColor, Action action, float fadeStart = 0, float fadeEnd = 1)
    {
        instance.Excute(startTransition, endTransition, action, fadeImageColor, transitionColor, fadeStart, fadeEnd);
    }
    public static void Play(string startTransition, string endTransition, Color fadeImageColor, Color transitionColor, string sceneName, float fadeStart = 0, float fadeEnd = 1)
    {
        instance.Excute(startTransition, endTransition, sceneName, fadeImageColor, transitionColor, fadeStart, fadeEnd);
    }
    public void Excute(string startTransition, string endTransition, string sceneName, Color fadeImageColor, Color transitionColor, float fadeStart, float fadeEnd)
    {
        Transition st = null, ed = null;

        foreach (Transition transition in transitions)
        {
            if (transition.transitionName.Equals(startTransition))
                st = transition;

            if (transition.transitionName.Equals(endTransition))
                ed = transition;
        }

        if (st == null || ed == null)
        {
            Debug.LogError("Transition 찾기 실패 " + (st == null ? startTransition : endTransition));

            return;
        }

        st.material.SetColor("_Color", transitionColor);
        ed.material.SetColor("_Color", transitionColor);

        fadeImage.color = fadeImageColor;

        StartCoroutine(FadeCoroutine(fadeStart, fadeEnd, st.transitionLength, ed.transitionLength, sceneName, st, ed));
    }
    public void Excute(string startTransition, string endTransition, Action action, Color fadeImageColor, Color transitionColor, float fadeStart, float fadeEnd)
    {
        Transition st = null, ed = null;

        foreach (Transition transition in transitions)
        {
            if (transition.transitionName.Equals(startTransition))
                st = transition;
            
            if (transition.transitionName.Equals(endTransition))
                ed = transition;
        }

        if (st == null || ed == null)
        {
            Debug.LogError("Transition 찾기 실패 " + (st == null ? startTransition : endTransition));

            return;
        }

        st.material.SetColor("_Color", transitionColor);
        ed.material.SetColor("_Color", transitionColor);

        fadeImage.color = fadeImageColor;

        StartCoroutine(TransitionCoroutine(st, ed));
        StartCoroutine(FadeCoroutine(fadeStart, fadeEnd, st.transitionLength, ed.transitionLength, action));
    }
    IEnumerator TransitionCoroutine(Transition startTransition, Transition endTransition)
    {
        startTransition.uiParticle.Play();

        yield return new WaitForSeconds(startTransition.transitionLength);

        endTransition.uiParticle.Play();
    }
    IEnumerator FadeCoroutine(float fadeStart, float fadeEnd, float startTransitionLength, float endTransitionLength, Action action)
    {
        float t = 0;
        float max = startTransitionLength;

        Color startColor = fadeImage.color;
        Color endColor = fadeImage.color;

        startColor.a = 0f;
        endColor.a = 1f;

        while (t < max)
        {
            t += Time.deltaTime;

            fadeImage.color = Color.Lerp(startColor, endColor, Mathf.InverseLerp(fadeStart, max, t));

            yield return null;
        }

        action.Invoke(); // 함수 실행

        t = 1f;

        max = endTransitionLength;

        while (t > 0)
        {
            t -= Time.deltaTime;

            fadeImage.color = Color.Lerp(startColor, endColor, Mathf.InverseLerp(fadeEnd, max, t));

            yield return null;
        }
    }
    IEnumerator FadeCoroutine(float fadeStart, float fadeEnd, float startTransitionLength, float endTransitionLength, string sceneName, Transition st, Transition ed)
    {
        float t = 0;
        float max = startTransitionLength;

        Color startColor = fadeImage.color;
        Color endColor = fadeImage.color;

        startColor.a = 0f;
        endColor.a = 1f;

        st.uiParticle.Play();

        while (t < max)
        {
            t += Time.deltaTime;

            fadeImage.color = Color.Lerp(startColor, endColor, Mathf.InverseLerp(fadeStart, max, t));

            yield return null;
        }

        yield return WaitForSceneLoad(sceneName);

        ed.uiParticle.Play();

        max = endTransitionLength;

        t = max;

        yield return null;

        while (t > 0)
        {
            t -= Time.deltaTime;

            fadeImage.color = Color.Lerp(startColor, endColor, Mathf.InverseLerp(fadeEnd, max, t));

            yield return null;
        }
    }
    private IEnumerator WaitForSceneLoad(string sceneName)
    {
        // 씬 로딩 시작 (현재 씬 이름으로 예제 작성)
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);

        // 씬 로딩이 완료될 때까지 대기
        while (!asyncOperation.isDone)
        {
            yield return null;
        }

        Debug.Log("Scene Loaded!");
    }
}


[System.Serializable]
public class Transition
{
    public string transitionName;
    public UIParticle uiParticle;
    public Material material;
    public float transitionLength;
}