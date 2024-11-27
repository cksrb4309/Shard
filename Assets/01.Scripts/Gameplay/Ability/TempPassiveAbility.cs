using UnityEngine;

[CreateAssetMenu(fileName = "TempPassiveAbility", menuName = "Ability/TempPassiveAbility")]
public class TempPassiveAbility : TempAbility, IPassive
{
    public Attribute attribute;
    public float startValue;
    public float stackValue;
    public override ICondition GetCondition() => this;
    public (Attribute, string, float) GetValue()
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
