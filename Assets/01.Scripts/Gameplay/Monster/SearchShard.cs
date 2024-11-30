using UnityEngine;

public class SearchShard : MonoBehaviour
{
    public Monster monster;
    public LayerMask layerMask;
    public float radius;

    private void FixedUpdate()
    {
        if (Physics.CheckSphere(transform.position, radius, layerMask))
        {
            monster.InShard();
        }
        else
        {
            monster.OutShard();
        }
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawSphere(transform.position, radius);
    }
}