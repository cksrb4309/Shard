using UnityEngine;

public class CoreRotateAnimation : MonoBehaviour
{
    public float ratateSpeed = 5f;
    public Material material;

    private void Start()
    {
        if (material != null)
            material.SetFloat("_DamageLevel", 0);
    }

    private void FixedUpdate()
    {
        transform.Rotate(Vector3.forward * ratateSpeed * Time.fixedDeltaTime);
    }
}
