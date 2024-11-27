using UnityEngine;
[CreateAssetMenu(fileName = "RecoveryHitAbility", menuName = "Ability/Tear2/RecoveryHitAbility")]
public class RecoveryHitAbility : TempAbility, IOnHit
{
    int healingValue = 0;

    public override void SetCount(int count)
    {
        base.SetCount(count);

        healingValue = count;
    }
    public override void Add()
    {
        base.Add();

        healingValue = count;
    }

    public void OnHit(AttackData attackData)
    {
        PlayerStatus.Healing(healingValue);
    }
    public override ICondition GetCondition() => this;
}
