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
 * �׿��� �� �߻��ϴ� ������
 * 1�� �̳��� 3���� ���� óġ �� �̵� �ӵ��� ���ݼӵ�
 * ���� óġ�ϸ� ü�� ����� ����
 * ���� óġ�ϸ� ���ݷ� ����?
 */