using UnityEngine;

// ������ �������� �� Ȯ�������� �߻���Ű�� �������� Interface
public interface IOnHitChance : ICondition
{
    public bool OnHit(AttackData attackData);
}