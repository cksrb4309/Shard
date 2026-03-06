using UnityEngine;

public abstract class MonsterMove : MonoBehaviour
{
    public abstract void StartMove();
    public abstract void StopMove();
    public abstract void SetDir(Vector3 dir);
}