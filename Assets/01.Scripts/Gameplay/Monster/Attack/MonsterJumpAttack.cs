using System.Collections;
using UnityEngine;

public class MonsterJumpAttack : MonsterAttack
{
    public CustomMonster monster;

    public LayerMask layerMask;

    public float searchRange;
    public float attackRange;

    public float cooltimeDelay;

    public bool isBoss = false;

    bool isAttack = false;
    bool isCooltime = false;
    public override void StartAttack()
    {
        isAttack = true;

        isCooltime = false;
    }

    public override void StopAttack()
    {
        isAttack = false;
    }
    private void FixedUpdate()
    {
        if (!isAttack) return;
        if (isCooltime) return;

        if (Physics.CheckSphere(transform.position, searchRange, layerMask))
        {
            isCooltime = true;
            StartCoroutine(CooltimeCoroutine());

            Debug.Log("attackRange " + attackRange.ToString());

            monster.TriggerJumpAttack(attackRange, isBoss);
        }
    }
    IEnumerator CooltimeCoroutine()
    {
        yield return new WaitForSeconds(cooltimeDelay);

        isCooltime = false;
    }
}
