using System.Collections;
using UnityEngine;

public class ReturnObj : MonoBehaviour
{
    public float delay;
    public string returnPoolName;

    private void OnEnable()
    {
        StartCoroutine(ReturnCoroutine());
    }

    IEnumerator ReturnCoroutine()
    {
        yield return new WaitForSeconds(delay);

        PoolingManager.Instance.ReturnObject(returnPoolName, gameObject);
    }
}
