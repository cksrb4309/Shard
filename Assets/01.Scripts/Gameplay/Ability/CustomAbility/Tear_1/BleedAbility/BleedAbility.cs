using UnityEngine;

[CreateAssetMenu(fileName = "BleedAbility", menuName = "Ability/Tear1/BleedAbility")]
public class BleedAbility : Ability, IOnHit
{
    public TickEffect bleedStatusEffect;
    const float probability = 0.1f;
    public override void SetCount(int count)
    {
        base.SetCount(count);

        bleedStatusEffect.SetCount(count);
    }
    public void OnHit(AttackData attackData)
    {
        if (LuckManager.Calculate(probability, true))
            GameManager.GetLastHit().ReceiveDebuff(bleedStatusEffect);
    }
    public override ICondition GetCondition() => this;
}
