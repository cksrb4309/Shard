using UnityEngine;

public class SearchMonster : MonoBehaviour
{
    public Teammate teammate;

    public SphereCollider cd;
    public void Setting(float range)
    {
        cd.radius = range;
    }
    private void OnTriggerEnter(Collider other)
    {
        teammate.Attack(other.GetComponent<IAttackable>());
    }
    public void OnSearch()
    {
        cd.enabled = true;
    }
    public void OffSearch()
    {
        cd.enabled = false;
    }
}
