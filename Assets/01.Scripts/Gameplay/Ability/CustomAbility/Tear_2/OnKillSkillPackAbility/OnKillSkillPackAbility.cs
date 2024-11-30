using UnityEngine;

[CreateAssetMenu(fileName = "OnKillSkillPackAbility", menuName = "Ability/Tear2/OnKillSkillPackAbility")]
public class OnKillSkillPackAbility : Ability, IOnKill
{
    public float startProbability;
    public float stackProbability;

    float probability;

    public override void SetCount(int count)
    {
        base.SetCount(count);

        probability = startProbability + stackProbability * (1 - Mathf.Exp(-0.4f * count));
    }

    public void OnKill(AttackData attackData)
    {
        if (!LuckManager.Calculate(probability, true)) return;

        PoolingManager.Instance.GetObject("SkillPack").transform.position = attackData.position;
    }
    public override ICondition GetCondition() => this;
}
