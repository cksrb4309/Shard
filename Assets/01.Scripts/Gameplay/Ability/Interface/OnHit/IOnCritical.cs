using UnityEngine;

public interface IOnCritical : ICondition
{
    public void OnCritical(AttackData attackData);
}