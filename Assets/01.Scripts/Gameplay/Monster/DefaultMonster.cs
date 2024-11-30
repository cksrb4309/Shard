using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class DefaultMonster : Monster
{
    public float moveSpeed = 3f;
    public float updateInterval; // ��ǥ ��ġ ���� �ֱ�
    public float rotationSpeed = 360f; // ȸ�� �ӵ�

    public Transform myTransform;

    Vector3 moveDir = Vector3.zero;
    public override void Setting(float monsterHpMultiplier, float damageMultiplier)
    {
        base.Setting(monsterHpMultiplier, damageMultiplier);

        StartCoroutine(TrackingCoroutine());
    }
    public void FixedUpdate()
    {
        if (moveDir != Vector3.zero)
        {
            transform.position += moveDir * moveSpeed * Time.fixedDeltaTime;
            //rb.MovePosition(myTransform.position + moveDir * moveSpeed * Time.fixedDeltaTime);

            // ���� �ٶ󺸴� ������ ���������� �÷��̾� �������� ȸ��
            Quaternion targetRotation = Quaternion.LookRotation(moveDir, Vector3.up);
            myTransform.rotation = Quaternion.RotateTowards(myTransform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }
    IEnumerator TrackingCoroutine()
    {
        while (true)
        {
            moveDir = (GameManager.Instance.GetPlayerPosition(myTransform.position) - myTransform.position).normalized;

            yield return new WaitForSeconds(updateInterval);
        }
    }
}