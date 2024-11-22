using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Item/PassiveItem")]
public class PassiveAbility : Ability, IPassive
{
    public PassiveData passiveData;
    public override ICondition GetCondition() => this;
    public (Attribute, string, float) GetValue(int count)
    {
        return (passiveData.attribute, abilityName, passiveData.startValue + passiveData.stackValue * (count - 1));
    }
}