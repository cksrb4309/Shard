using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "KillProbabilityHealingOrbDropAbility", menuName = "Ability/Tear1/KillProbabilityHealingOrbDropAbility")]
public class KillProbabilityHealingOrbDropAbility : Ability
{
    public float startHeal;
    public float stackheal;

    float heal;
    const float probability = 0.5f;

    public override HashSet<AbilityEventType> SubscribedEvents => AttackData.KillEventType;

    public override void SetCount(int count)
    {
        base.SetCount(count);

        heal = startHeal + stackheal * (count - 1);
    }
    public override void OnEvent(AttackData attackData)
    {
        if (LuckManager.Calculate(probability, true))
        {
            HealingOrb healingOrb = PoolingManager.Instance.GetObject<HealingOrb>("HealingOrb");

            healingOrb.transform.position = attackData.Position;

            healingOrb.Setting(heal);
        }
    }
}
