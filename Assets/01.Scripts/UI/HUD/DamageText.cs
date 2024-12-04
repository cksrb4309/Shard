using System.Collections;
using TMPro;
using UnityEngine;

public class DamageText : MonoBehaviour
{
    public string damageTextName;

    public void Return()
    {
        PoolingManager.Instance.ReturnObject(damageTextName, transform.parent.gameObject);
    }


    //public TMP_Text text;
    //public string poolName;
    //Color startColor;
    //Color currentColor;
    //float alpha = 1f;

    //public void Awake()
    //{
    //    startColor = text.color;
    //}
    //private void OnEnable()
    //{
    //    text.color = startColor;

    //    currentColor = startColor;

    //    alpha = 1f;
    //}
    //private void FixedUpdate()
    //{
    //    transform.position += Vector3.up * Time.deltaTime;

    //    alpha -= Time.fixedDeltaTime;

    //    currentColor.a = alpha;

    //    text.color = currentColor;

    //    if (alpha < 0) PoolingManager.Instance.ReturnObject(poolName, gameObject);
    //}
}
