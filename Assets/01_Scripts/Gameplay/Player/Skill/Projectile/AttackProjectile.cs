using System.Collections;
using UnityEngine;

public class AttackProjectile : MonoBehaviour
{
    public string projectileName;

    public bool isPiercing = false;

    public HitEffectName hitEffectName; 

    float speed;

    bool isAttacked = false;

    AttackData attackData;
    Coroutine coroutine = null;
    public void SetAttackProjectile(AttackData attackData, float damage, float speed, float destroyDelay, Vector3 pos, Quaternion rotation)
    { 
        this.speed = speed;
        this.attackData = attackData;

        attackData.Value = damage;

        transform.position = pos;
        transform.rotation = rotation;

        isAttacked = false;

        coroutine = StartCoroutine(WaitReturnCoroutine(destroyDelay));
    }
    IEnumerator WaitReturnCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        Return();
    }
    private void FixedUpdate()
    {
        transform.position += transform.forward * Time.fixedDeltaTime * speed;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!isPiercing)
        {
            if (isAttacked) return;

            isAttacked = true;
        }

        // Physics Layer 설정을 통해 공격 할 수 있는 것만 충돌함
        IAttackable attackable = other.GetComponent<IAttackable>();

        if (attackable != null)
        {
            attackData.OnHit(attackable, null, false);

            ParticleManager.Play(transform.position, hitEffectName);

            if (!isPiercing)
            {
                Return();
            }
        }
    }
    private void Return()
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }
        PoolingManager.Instance.ReturnObject(projectileName, gameObject);

        AttackData.ReleaseAttackData(attackData);
    }
}
