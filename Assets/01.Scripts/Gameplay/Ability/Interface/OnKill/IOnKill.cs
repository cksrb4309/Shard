using UnityEngine;

public interface IOnKill : ICondition
{
    public void OnKill(AttackData attackData);
}