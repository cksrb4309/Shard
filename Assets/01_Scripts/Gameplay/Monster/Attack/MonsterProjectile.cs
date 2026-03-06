using System.Collections;
using UnityEngine;

public class MonsterProjectile : MonoBehaviour
{
    public string projectileName;
    float damage = 0;
    float speed = 0;
    public void Setting(float damage, float speed, float duration)
    {
        this.damage = damage;
        this.speed = speed;

        StartCoroutine(ReturnCoroutine(duration));
    }
    public void FixedUpdate()
    {
        transform.position += transform.forward * speed * Time.fixedDeltaTime;
    }
    private void OnTriggerEnter(Collider other)
    {
        other.GetComponent<IDamageable>().TakeDamage(damage);

        PoolingManager.Instance.ReturnObject(projectileName, gameObject);
    }
    IEnumerator ReturnCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        PoolingManager.Instance.ReturnObject(projectileName, gameObject);
    }
}
