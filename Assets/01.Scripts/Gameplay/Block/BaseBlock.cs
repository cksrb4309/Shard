using UnityEngine;

public class BaseBlock : MonoBehaviour, IAttackable
{
    public float maxHp;
    public int reward = 5;
    float hp;
    Material material;
    private void Start()
    {
        hp = maxHp;
        material = GetComponent<MeshRenderer>().material;
    }
    public bool IsAlive()
    {
        return hp > 0;
    }
    public void ReceiveHit(float damage)
    {
        hp -= damage;

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
}
