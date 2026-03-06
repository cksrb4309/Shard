using System.Collections;
using UnityEngine;

public class ReturnObj : MonoBehaviour
{
    public bool isParentReturn = false;
    public bool isAutoReturn = true;

    public float delay;
    public string returnPoolName;

    private void OnEnable()
    {
        if (isAutoReturn)
        {
            StartCoroutine(ReturnCoroutine());
        }
    }

    IEnumerator ReturnCoroutine()
    {
        yield return new WaitForSeconds(delay);

        PoolingManager.Instance.ReturnObject(returnPoolName, isParentReturn ? transform.parent.gameObject : gameObject);
    }
    public void Return()
    {
        PoolingManager.Instance.ReturnObject(returnPoolName, isParentReturn ? transform.parent.gameObject : gameObject);
    }
}
