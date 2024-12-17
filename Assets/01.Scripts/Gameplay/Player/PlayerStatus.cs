using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class PlayerStatus : MonoBehaviour
{
    public PlayerAttributes playerAttributes;
    public PlayerHealth playerHealth;

    static PlayerStatus instance = null;

    public Dictionary<int, int> statusEffects = new Dictionary<int, int>();
    public Dictionary<int, Coroutine> coroutines = new Dictionary<int, Coroutine>();

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
        PlayerStatusNameToIdMapper.RegisterStatusEffect(statusEffect.effectName);

        int id = PlayerStatusNameToIdMapper.GetId(statusEffect.effectName);

        if (statusEffects.ContainsKey(id))
        {
            // 추가 스택이 가능하면 효과를 1 중첩한다!
            if (statusEffect.maxCount > statusEffects[id])
            {
                statusEffects[id]++;
            }
            // 아님 말고 ~
        }
        else
        {
            // 상태에 없던 효과였을 때
            statusEffects.Add(id, 1);
        }

        if (coroutines.ContainsKey(id))
        {
            StopCoroutine(coroutines[id]);
        }

        #region 상태 효과 적용
        if (statusEffect is StatEffect statEffect)
        {
            playerAttributes.ApplyActiveBuffAttribute(
                statEffect.attribute,
                statEffect.effectName,
                statEffect.value + statEffect.countValue * (statusEffects[id] - 1));

            if (gameObject.activeSelf)
                coroutines[id] = StartCoroutine(StatEffectCoroutine(statEffect));
        }
        if (statusEffect is TickEffect tickEffect)
        {
            if (gameObject.activeSelf)
                coroutines[id] = StartCoroutine(TickEffectCoroutine(tickEffect));
        }
        #endregion
    }
    IEnumerator StatEffectCoroutine(StatEffect statEffect)
    {
        yield return new WaitForSeconds(statEffect.duration);

        playerAttributes.RemoveActiveBuffAttribute(statEffect.attribute, statEffect.effectName);

        statusEffects.Remove(PlayerStatusNameToIdMapper.GetId(statEffect.effectName));
        coroutines.Remove(PlayerStatusNameToIdMapper.GetId(statEffect.effectName));
    }
    IEnumerator TickEffectCoroutine(TickEffect tickEffect)
    {
        float duration = tickEffect.duration;
        float interval = tickEffect.interval;
        int id = PlayerStatusNameToIdMapper.GetId(tickEffect.effectName);
        while (true)
        {
            float damage = tickEffect.value * statusEffects[id];

            if (damage > 0)
            {
                // 틱 데미지     TODO [플레이어 틱뎀 검토]                   모든 틱 데미지는 기본 틱 데미지에 현재 난이도 * 0.2 (20%) 데미지를 추가한다
                playerHealth.Hit(damage + (damage * DifficultyManager.Difficulty) * 0.2f);

                // 근데 사실 잘 생각해보면 플레이어에게 틱 데미지를 줄 일이 현재로써는 많이 없음
                // 현재 난이도에 따른 데미지가 맘에 안드는지는 고민해봐야함
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
        statusEffects.Remove(id);
        coroutines.Remove(id);
    }
    public void SetHealingMultiplier(float healingMultiplier)
    {
        this.healingMultiplier = healingMultiplier;
    }
    public static void Healing(float value)
    {
        instance.playerHealth.Heal(instance.healingMultiplier * value);
    }
}
