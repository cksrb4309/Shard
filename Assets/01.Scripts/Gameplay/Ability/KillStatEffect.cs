using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "KillStatEffect", menuName = "Ability/KillStatEffect")]
public class KillStatEffect : Ability, IOnKill
{
    public StatusEffect statusEffect;
    public override ICondition GetCondition() => this;
    public void OnKill()
    {
        PlayerStatus.GetStatusEffect(statusEffect);

        Debug.Log("KillStatEffect");
    }
}

[CreateAssetMenu(fileName = "KillOneSecondTripleKillStatEffect", menuName = "Ability/KillOneSecondTripleKillStatEffect")]
public class KillOneSecondTripleKillStatEffect : Ability, IOnKill
{
    public StatusEffect statusEffect;
    public override ICondition GetCondition() => this;
    int count = 0;
    int index = 0;
    Coroutine[] coroutines = new Coroutine[3] { null, null, null};
    public void OnKill()
    {
        count++;

        if (count == 3)
        {
            for (int i = 0; i < 3; i++)
            {
                if (coroutines[i] != null)
                {
                    GameManager.Instance.StopCoroutine(TripleKillCoroutine());
                    coroutines[i] = null;
                }
            }

            count = 0;

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
        count--;
    }
}
/* 
 * 죽였을 때 발생하는 아이템
 * 1초 이내에 3명의 적을 처치 시 이동 속도와 공격속도
 * 적을 처치하면 체력 재생량 증가
 * 적을 처치하면 공격력 증가?
 */