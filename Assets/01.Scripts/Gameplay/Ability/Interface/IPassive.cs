using UnityEngine;

public interface IPassive : ICondition
{
    public (Attribute, string, float) GetValue();
    public float TempGetValue();
}