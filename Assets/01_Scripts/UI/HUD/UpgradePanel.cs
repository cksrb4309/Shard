using TMPro;
using UnityEngine;

public class UpgradePanel : MonoBehaviour
{
    public CoreInteractUI coreInteractUI;

    public TMP_Text abilityNameTextUI;
    public TMP_Text levelUpCountTextUI;

    AbilityAndText abilityAndText = new AbilityAndText();

    Ability ability = null;

    public void Setting(Ability ability)
    {
        this.ability = ability;

        abilityNameTextUI.text = ability.abilityName;

        abilityAndText.ability = ability;
        abilityAndText.text = levelUpCountTextUI;
    }
    public void UpgradeExcute()
    {
        coreInteractUI.UpgradeUserStat(abilityAndText);
    }
}
[System.Serializable]
public class AbilityAndText
{
    public Ability ability = null;
    public TMP_Text text = null;
}