using UnityEngine;

[CreateAssetMenu(fileName = "PassiveAbility", menuName = "Ability/PassiveAbility")]
public class PassiveAbility : Ability, IPassive
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
}
