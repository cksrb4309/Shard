using UnityEngine;

// 공격이 명중했을 때 확률적으로 발생시키는 아이템의 Interface
public interface IOnHitChance : ICondition
{
    public bool OnHit(AttackData attackData);
}