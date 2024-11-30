using UnityEngine;

[CreateAssetMenu(fileName = "CriticalStatEffectAbility", menuName = "Ability/CriticalStatEffectAbility")]
public class CriticalStatEffect : Ability, IOnCritical
{
    public StatEffect statEffect;
    public override ICondition GetCondition() => this;
    public override void SetCount(int count)
    {
        base.SetCount(count);
        EffectSet();
    }
    public void EffectSet()
    {
        statEffect.SetCount(count);
    }
    public void OnCritical(AttackData attackData)
    {
        PlayerStatus.GetStatusEffect(statEffect);
    }
}
