using UnityEngine;

public class TempPassiveAbility : TempAbility, IPassive
{
    public Attribute attribute;
    public string abilityName;
    public float startValue;
    public float stackValue;
    int count;
    public override ICondition GetCondition() => this;
    public (Attribute, string, float) GetValue(int count)
    {
        return (attribute, abilityName, startValue + stackValue * (count - 1));
    }

    public override void SetCount(int count)
    {
        this.count = count;
    }

    public float TempGetValue()
    {
        return startValue + stackValue * (count - 1);
    }
}
