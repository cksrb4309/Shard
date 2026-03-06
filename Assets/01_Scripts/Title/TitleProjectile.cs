using UnityEngine;
using static UnityEngine.ParticleSystem;

public class TitleProjectile : MonoBehaviour
{
    public float moveSpeed;
    public ParticleSystem particle;
    public void OnTriggerEnter(Collider other)
    {
        other.GetComponent<TitleBlock>().ReceiveDamage(10f);

        particle.transform.SetParent(null);

        particle.transform.position = transform.position;

        particle.Play();

        PoolingManager.Instance.ReturnObject("TitleBullet", gameObject);
    }

    private void FixedUpdate()
    {
        transform.position += transform.forward * moveSpeed * Time.fixedDeltaTime;
    }
}
