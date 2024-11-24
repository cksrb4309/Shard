using UnityEngine;

public abstract class Monster : MonoBehaviour, IAttackable
{
    public string mobName;
    public float baseMaxHp;
    public int reward;
    protected float maxHp;
    protected float hp;
    public void ReceiveHit(float damage)
    {
        hp -= damage;

        GameManager.Instance.LastHitMonster = this;

        if (!IsAlive()) Dead();
    }
    public virtual void Dead()
    {
        RewardManager.MonsterDrop(reward);

        PoolingManager.Instance.ReturnObject(mobName, gameObject);
    }
    public bool IsAlive()
    {
        if (hp <= 0) return false;
        return true;
    }
    public abstract void Setting(float hp, float damage);
}