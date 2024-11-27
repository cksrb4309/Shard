using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "KillStatEffect", menuName = "Ability/KillStatEffect")]
public class KillStatEffect : TempAbility, IOnKill
{
    public StatusEffect statusEffect;
    public override ICondition GetCondition() => this;
    public void OnKill(AttackData attackData)
    {
        PlayerStatus.GetStatusEffect(statusEffect);

        Debug.Log("KillStatEffect");
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
    public override void Add()
    {
        base.Add();

        EffectSetting();
    }
}

[CreateAssetMenu(fileName = "KillOneSecondTripleKillStatEffect", menuName = "Ability/KillOneSecondTripleKillStatEffect")]
public class KillOneSecondTripleKillStatEffect : TempAbility, IOnKill
{
    public StatusEffect statusEffect;
    public override ICondition GetCondition() => this;

    int killCount = 0;
    int index = 0;

    Coroutine[] coroutines = new Coroutine[3] { null, null, null};
    public void OnKill(AttackData attackData)
    {
        killCount++;

        if (killCount == 3)
        {
            for (int i = 0; i < 3; i++)
            {
                if (coroutines[i] != null)
                {
                    GameManager.Instance.StopCoroutine(coroutines[i]);

                    coroutines[i] = null;
                }
            }

            killCount = 0;

            PlayerStatus.GetStatusEffect(statusEffect);
        }
        else
        {
            coroutines[index++] = GameManager.Instance.StartCoroutine(TripleKillCoroutine());

            if (index == 3) index = 0;
        }
    }
    IEnumerator TripleKillCoroutine()
    {
        yield return new WaitForSeconds(1f);
        killCount--;
    }
}
/* 
 * �׿��� �� �߻��ϴ� ������
 * 1�� �̳��� 3���� ���� óġ �� �̵� �ӵ��� ���ݼӵ�
 * ���� óġ�ϸ� ü�� ����� ����
 * ���� óġ�ϸ� ���ݷ� ����?
 */