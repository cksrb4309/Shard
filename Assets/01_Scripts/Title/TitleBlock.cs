using UnityEngine;

public class TitleBlock : MonoBehaviour
{
    public MeshRenderer meshRenderer;
    public float maxHp;

    public Collider cd;

    Material material = null;
    float hp = 0;

    private void OnEnable()
    {
        hp = maxHp;

        if (material == null) material = meshRenderer.material;

        meshRenderer.enabled = true;

        cd.enabled = true;

        material.SetFloat("_DamageLevel", 0);
    }

    public void ReceiveDamage(float damage)
    {
        hp -= damage;

        material.SetFloat("_DamageLevel", 1 - (hp / maxHp));

        if (hp <= 0)
        {
            BreakBaseBlock breakBlock = PoolingManager.Instance.GetObject<BreakBaseBlock>("BreakBaseBlock");
            if (breakBlock != null)
                breakBlock.Play(transform.position + new Vector3(0.39194f, 0, 0));

            meshRenderer.enabled = false;

            cd.enabled = false;

            PoolingManager.Instance.ReturnObject("TitleBlock", gameObject);
        }
    }
    public bool IsAlive()
    {
        return hp > 0;
    }
}
