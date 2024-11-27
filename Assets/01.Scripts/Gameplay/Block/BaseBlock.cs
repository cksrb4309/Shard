using System.Collections.Generic;
using UnityEngine;

public class BaseBlock : MonoBehaviour, IAttackable
{
    public float maxHp;
    public int reward = 5;
    float hp;
    Material material;
    List<(string, Coroutine)> statusEffectList = new List<(string, Coroutine)>();


    Vector3 pos;

    private void Start()
    {
        pos = transform.position;
        hp = maxHp;
        material = GetComponent<MeshRenderer>().material;
    }
    public bool IsAlive()
    {
        return hp > 0;
    }
    public void ReceiveHit(float damage)
    {
        if (!IsAlive()) return;

        hp -= damage;

        GameManager.SetLastHit(this);

        if (!IsAlive())
        {
            PoolingManager.Instance.GetObject("BreakBaseBlock").transform.position = transform.position;

            RewardManager.BaseBlockDrop(reward);

            gameObject.SetActive(false);
        }
        else
        {
            material.SetFloat("_DamageLevel", 1f - hp / maxHp);
        }
    }

    public Vector3 GetPosition()
    {
        return pos;
    }

    public void ReceiveDebuff(StatusEffect effect, float damage)
    {
        ReceiveHit(damage);
    }
}