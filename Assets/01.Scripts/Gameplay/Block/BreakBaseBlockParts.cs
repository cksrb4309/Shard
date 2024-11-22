using UnityEngine;

public class BreakBaseBlockParts : MonoBehaviour
{
    public Vector3 dir = Vector3.zero;
    public float speed;
    Vector3 startPos;
    Vector3 startRot;
    Vector3 startScale;
    Vector3 randomRot;
    Vector3 shrinkRate = new Vector3(70.7f, 70.7f, 100f);
    private void Awake()
    {
        startPos = transform.localPosition;
        startRot = transform.localRotation.eulerAngles;
        startScale = transform.localScale;
    }

    private void OnEnable()
    {
        transform.localPosition = startPos;
        transform.localRotation = Quaternion.Euler(startRot);
        transform.localScale = startScale;
        randomRot = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
    }

    private void FixedUpdate()
    {
        transform.position += dir * Time.fixedDeltaTime;
        transform.Rotate(randomRot * Time.fixedDeltaTime * 180f);

        transform.localScale -= shrinkRate * Time.fixedDeltaTime;
        transform.localScale = Vector3.Max(transform.localScale, Vector3.zero);
    }
}
