using UnityEngine;

public abstract class TempAbility : ScriptableObject
{
    public abstract void SetCount(int count);
    public abstract ICondition GetCondition();
}