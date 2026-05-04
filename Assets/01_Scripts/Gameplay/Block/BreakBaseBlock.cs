using System.Collections;
using UnityEngine;

public class BreakBaseBlock : MonoBehaviour
{
    const string PoolName = "BreakBaseBlock";

    [SerializeField] GpuBreakBlockEffect breakEffect;

    Coroutine returnCoroutine;

    void Awake()
    {
        if (breakEffect == null)
            breakEffect = GetComponent<GpuBreakBlockEffect>();
    }

    public void Play(Vector3 worldPosition)
    {
        transform.position = worldPosition;

        if (breakEffect == null)
        {
            Debug.LogError("BreakBaseBlock requires GpuBreakBlockEffect on the same object.", this);
            return;
        }

        breakEffect.ResetEffect();
        breakEffect.PlayBreak();

        if (returnCoroutine != null)
            StopCoroutine(returnCoroutine);

        returnCoroutine = StartCoroutine(ReturnCoroutine());
    }

    void OnDisable()
    {
        if (returnCoroutine != null)
        {
            StopCoroutine(returnCoroutine);
            returnCoroutine = null;
        }
    }

    IEnumerator ReturnCoroutine()
    {
        yield return new WaitForSeconds(breakEffect.EffectDuration);

        returnCoroutine = null;
        PoolingManager.Instance.ReturnObject(PoolName, gameObject);
    }
}
