using System.Collections;
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
            GameObject breakBlock = PoolingManager.Instance.GetObject("BreakBaseBlock");

            breakBlock.transform.position = transform.position + new Vector3(0.39194f, 0, 0);

            StartCoroutine(ReturnObjectCoroutine(breakBlock));

            meshRenderer.enabled = false;

            cd.enabled = false;

            PoolingManager.Instance.ReturnObject("TitleBlock", gameObject);
        }
    }
    public bool IsAlive()
    {
        return hp > 0;
    }
    IEnumerator ReturnObjectCoroutine(GameObject obj)
    {
        yield return new WaitForSeconds(1f);

        PoolingManager.Instance.ReturnObject("BreakBaseBlock", obj);

        PoolingManager.Instance.ReturnObject("TitleBlock", gameObject);
    }
}
