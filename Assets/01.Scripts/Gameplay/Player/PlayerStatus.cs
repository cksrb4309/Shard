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
        if (statusEffects.ContainsKey(statusEffect.effectName))
        {
            // �߰� ������ �����ϸ� ȿ���� 1 ��ø�Ѵ�!
            if (statusEffect.maxStack > statusEffects[statusEffect.effectName])
            {
                statusEffects[statusEffect.effectName]++;
            }
            // �ƴ� ���� ~
        }
        else
        {
            // ���¿� ���� ȿ������ ��
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
                // ƽ ������                        ��� ƽ �������� �⺻ ƽ �������� ���� ���̵� * 0.2 (20%) �������� �߰��Ѵ�
                playerHealth.Hit(damage + (damage * DifficultyManager.Difficulty) * 0.2f);
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
        statusEffects.Remove(tickEffect.effectName);
        coroutines.Remove(tickEffect.effectName);
    }
    public void SetHealingMultiplier(float healingMultiplier)
    {
        this.healingMultiplier = healingMultiplier;
    }
}
public class StatusEffect : ScriptableObject // ���� ��ȭ ȿ��
{
    [Header("Base")]
    public string effectName; // ��Ī
    public float duration; // ���ӽð�
    public float interval; // ���� ������
    public int maxStack; // ��ø Ƚ��
}
[CreateAssetMenu(fileName = "Item", menuName = "Item/StatEffect")]
public class StatEffect : StatusEffect
{
    [Header("Stat")]
    public Attribute attribute; // �Ӽ� ����
    public float startValue; // �� ���� (���� : 1~..., ����� : ...~1)
    public float stackValue; // ��ø���� �����ϴ� ��
}
[CreateAssetMenu(fileName = "Item", menuName = "Item/TickEffect")]
public class TickEffect : StatusEffect
{
    [Header("Tick")]
    // ƽ ���� ���ϴ� ������ ( ������ ��� ƽ���� ���� �Ѵ� )
    public float startValue; // �� ���� (���� : 1~..., ����� : ...~1)
    public float stackValue; // ��ø���� �����ϴ� ��
}