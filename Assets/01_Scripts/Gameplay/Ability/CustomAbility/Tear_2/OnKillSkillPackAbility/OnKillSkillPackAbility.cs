using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "OnKillSkillPackAbility", menuName = "Ability/Tear2/OnKillSkillPackAbility")]
public class OnKillSkillPackAbility : Ability
{
    public float startProbability;
    public float stackProbability;

    float probability;

    public override HashSet<AbilityEventType> SubscribedEvents => AttackData.KillEventType;

    public override void SetCount(int count)
    {
        base.SetCount(count);

        probability = startProbability + stackProbability * (1 - Mathf.Exp(-0.4f * count));
    }
    public override void OnEvent(AttackData attackData)
    {
        if (LuckManager.Calculate(probability, true))
        {
            PoolingManager.Instance.GetObject("SkillPack").transform.position = attackData.Position;
        }
    }
}
