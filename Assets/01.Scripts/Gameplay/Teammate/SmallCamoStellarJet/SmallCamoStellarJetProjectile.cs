using System.Collections;
using UnityEngine;

public class SmallCamoStellarJetProjectile : MonoBehaviour
{
    public string projectileName = "SmallCamoStellarJetProjectile";

    public float speed;
    public float returnDelay;

    float damage;
    Vector3 dir;
    public void Setting(float damage, Quaternion rotation)
    {
        this.damage = damage;

        transform.rotation = rotation;
    }
    private void OnEnable()
    {
        StartCoroutine(ReturnCoroutine());
    }
    IEnumerator ReturnCoroutine()
    {
        yield return new WaitForSeconds(returnDelay);

        PoolingManager.Instance.ReturnObject(projectileName, gameObject);
    }
    private void FixedUpdate()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
    }
    private void OnTriggerEnter(Collider other)
    {
        other.GetComponent<IAttackable>().ReceiveHit(damage);

        PoolingManager.Instance.ReturnObject(projectileName, gameObject);
    }
}
