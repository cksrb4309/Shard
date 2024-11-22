using System;
using UnityEngine;

public class AttackData
{
    public Action onKillAction = null; // óġ���� �� ������ Action
    public Action onHitAction = null; // ������ ������ ������ Action
    public Action onCriticalAction = null; // ũ��Ƽ�� ������ ������ ������ Action
    public Action<float> onHitDamageAction = null; // ������ ������ Damage �� �ް� ������ Action
    public Func<bool> onHitChanceAction = null; // ������ �� ���� Ȯ���� �ѹ��� ������ Func
    public Func<float, bool> onHitDamageChanceAction = null; // ������ �� ���� Ȯ���� Damage �� �ް� ������ Func

    public AttackData(Action onKillAction, Action onHitAction, Action onCriticalAction,
        Action<float> onHitDamageAction, Func<bool> onHitChanceAction, Func<float, bool> onHitDamageChanceAction)
    {
        this.onKillAction = onKillAction;
        this.onHitAction = onHitAction;
        this.onCriticalAction = onCriticalAction;
        this.onHitDamageAction = onHitDamageAction;
        this.onHitChanceAction = onHitChanceAction;
        this.onHitDamageChanceAction = onHitDamageChanceAction;
    }
    public AttackData()
    {
        onKillAction = null;
        onHitAction = null;
        onCriticalAction = null;
        onHitDamageAction = null;
        onHitChanceAction = null;
        onHitDamageChanceAction = null;
    }
    public void OnKill()
    {
        onKillAction?.Invoke();
    }
    public void OnHit(float damage)
    {
        OnHit();
        OnHitDamage(damage);
        OnHitChance();
        OnHitChanceDamage(damage);
    }
    public void OnCritical()
    {
        onCriticalAction?.Invoke();
    }
    void OnHit()
    {
        onHitAction?.Invoke();
    }
    void OnHitDamage(float damage)
    {
        onHitDamageAction?.Invoke(damage);
    }
    void OnHitChance()
    {
        // Func<bool> onHitChanceAction = null;

        if (onHitChanceAction != null)
        {
            foreach (var handler in onHitChanceAction.GetInvocationList())
            {
                bool result = ((Func<bool>)handler)();
                if (result)
                {
                    onHitChanceAction -= (Func<bool>)handler; // ����� true�̸� �ش� ������ ����
                }
            }
        }
    }
    void OnHitChanceDamage(float damage)
    {
        if (onHitDamageChanceAction != null)
        {
            foreach (var handler in onHitDamageChanceAction.GetInvocationList())
            {
                bool result = ((Func<float, bool>)handler)(damage);

                if (result)
                {
                    onHitDamageChanceAction -= (Func<float, bool>)handler; // ����� true�̸� �ش� ������ ����
                }
            }
        }
    }
}