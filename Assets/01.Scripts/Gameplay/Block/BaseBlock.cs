using System.Collections.Generic;
using UnityEngine;

public class BaseBlock : MonoBehaviour, IAttackable
{
    public Transform coreTransform = null;

    public float maxHp = 1000f;
    public float minMaxHp = 1000f;
    public float maxMaxHp = 10000f;
    public float maxLength = 80f;
    public float minLength = 9f;
    public int reward = 5;
    public int minReward = 5;
    public int maxReward = 30;

    float hp;

    Material material;

    List<(string, Coroutine)> statusEffectList = new List<(string, Coroutine)>();

    Vector3 pos;

    GameObject blockObj;

    Collider cd;
    private void Start()
    {
        pos = transform.position + new Vector3(0.39194f, 0, 0);
        hp = maxHp;
        blockObj = transform.GetChild(0).gameObject;
        material = blockObj.GetComponent<MeshRenderer>().material;
        cd = GetComponent<Collider>();
        //Debug.Log("개수확인"); 7569
    }
    public bool IsAlive()
    {
        return hp > 0;
    }
    public void ReceiveHit(float damage)
    {
        if (!IsAlive()) return;

        hp -= damage;

        DamageTextController.OnDamageText(pos, damage);

        GameManager.SetLastHit(this);

        if (!IsAlive())
        {
            Dead();
        }
        else
        {
            material.SetFloat("_DamageLevel", 1f - hp / maxHp);
        }
    }
    void Dead()
    {
        PoolingManager.Instance.GetObject("BreakBaseBlock").transform.position = pos;

        RewardManager.BaseBlockDrop(reward);

        blockObj.SetActive(false);

        cd.enabled = false;

        material.SetFloat("_DamageLevel", 0);
    }

    public Vector3 GetPosition()
    {
        return pos;
    }

    public void ReceiveDebuff(StatusEffect effect, float damage)
    {
        ReceiveHit(damage);
    }
    public void Setting()
    {
        Vector3 p = transform.position + new Vector3(0.39194f, 0, 0);

        p.Set(p.x, p.y, p.z * 1.9f);

        float length = (p - coreTransform.position).magnitude;

        maxHp = Mathf.Lerp(minMaxHp, maxMaxHp, Mathf.InverseLerp(minLength, maxLength, length));
        reward = (int)Mathf.Lerp(minReward, maxReward, Mathf.InverseLerp(minLength, maxLength, length));
    }
}