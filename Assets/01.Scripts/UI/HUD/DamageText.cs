using System.Collections;
using TMPro;
using UnityEngine;

public class DamageText : MonoBehaviour
{
    [SerializeField] TMP_Text label;

    public void SetText(string value)
    {
        label.SetText(value);
    }
}
