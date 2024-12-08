using System.Collections;
using UnityEngine;

public class PlayModeSelectUI : MonoBehaviour
{
    public GameObject shipSelectMenuUI;

    int shipID = 0;

    bool isCancel = false;
    bool isPlaying = false;

    public void SinglePlay()
    {
        shipSelectMenuUI.SetActive(true);

        StartCoroutine(SinglePlayCoroutine());
    }
    public void MultyPlay()
    {

    }
    IEnumerator SinglePlayCoroutine()
    {
        while (isCancel == false && isPlaying == false) yield return null;

        if (isCancel)
        {
            isCancel = false;

            yield break;
        }

        PlayerPrefs.SetInt("PlayerCount", 1);

        PlayerPrefs.SetInt("Player0", shipID);

        PlayerPrefs.SetInt("MainPlayer", 0);

        ScreenTransition.Play("Cube_FadeOut", "Cube_FadeIn", Color.black, Color.black, "Game", 1.5f, 0);
    }
    IEnumerator MultyPlayCoroutine()
    {
        yield return null;
    }

    public void SetShipID(int shipID)
    {
        this.shipID = shipID;

        isPlaying = true;
    }

    public void Cancel()
    {
        isCancel = true;
    }
    public void Close()
    {
        gameObject.SetActive(false);
    }
}
