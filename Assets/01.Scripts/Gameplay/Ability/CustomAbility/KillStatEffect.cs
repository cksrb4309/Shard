using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "KillStatEffect", menuName = "Ability/KillStatEffect")]
public class KillStatEffect : Ability, IOnKill
{
    public StatusEffect statusEffect;
    public override ICondition GetCondition() => this;
    public void OnKill(AttackData attackData)
    {
        PlayerStatus.GetStatusEffect(statusEffect);
    }
    void EffectSetting()
    {
        statusEffect.SetCount(count);
    }
    public override void SetCount(int count)
    {
        base.SetCount(count);

        EffectSetting();
    }
}


/* 
 * 죽였을 때 발생하는 아이템
 * 1초 이내에 3명의 적을 처치 시 이동 속도와 공격속도
 * 적을 처치하면 체력 재생량 증가
 * 적을 처치하면 공격력 증가?
 */