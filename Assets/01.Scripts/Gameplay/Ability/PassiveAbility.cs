using UnityEngine;

[CreateAssetMenu(fileName = "Ability", menuName = "Ability/PassiveItem")]
public class PassiveAbility : Ability, IPassive
{
    public PassiveData passiveData;
    public override ICondition GetCondition() => this;
    public (Attribute, string, float) GetValue(int count)
    {
        return (passiveData.attribute, abilityName, passiveData.startValue + passiveData.stackValue * (count - 1));
    }

    public float TempGetValue()
    {
        throw new System.NotImplementedException();
    }
}