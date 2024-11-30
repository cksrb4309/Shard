using System.Collections;
using UnityEngine;

public abstract class Teammate : MonoBehaviour
{
    public SearchMonster search;

    public float baseAttackDamage;
    public float baseAttackSpeed;
    public float patrolRange;
    public float searchRange;
    public float deadRange;
    public float rotateSpeed;
    public float patrolMoveSpeed;
    public float attackMoveSpeed;
    protected float attackSpeed;
    protected float attackDamage;
    protected Vector3 patrolPivot;
    protected IAttackable attackable = null;

    public virtual void Setting(Vector3 position)
    {
        patrolPivot = position;

        search.Setting(searchRange);

        Patrol();
    }
    protected void Patrol()
    {
        StopAllCoroutines();

        StartCoroutine(PatrolCoroutine());

        search.OnSearch();
    }
    protected IEnumerator PatrolCoroutine()
    {
        while (true)
        {
            Vector3 dir = (
                (new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized * 
                Random.Range(0f, 0.8f * patrolRange)) - transform.position
                ).normalized;

            Quaternion targetRotation = Quaternion.LookRotation(dir, Vector3.up);

            while ((patrolPivot - transform.position).magnitude > patrolRange)
            {
                if ((patrolPivot - transform.position).magnitude > deadRange)
                {
                    /*dir = (
                (new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized *
                Random.Range(0f, 0.8f * patrolRange)) - transform.position
                ).normalized;
                    targetRotation = Quaternion.LookRotation(dir, Vector3.up);

                    transform.position += dir * Time.deltaTime;*/
                    break;
                }
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 90f * Time.fixedDeltaTime);

                transform.position += dir * Time.deltaTime * patrolMoveSpeed;

                yield return null;
            }
            while ((patrolPivot - transform.position).magnitude <= patrolRange)
            {
                if ((patrolPivot - transform.position).magnitude > deadRange)
                {
                    /*dir = (
                (new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized *
                Random.Range(0f, 0.8f * patrolRange)) - transform.position
                ).normalized;
                    targetRotation = Quaternion.LookRotation(dir, Vector3.up);

                    transform.position += dir * Time.deltaTime;*/
                    break;
                }

                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 90f * Time.fixedDeltaTime);

                transform.position += dir * Time.deltaTime * patrolMoveSpeed;

                yield return null;
            }
            yield return null;
        }
    }
    public void Attack(IAttackable attackable)
    {
        StopAllCoroutines();

        search.OffSearch();

        this.attackable = attackable;

        StartCoroutine(AttackCoroutine());
    }
    protected IEnumerator AttackMoveCoroutine()
    {
        while (true)
        {
            Vector3 dir = (
                (new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized *
                Random.Range(0f, 0.8f * patrolRange)) - transform.position
                ).normalized;

            Quaternion targetRotation;

            while ((patrolPivot - transform.position).magnitude > patrolRange)
            {
                if ((patrolPivot - transform.position).magnitude > deadRange)
                {
                    /*dir = (
                (new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized *
                Random.Range(0f, 0.8f * patrolRange)) - transform.position
                ).normalized;

                    transform.position += dir * Time.deltaTime;*/
                    break;
                }


                transform.position += dir * Time.deltaTime * patrolMoveSpeed * attackMoveSpeed;

                targetRotation = Quaternion.LookRotation((attackable.GetPosition() - transform.position).normalized, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotateSpeed * Time.fixedDeltaTime);

                yield return null;

            }
            while ((patrolPivot - transform.position).magnitude < patrolRange)
            {
                if ((patrolPivot - transform.position).magnitude > deadRange)
                {
                    /*dir = (
                (new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized *
                Random.Range(0f, 0.8f * patrolRange)) - transform.position
                ).normalized;

                    targetRotation = Quaternion.LookRotation(dir, Vector3.up);

                    transform.position += dir * Time.deltaTime;*/
                    break;
                }
                transform.position += dir * Time.deltaTime * patrolMoveSpeed * attackMoveSpeed;

                targetRotation = Quaternion.LookRotation((attackable.GetPosition() - transform.position).normalized, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotateSpeed * Time.fixedDeltaTime);

                yield return null;
            }

            yield return null;
        }
    }
    protected abstract IEnumerator AttackCoroutine();
    public void AttackDamageUpgrade(int count)
    {
        attackDamage = baseAttackDamage * PlayerAttributes.Get(Attribute.AttackDamage);

        attackDamage *= count;
    }
    public void AttackSpeedUpgrade(int count)
    {
        attackSpeed = baseAttackSpeed;

        attackSpeed *= count;
    }
}