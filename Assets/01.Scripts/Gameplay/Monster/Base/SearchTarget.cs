using UnityEngine;

public class SearchTarget : MonoBehaviour
{
    CustomMonster customMonster;

    public float radius;

    public LayerMask layerMask;

    private void Awake()
    {
        customMonster = transform.parent.GetComponent<CustomMonster>();
    }

    public void OnTriggerEnter(Collider other)
    {
        Search(radius);
    }
    public void Search(float radius)
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius, layerMask);

        float min = float.MaxValue;

        Collider target = null;

        for (int i = 0; i < colliders.Length; i++)
        {
            float current = Vector3.Magnitude(colliders[i].transform.position - transform.position);

            if (min > current)
            {
                min = current;

                target = colliders[i];
            }
        }
        if (target == null)
        {
            customMonster.SetTarget(GameManager.GetCoreTransform());
        }
        else
        {
            customMonster.SetTarget(target.transform);
        }
    }
}
