using UnityEngine;
[CreateAssetMenu(fileName = "RecoveryHitAbility", menuName = "Ability/Tear2/RecoveryHitAbility")]
public class RecoveryHitAbility : Ability, IOnHit
{
    int healingValue = 0;

    public override void SetCount(int count)
    {
        base.SetCount(count);

        healingValue = count;
    }
    public void OnHit(AttackData attackData)
    {

        PlayerStatus.Healing((float)healingValue);
    }
    public override ICondition GetCondition() => this;
}
