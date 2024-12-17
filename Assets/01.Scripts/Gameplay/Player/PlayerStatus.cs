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

    float healingMultiplier; // ġ���� �⺻ 1

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
            // �߰� ������ �����ϸ� ȿ���� 1 ��ø�Ѵ�!
            if (statusEffect.maxCount > statusEffects[id])
            {
                statusEffects[id]++;
            }
            // �ƴ� ���� ~
        }
        else
        {
            // ���¿� ���� ȿ������ ��
            statusEffects.Add(id, 1);
        }

        if (coroutines.ContainsKey(id))
        {
            StopCoroutine(coroutines[id]);
        }

        #region ���� ȿ�� ����
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
                // ƽ ������     TODO [�÷��̾� ƽ�� ����]                   ��� ƽ �������� �⺻ ƽ �������� ���� ���̵� * 0.2 (20%) �������� �߰��Ѵ�
                playerHealth.Hit(damage + (damage * DifficultyManager.Difficulty) * 0.2f);

                // �ٵ� ��� �� �����غ��� �÷��̾�� ƽ �������� �� ���� ����ν�� ���� ����
                // ���� ���̵��� ���� �������� ���� �ȵ������ ����غ�����
            }
            else
            {
                // ƽ ��
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
