using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class StatusUI : MonoBehaviour
{
    public static StatusUI Instance { get; private set; }

    [SerializeField] InputActionReference statusOpenAction;
    [SerializeField] GameObject panel;

    [SerializeField] TMP_Text attackDamageAmount;
    [SerializeField] TMP_Text attackSpeedAmount;
    [SerializeField] TMP_Text criticalChanceAmount;
    [SerializeField] TMP_Text criticalDamageAmount;
    [SerializeField] TMP_Text moveSpeedAmount;
    [SerializeField] TMP_Text healthRegenAmount;
    [SerializeField] TMP_Text luckAmount;

    public void Awake()
    {
        Instance = this;
    }
    public void Update()
    {
        if (statusOpenAction.action.WasPressedThisFrame())
        {
            Toggle();
        }
    }
    public void Toggle()
    {
        bool isActive = panel.activeSelf;

        if (isActive)
            panel.SetActive(false);
        else
            panel.SetActive(true);
        
    }
    public void UpdateAttribute(Attribute attribute)
    {
        float value = PlayerAttributes.Instance.GetAttribute(attribute);
        switch (attribute)
        {
            case Attribute.AttackDamage:
                attackDamageAmount.text = value.ToString("F0");
                break;

            case Attribute.AttackSpeed:
                attackSpeedAmount.text = value.ToString("F2");
                break;

            case Attribute.FlatCriticalChance:
            case Attribute.RateCriticalChance:

                value = PlayerAttributes.Instance.GetAttribute(Attribute.FlatCriticalChance) *
                        PlayerAttributes.Instance.GetAttribute(Attribute.RateCriticalChance);

                criticalChanceAmount.text = (value * 100f).ToString("F0") + "%"; break;

            case Attribute.CriticalDamage:
                criticalDamageAmount.text = (value * 100f).ToString("F0") + "%"; break;

            case Attribute.MoveSpeed:
                moveSpeedAmount.text = (value * 70f).ToString("F0"); break;

            case Attribute.HealthRegenRate:
                healthRegenAmount.text = value.ToString() + " / Sec"; break;

            case Attribute.Luck:
                luckAmount.text = value.ToString(); break;
        }
    }
}
