using UnityEngine;

[CreateAssetMenu(fileName = "KillProbabilityHealingOrbDropAbility", menuName = "Ability/Tear1/KillProbabilityHealingOrbDropAbility")]
public class KillProbabilityHealingOrbDropAbility : Ability, IOnKill
{
    public float startHeal;
    public float stackheal;

    float heal;
    const float probability = 0.5f;
    public override void SetCount(int count)
    {
        base.SetCount(count);

        heal = startHeal + stackheal * (count - 1);
    }
    public void OnKill(AttackData attackData)
    {
        if (!LuckManager.Calculate(probability, true))
        {

        }
        else
        {
            HealingOrb healingOrb = PoolingManager.Instance.GetObject<HealingOrb>("HealingOrb");

            healingOrb.transform.position = attackData.position;

            healingOrb.Setting(heal);
        }
    }
    public override ICondition GetCondition() => this;
}
