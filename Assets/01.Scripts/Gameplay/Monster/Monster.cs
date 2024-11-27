using UnityEngine;

public abstract class Monster : MonoBehaviour, IAttackable
{
    public string mobName;
    public float baseMaxHp;
    public int reward;
    protected float maxHp;
    protected float hp;

    Vector3 deadPosition;

    public void ReceiveHit(float damage)
    {
        hp -= damage;

        GameManager.SetLastHit(this);

        if (!IsAlive()) Dead();
    }
    public void Heal(float heal)
    {
        if (IsAlive())
            hp += heal;
    }
    public virtual void Dead()
    {
        RewardManager.MonsterDrop(reward);

        deadPosition = transform.position;

        PoolingManager.Instance.ReturnObject(mobName, gameObject);
    }
    public bool IsAlive()
    {
        if (hp <= 0) return false;
        return true;
    }
    public abstract void Setting(float hp, float damage);

    public Vector3 GetPosition()
    {
        if (IsAlive())
        {
            return transform.position;
        }
        return deadPosition;
    }
    public void ReceiveDebuff(StatusEffect effect, float damage)
    {
        ReceiveHit(damage);



        if (effect is StatEffect statEffect)
        {
            
        }
        else if (effect is TickEffect tickEffect)
        {

        }
    }
}