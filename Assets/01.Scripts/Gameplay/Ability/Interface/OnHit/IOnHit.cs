using UnityEngine;

public interface IOnHit : ICondition
{
    public void OnHit(AttackData attackData);
}
