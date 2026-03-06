using UnityEngine;

public class MonsterTracking : MonoBehaviour
{
    protected Transform currentTarget = null;
    protected Transform myTransform;
    private void Awake()
    {
        myTransform = transform;
    }
    public virtual Vector3 GetDir()
    {
        return (currentTarget.position - myTransform.position).normalized;
    }
    public virtual void SetTarget(Transform target)
    {
        currentTarget = target;
    }
}
