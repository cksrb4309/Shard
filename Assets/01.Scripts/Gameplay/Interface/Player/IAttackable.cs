using UnityEngine;

public interface IAttackable
{
    public Vector3 GetPosition();
    public void ReceiveHit(float damage);
    public void ReceiveDebuff(StatusEffect effect, float damage = 0);
    public bool IsAlive();
}