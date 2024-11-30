using UnityEngine;
[CreateAssetMenu(fileName = "CriticalRecoveryAbility", menuName = "Ability/Tear2/CriticalRecoveryAbility")]
public class CriticalRecoveryAbility : Ability, IOnCritical
{
    public float startValue;
    public float stackValue;
    float value;

    public override void SetCount(int count)
    {
        base.SetCount(count);
        Set();
    }
    void Set()
    {
        value = startValue + stackValue * (count - 1);
    }
    public void OnCritical(AttackData attackData)
    {
        PlayerStatus.Healing(value);
    }
    public override ICondition GetCondition() => this;
}
