using System;
using UnityEngine;

public class AttackData
{
    public Action onKillAction = null; // 처치했을 때 적용할 Action
    public Action onHitAction = null; // 명중할 때마다 적용할 Action
    public Action onCriticalAction = null; // 크리티컬 명중할 때마다 적용할 Action
    public Action<float> onHitDamageAction = null; // 명중할 때마다 Damage 값 받고 적용할 Action
    public Func<bool> onHitChanceAction = null; // 명중할 때 일정 확률로 한번만 적용할 Func
    public Func<float, bool> onHitDamageChanceAction = null; // 명중할 때 일정 확률로 Damage 값 받고 적용할 Func

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
                    onHitChanceAction -= (Func<bool>)handler; // 결과가 true이면 해당 구독을 제거
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
                    onHitDamageChanceAction -= (Func<float, bool>)handler; // 결과가 true이면 해당 구독을 제거
                }
            }
        }
    }
}