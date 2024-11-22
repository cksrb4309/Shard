using UnityEngine;

public interface IAttackable
{
    public void ReceiveHit(float damage);
    public bool IsAlive();
}