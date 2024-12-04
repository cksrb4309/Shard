using System.Collections;
using UnityEngine;

public class SearchShard : MonoBehaviour
{
    CustomMonster customMonster = null;
    public LayerMask layerMask;
    public float radius;

    WaitForSeconds delay = new WaitForSeconds(0.3f);

    private void Awake()
    {
        customMonster = transform.parent.GetComponent<CustomMonster>();
    }

    public void StartSearch()
    {
        StartCoroutine(SearchCoroutine());
    }
    public void StopSearch()
    {
        StopAllCoroutines();
    }
    IEnumerator SearchCoroutine()
    {
        while (true)
        {
            if (Physics.CheckSphere(transform.position, radius, layerMask))
            {
                customMonster.InShard();
            }
            else
            {
                customMonster.OutShard();
            }
            yield return delay;
        }
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawSphere(transform.position, radius);
    }
}