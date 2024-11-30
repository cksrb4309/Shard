using UnityEngine;

[CreateAssetMenu(fileName = "OnKillSheildAbility", menuName = "Ability/Tear1/OnKillSheildAbility")]
public class OnKillSheildAbility : Ability, IOnKill
{
    public float startValue;
    public float stackValue;

    float value;
    public override void SetCount(int count)
    {
        base.SetCount(count);

        value = startValue + stackValue * (count - 1);
    }
    public void OnKill(AttackData attackData)
    {
        PlayerHealth.GetSheild(value);
    }
    public override ICondition GetCondition() => this;
}
