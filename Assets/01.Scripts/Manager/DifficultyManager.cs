using System.Collections;
using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    static DifficultyManager instance = null;

    public static float Difficulty = 1;

    float time = 0;

    public void Start()
    {
        instance = this;

        StartCoroutine(IncreaseDifficultyCoroutine());
    }

    IEnumerator IncreaseDifficultyCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);

            time += 1f;

            if (time >= 60)
            {
                time = 0;

                Difficulty *= 1.5f;
            }
        }
    }
    
    public void StopIncreaseDifficulty()
    {
        StopCoroutine(IncreaseDifficultyCoroutine());
    }

    public static void NextStageSetting()
    {
        instance.time += 30;

        if (instance.time > 60)
        {
            instance.time -= 60;

            Difficulty *= 1.5f;
        }

        instance.StartCoroutine(instance.IncreaseDifficultyCoroutine());
    }
}