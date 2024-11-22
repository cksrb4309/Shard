using System.Collections;
using UnityEngine;

public class BreakBaseBlock : MonoBehaviour
{
    private void OnEnable()
    {
        StartCoroutine(ReturnCoroutine());
    }
    IEnumerator ReturnCoroutine()
    {
        yield return new WaitForSeconds(1f);

        PoolingManager.Instance.ReturnObject("BreakBaseBlock", gameObject);
    }
}