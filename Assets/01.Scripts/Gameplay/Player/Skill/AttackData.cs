using System;
using UnityEngine;

public class AttackData
{
    public Action<AttackData> onKillAction = null; // óġ���� �� ������ Action
    public Action<AttackData> onHitAction = null; // ������ ������ ������ Action
    public Action<AttackData> onCriticalAction = null; // ũ��Ƽ�� ������ ������ ������ Action
    public Action<AttackData, float> onHitDamageAction = null; // ������ ������ Damage �� �ް� ������ Action
    public Func<AttackData, bool> onHitChanceAction = null; // ������ �� ���� Ȯ���� �ѹ��� ������ Func
    public Func<AttackData, float, bool> onHitDamageChanceAction = null; // ������ �� ���� Ȯ���� Damage �� �ް� ������ Func

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

                if (result) // ���� �ش� ������ ���� ������ ��
                {
                    // ���� ������ �ƴ� �� �ڽ��� �ش� ���ݿ� ���� �Լ��� ����
                    if (!isMainAttack) onHitChanceAction -= (Func<AttackData, bool>)handler;

                    // �ش� ���ݿ� ���� �ѱ� AttackData ���� �Ȱ��� ������ ���� ���� �Լ��� �����Ѵ�
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

                if (result) // ���� �ش� ������ ���� ������ ��
                {
                    // ���� ������ �ƴ� �� �ڽ��� �ش� ���ݿ� ���� �Լ��� ����
                    if (!isMainAttack) onHitDamageChanceAction -= (Func<AttackData, float, bool>)handler;

                    // �ش� ���ݿ� ���� �ѱ� AttackData ���� �Ȱ��� ������ ���� ���� �Լ��� �����Ѵ�
                    temp.onHitDamageChanceAction -= (Func<AttackData, float, bool>)handler;
                }
            }
        }
    }
}