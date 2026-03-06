using System.Collections.Generic;
using UnityEngine;

public abstract class Ability : ScriptableObject
{
    public string abilityName;
    public int tier;
    protected int count;
    public abstract HashSet<AbilityEventType> SubscribedEvents { get; }
    public virtual void SetCount(int count) => this.count = count;
    public virtual void Add() => SetCount(count+1);
    public abstract void OnEvent(AttackData attackData);
}