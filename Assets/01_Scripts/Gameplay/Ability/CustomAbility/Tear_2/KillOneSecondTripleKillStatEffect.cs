using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "KillOneSecondTripleKillStatEffect", menuName = "Ability/Tear2/KillOneSecondTripleKillStatEffect")]
public class KillOneSecondTripleKillStatEffect : Ability
{
    public StatusEffect statusEffect_1;
    public StatusEffect statusEffect_2;

    int killCount = 0;
    int index = 0;

    Coroutine[] coroutines = new Coroutine[3] { null, null, null };

    public override HashSet<AbilityEventType> SubscribedEvents => AttackData.KillEventType;

    public override void SetCount(int count)
    {
        base.SetCount(count);

        statusEffect_1.SetCount(count);
        statusEffect_2.SetCount(count);
    }
    IEnumerator TripleKillCoroutine()
    {
        yield return new WaitForSeconds(1f);

        killCount--;
    }

    public override void OnEvent(AttackData attackData)
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
}