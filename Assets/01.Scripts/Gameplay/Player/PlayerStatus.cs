using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class PlayerStatus : MonoBehaviour
{
    public PlayerAttributes playerAttributes;
    public PlayerHealth playerHealth;

    static PlayerStatus instance = null;

    public Dictionary<string, int> statusEffects = new Dictionary<string, int>();
    public Dictionary<string, Coroutine> coroutines = new Dictionary<string, Coroutine>();

    float healingMultiplier; // 치유양 기본 1

    private void Awake()
    {
        instance = this;
    }
    public static void GetStatusEffect(StatusEffect statusEffect)
    {
        instance.GetStatusEffectApply(statusEffect);
    }
    public void GetStatusEffectApply(StatusEffect statusEffect)
    {
        if (statusEffects.ContainsKey(statusEffect.effectName))
        {
            // 추가 스택이 가능하면 효과를 1 중첩한다!
            if (statusEffect.maxStack > statusEffects[statusEffect.effectName])
            {
                statusEffects[statusEffect.effectName]++;
            }
            // 아님 말고 ~
        }
        else
        {
            // 상태에 없던 효과였을 때
            statusEffects.Add(statusEffect.effectName, 1);
        }

        if (coroutines.ContainsKey(statusEffect.effectName))
        {
            StopCoroutine(coroutines[statusEffect.effectName]);
        }


        if (statusEffect is StatEffect statEffect)
        {
            playerAttributes.ApplyActiveBuffAttribute(
                statEffect.attribute,
                statEffect.effectName,
                statEffect.startValue + statEffect.stackValue * (statusEffects[statusEffect.effectName] - 1));

            coroutines[statEffect.effectName] = StartCoroutine(StatEffectCoroutine(statEffect));
        }
        if (statusEffect is TickEffect tickEffect)
        {
            coroutines[tickEffect.effectName] = StartCoroutine(TickEffectCoroutine(tickEffect));
        }
    }
    IEnumerator StatEffectCoroutine(StatEffect statEffect)
    {
        yield return new WaitForSeconds(statEffect.duration);

        playerAttributes.RemoveActiveBuffAttribute(statEffect.attribute, statEffect.effectName);

        statusEffects.Remove(statEffect.effectName);
        coroutines.Remove(statEffect.effectName);
    }
    IEnumerator TickEffectCoroutine(TickEffect tickEffect)
    {
        float duration = tickEffect.duration;
        float interval = tickEffect.interval;

        while (true)
        {
            float damage = tickEffect.startValue + tickEffect.stackValue * (statusEffects[tickEffect.effectName] - 1);

            if (damage > 0)
            {
                // 틱 데미지                        모든 틱 데미지는 기본 틱 데미지에 현재 난이도 * 0.2 (20%) 데미지를 추가한다
                playerHealth.Hit(damage + (damage * DifficultyManager.Difficulty) * 0.2f);
            }
            else
            {
                // 틱 힐
                playerHealth.Heal(damage * healingMultiplier);
            }

            yield return new WaitForSeconds(interval);

            duration -= interval;
            if (duration < 0) break;
        }
        statusEffects.Remove(tickEffect.effectName);
        coroutines.Remove(tickEffect.effectName);
    }
    public void SetHealingMultiplier(float healingMultiplier)
    {
        this.healingMultiplier = healingMultiplier;
    }
}
public class StatusEffect : ScriptableObject // 상태 변화 효과
{
    [Header("Base")]
    public string effectName; // 명칭
    public float duration; // 지속시간
    public float interval; // 적용 딜레이
    public int maxStack; // 중첩 횟수
}
[CreateAssetMenu(fileName = "Item", menuName = "Item/StatEffect")]
public class StatEffect : StatusEffect
{
    [Header("Stat")]
    public Attribute attribute; // 속성 선택
    public float startValue; // 값 선택 (버프 : 1~..., 디버프 : ...~1)
    public float stackValue; // 중첩마다 증가하는 값
}
[CreateAssetMenu(fileName = "Item", menuName = "Item/TickEffect")]
public class TickEffect : StatusEffect
{
    [Header("Tick")]
    // 틱 마다 가하는 데미지 ( 음수일 경우 틱마다 힐을 한다 )
    public float startValue; // 값 선택 (버프 : 1~..., 디버프 : ...~1)
    public float stackValue; // 중첩마다 증가하는 값
}