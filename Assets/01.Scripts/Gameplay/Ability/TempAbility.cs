using UnityEngine;

public abstract class TempAbility : ScriptableObject
{
    public string abilityName;
    protected int count;
    public virtual void SetCount(int count) => this.count = count;
    public int GetCount() => count;
    public virtual void Add() => count++;
    public abstract ICondition GetCondition();
}