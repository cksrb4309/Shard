using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "KillOneSecondTripleKillStatEffect", menuName = "Ability/Tear2/KillOneSecondTripleKillStatEffect")]
public class KillOneSecondTripleKillStatEffect : Ability, IOnKill
{
    public StatusEffect statusEffect_1;
    public StatusEffect statusEffect_2;
    public override ICondition GetCondition() => this;

    int killCount = 0;
    int index = 0;

    Coroutine[] coroutines = new Coroutine[3] { null, null, null };

    public override void SetCount(int count)
    {
        base.SetCount(count);
        Set();
    }
    void Set()
    {
        statusEffect_1.SetCount(count);
        statusEffect_2.SetCount(count);
    }

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

            PlayerStatus.GetStatusEffect(statusEffect_1);
            PlayerStatus.GetStatusEffect(statusEffect_2);
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