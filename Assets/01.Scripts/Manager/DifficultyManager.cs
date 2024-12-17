using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DifficultyManager : MonoBehaviour
{
    static DifficultyManager instance = null;

    public static float Difficulty = 1;

    public Image[] difficultyImages;
    public Image[] effectImages;

    public Color color;

    Coroutine blankCoroutine = null;
    int currentBlankIndex = 0;
    float time = 0;
    float timeValue
    {
        get { return time; }
        set {
            time = value; 

            if ((int)(time / 3f) != currentBlankIndex)
            {
                currentBlankIndex = Mathf.Clamp((int)(time / 3f), 0, 9);

                Color alphaOneColor = color;
                Color alphaZeroeColor = color;

                alphaZeroeColor.a = 0;
                alphaOneColor.a = 1;

                for (int i = 0; i < difficultyImages.Length; i++)
                {
                    if (currentBlankIndex < i)
                    {
                        difficultyImages[i].color = alphaZeroeColor;
                    }
                    else
                    {
                        difficultyImages[i].color = alphaOneColor;
                    }
                }

                StopCoroutine(blankCoroutine);

                blankCoroutine = StartCoroutine(BlankCoroutine());
            }
        }
    }
    public void Start()
    {
        instance = this;

        Difficulty = 1;

        foreach (var item in difficultyImages)
            item.color = color;

        blankCoroutine = StartCoroutine(BlankCoroutine());

        StartCoroutine(IncreaseDifficultyCoroutine());
    }
    IEnumerator BlankCoroutine()
    {
        float t = 0;

        Color color = this.color;

        while (true)
        {
            difficultyImages[currentBlankIndex].color = color;

            while (t < 1f)
            {
                t += Time.deltaTime;
                color.a = t;
                difficultyImages[currentBlankIndex].color = color;
                yield return null;
            }
            while (t > 0)
            {
                t -= Time.deltaTime;
                color.a = t;
                difficultyImages[currentBlankIndex].color = color;
                yield return null;
            }
        }
    }
    public TMP_Text difficultyUpText;
    public TMP_Text difficultyCountText;
    IEnumerator DifficultyUpTextCoroutine()
    {
        Color color = Color.white;

        color.a = 1;
        difficultyUpText.color = color;
        yield return new WaitForSeconds(0.25f);
        color.a = 0;
        difficultyUpText.color = color;
        yield return new WaitForSeconds(0.25f);
        color.a = 1;
        difficultyUpText.color = color;
        yield return new WaitForSeconds(0.25f);
        color.a = 0;
        difficultyUpText.color = color;
        yield return new WaitForSeconds(0.25f);
        color.a = 1;
        difficultyUpText.color = color;
        yield return new WaitForSeconds(0.25f);
        color.a = 0;
        difficultyUpText.color = color;
        yield return new WaitForSeconds(0.25f);
        color.a = 1;
        difficultyUpText.color = color;
        yield return new WaitForSeconds(0.25f);
        color.a = 0;
        difficultyUpText.color = color;

        difficultyCountText.text = (int.Parse(difficultyCountText.text) + 1).ToString();
    }
    IEnumerator IncreaseDifficultyCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);

            timeValue += 1f;

            if (timeValue >= 30)
            {
                timeValue = 0;

                Difficulty *= 1.35f;

                StartCoroutine(EffectCoroutine());

                StartCoroutine(DifficultyUpTextCoroutine());
            }
        }
    }
    IEnumerator EffectCoroutine()
    {
        Color color = effectImages[0].color;
        color.a = 1f;
        foreach (var item in effectImages)
        {
            item.color = color;
        }
        color.a = 0f;
        for (int i = effectImages.Length - 1; i >= 0; i--) 
        {
            yield return new WaitForSeconds(0.2f);

            effectImages[i].color = color;  
        }
    }
    public static void NextStageSetting()
    {
        instance.timeValue += 15;

        if (instance.timeValue > 30)
        {
            instance.timeValue -= 15;

            Difficulty *= 1.35f;

            instance.StartCoroutine(instance.EffectCoroutine());

            instance.StartCoroutine(instance.DifficultyUpTextCoroutine());
        }
    }
}