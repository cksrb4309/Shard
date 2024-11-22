using UnityEngine;
using System.Collections;
using UnityEditor.Rendering;
public class TestMonster : Monster
{
    public TickEffect tickEffect;

    public float baseHp;
    public float baseDamage;

    float damage;

    public float moveSpeed = 3f;
    public float updateInterval; // 목표 위치 갱신 주기
    public float rotationSpeed = 360f; // 회전 속도

    Transform myTransfrom;

    Vector3 targetPosition = Vector3.zero;
    Vector3 moveDir = Vector3.zero;

    public override void Setting(float hpMultiplier, float damageMultiplier)
    {
        maxHp = baseHp * hpMultiplier;
        hp = maxHp;
        damage = baseDamage * damageMultiplier;

        myTransfrom = transform;

        StartCoroutine(TrackingCoroutine());
    }
    IEnumerator TrackingCoroutine()
    {
        while (true)
        {
            moveDir = (GameManager.Instance.GetPlayerPosition(myTransfrom.position) - myTransfrom.position).normalized;

            yield return new WaitForSeconds(updateInterval);
        }
    }
    public void FixedUpdate()
    {
        if (moveDir != Vector3.zero)
        {
            myTransfrom.position += moveDir * moveSpeed * Time.fixedDeltaTime;

            // 현재 바라보는 방향을 점진적으로 플레이어 방향으로 회전
            Quaternion targetRotation = Quaternion.LookRotation(moveDir, Vector3.up);
            myTransfrom.rotation = Quaternion.RotateTowards(myTransfrom.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }
    public void OnTriggerEnter(Collider other)
    {
        Debug.LogWarning("other Name: " + other.gameObject.name);

        other.GetComponentInParent<IDamageable>().TakeDebuff(damage, tickEffect);
    }
}
