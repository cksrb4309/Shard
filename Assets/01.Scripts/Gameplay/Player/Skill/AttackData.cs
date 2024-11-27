using System;
using UnityEngine;

public class AttackData
{
    public Action<AttackData> onKillAction = null; // 처치했을 때 적용할 Action
    public Action<AttackData> onHitAction = null; // 명중할 때마다 적용할 Action
    public Action<AttackData> onCriticalAction = null; // 크리티컬 명중할 때마다 적용할 Action
    public Action<AttackData, float> onHitDamageAction = null; // 명중할 때마다 Damage 값 받고 적용할 Action
    public Func<AttackData, bool> onHitChanceAction = null; // 명중할 때 일정 확률로 한번만 적용할 Func
    public Func<AttackData, float, bool> onHitDamageChanceAction = null; // 명중할 때 일정 확률로 Damage 값 받고 적용할 Func

    public Vector3 position = Vector3.zero;
    public Vector3 rotation = Vector3.zero;

    bool isMainAttack;
    public AttackData(bool isMainAttack = false)
    {
        onKillAction = null;
        onHitAction = null;
        onCriticalAction = null;
        onHitDamageAction = null;
        onHitChanceAction = null;
        onHitDamageChanceAction = null;

        this.isMainAttack = isMainAttack;
    }
    public AttackData(AttackData attackData)
    {
        onKillAction = (Action<AttackData>)attackData.onKillAction?.Clone();
        onHitAction = (Action<AttackData>)attackData.onHitAction?.Clone();
        onCriticalAction = (Action<AttackData>)attackData.onCriticalAction?.Clone();
        onHitDamageAction = (Action<AttackData, float>)attackData.onHitDamageAction?.Clone();
        onHitChanceAction = (Func<AttackData, bool>)attackData.onHitChanceAction?.Clone();
        onHitDamageChanceAction = (Func<AttackData, float, bool>)attackData.onHitDamageChanceAction?.Clone();

        position = attackData.position;
        rotation = attackData.rotation;

        isMainAttack = false;
    }
    public void OnKill()
    {
        onKillAction?.Invoke(this);
    }
    public void OnHit(float damage, Vector3 position, Vector3 rotation)
    {
        this.position = position;
        this.rotation = rotation;

        OnHit();
        OnHitDamage(damage);
        OnHitChance();
        OnHitChanceDamage(damage);
    }
    public void OnCritical()
    {
        onCriticalAction?.Invoke(new AttackData(this));
    }
    void OnHit()
    {
        onHitAction?.Invoke(new AttackData(this));
    }
    void OnHitDamage(float damage)
    {
        onHitDamageAction?.Invoke(new AttackData(this), damage);
    }
    void OnHitChance()
    {
        if (onHitChanceAction != null)
        {
            foreach (var handler in onHitChanceAction.GetInvocationList())
            {
                AttackData temp = new AttackData(this);
                bool result = ((Func<AttackData, bool>)handler)(temp);

                if (result) // 만약 해당 공격을 성공 시켰을 때
                {
                    // 메인 공격이 아닐 때 자신의 해당 공격에 대한 함수를 제거
                    if (!isMainAttack) onHitChanceAction -= (Func<AttackData, bool>)handler;

                    // 해당 공격에 새로 넘긴 AttackData 또한 똑같은 공격을 막기 위해 함수를 제거한다
                    temp.onHitChanceAction -= (Func<AttackData, bool>)handler;
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
                AttackData temp = new AttackData(this);

                bool result = ((Func<AttackData, float, bool>)handler)(temp, damage);

                if (result) // 만약 해당 공격을 성공 시켰을 때
                {
                    // 메인 공격이 아닐 때 자신의 해당 공격에 대한 함수를 제거
                    if (!isMainAttack) onHitDamageChanceAction -= (Func<AttackData, float, bool>)handler;

                    // 해당 공격에 새로 넘긴 AttackData 또한 똑같은 공격을 막기 위해 함수를 제거한다
                    temp.onHitDamageChanceAction -= (Func<AttackData, float, bool>)handler;
                }
            }
        }
    }
}