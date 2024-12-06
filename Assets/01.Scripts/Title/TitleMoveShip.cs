using System.Collections;
using UnityEngine;

public class TitleMoveShip : MonoBehaviour
{
    public Transform[] startPositions;
    public Transform[] endPositions;

    public Vector2 moveSpeedRange;

    public Vector2 delayRange;

    public float attackDelay;

    public Transform firePosition;

    public Transform rotateTransform;

    public ParticleSystem attackParticle;

    bool isMove = false;

    bool isCooltime = false;

    TitleBlock target = null;

    Vector3 dir;

    public void Start()
    {
        StartCoroutine(WhileMoveCoroutine());
        StartCoroutine(AttackCoroutine());
        StartCoroutine(RotataCoroutine());
    }
    IEnumerator TraceTarget(TitleBlock t)
    {
        target = t;

        while (true)
        {
            yield return null;

            if (target == null) break;

            rotateTransform.LookAt(target.transform);
        }
    }
    IEnumerator RotataCoroutine()
    {
        while (true)
        {
            if (target != null) if (!target.IsAlive()) target = null;

            if (target == null)
            {
                Quaternion targetRotation = Quaternion.LookRotation(dir == Vector3.zero ? transform.forward : dir);

                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 45f * Time.fixedDeltaTime);
            }
            else
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, rotateTransform.rotation, 45f * Time.fixedDeltaTime);
            }

            yield return null;
        }
    }
    IEnumerator WhileMoveCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(delayRange.x, delayRange.y));

            isMove = true;

            float t = 0;

            float moveSpeed = Random.Range(moveSpeedRange.x, moveSpeedRange.y);

            Transform startPosition;
            Transform endPosition;

            if (0.5f > Random.value)
            {
                startPosition = startPositions[Random.Range(0, startPositions.Length)];
                endPosition = endPositions[Random.Range(0, endPositions.Length)];
            }
            else
            {
                startPosition = endPositions[Random.Range(0, endPositions.Length)];
                endPosition = startPositions[Random.Range(0, startPositions.Length)];
            }

            transform.position = startPosition.position;

            transform.LookAt(endPosition.position);

            dir = transform.forward;

            while (t < 1f)
            {
                t += Time.deltaTime * moveSpeed;

                transform.position = Vector3.Lerp(startPosition.position, endPosition.position, t);

                yield return null;
            }

            target = null;

            isMove = false;
        }
    }
    IEnumerator AttackCoroutine()
    {
        while (true)
        {
            while (!isCooltime)
            {
                if (isMove && target != null)
                {
                    yield return new WaitForSeconds(0.5f);

                    attackParticle.Play();

                    isCooltime = true;

                    GameObject bullet = PoolingManager.Instance.GetObject("TitleBullet");

                    bullet.transform.position = firePosition.position;
                    bullet.transform.rotation = firePosition.rotation;

                    StartCoroutine(CooltimeCoroutine());
                }
                yield return null;
            }
            yield return null;
        }
    }
    IEnumerator CooltimeCoroutine()
    {
        yield return new WaitForSeconds(attackDelay);

        isCooltime = false;
    }
    private void OnTriggerEnter(Collider other)
    {
        StartCoroutine(TraceTarget(other.GetComponent<TitleBlock>()));
    }
}