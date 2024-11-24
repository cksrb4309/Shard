using UnityEngine;

public interface IPassive : ICondition
{
    public (Attribute, string, float) GetValue(int count);
    public float TempGetValue();
}