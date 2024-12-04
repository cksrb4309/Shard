using UnityEngine;

public abstract class Ability : ScriptableObject
{
    public string abilityName;
    public int tear;
    protected int count;
    public virtual void SetCount(int count) => this.count = count;
    public int GetCount() => count;
    public virtual void Add() => SetCount(count+1);
    public abstract ICondition GetCondition();
}