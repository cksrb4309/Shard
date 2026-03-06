using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SearchNearPlayers : MonoBehaviour
{
    public StageManager stageManager;

    public Image circleFillImage;

    public Collider cd;
    int needPlayerCounts = 0;
    int currentPlayerCount = 0;

    float t = 0;

    float startSize;

    Color startColor;

    public void SetPlayerCount(int playerCount) => needPlayerCounts = playerCount;

    private void Awake()
    {
        startSize = transform.localScale.x;

        startColor = circleFillImage.color;
    }
    void OnEnable()
    {
        t = 0;

        circleFillImage.transform.localScale = Vector3.one * startSize;

        startColor.a = 1;

        circleFillImage.color = startColor;

        StartCoroutine(FillCoroutine());

        currentPlayerCount = 0;

        cd.enabled = true;
    }
    private void OnDisable()
    {
        cd.enabled = false;

        stageManager.isAroundPlayers = false;

        t = 0;
    }
    private void OnTriggerEnter(Collider other)
    {
        currentPlayerCount++;

        Debug.Log("Enter 현재 수 : " + currentPlayerCount.ToString() + " / 필요 수 : " + needPlayerCounts.ToString());
    }
    private void OnTriggerExit(Collider other)
    {
        currentPlayerCount--;

        Debug.Log("Exit 현재 수 : " + currentPlayerCount.ToString() + " / 필요 수 : " + needPlayerCounts.ToString());
    }
    IEnumerator FillCoroutine()
    {
        float next;

        while (t < 1f)
        {
            next = (currentPlayerCount / needPlayerCounts);

            if (t < next)
            {
                t += Time.deltaTime;

                if (t >= next) t = next;
            }
            else if (t > next)
            {
                t -= Time.deltaTime;

                if (t <= next) t = next;
            }
            circleFillImage.fillAmount = t;

            yield return null;
        }
        stageManager.isAroundPlayers = true;

        StartCoroutine(DisableCoroutine());
    }
    IEnumerator DisableCoroutine()
    {
        float size = startSize * 1.5f;

        while (t > 0)
        {
            t -= Time.deltaTime;

            startColor.a = t;

            circleFillImage.color = startColor;

            circleFillImage.transform.localScale =
                Vector3.one * Mathf.Lerp(Mathf.Lerp(0, size, t), Mathf.Lerp(size, startSize, t), t);

            yield return null;
        }
        gameObject.SetActive(false);
    }
}
