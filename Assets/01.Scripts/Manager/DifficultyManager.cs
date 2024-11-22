using System.Collections;
using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    public static int Difficulty = 0;

    float time = 0;

    public void Start()
    {
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

                Difficulty++;
            }
        }
    }
    
    public void StopIncreaseDifficulty()
    {
        StopCoroutine(IncreaseDifficultyCoroutine());
    }

    public void NextStageSetting()
    {
        time += 30;

        if (time > 60)
        {
            time -= 60;

            Difficulty++;
        }

        StartCoroutine(IncreaseDifficultyCoroutine());
    }
}