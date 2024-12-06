using TMPro;
using UnityEngine;

public class UpgradePanel : MonoBehaviour
{
    public CoreInteractUI coreInteractUI;

    public TMP_Text abilityNameTextUI;

    Ability ability = null;

    public void Setting(Ability ability)
    {
        this.ability = ability;

        abilityNameTextUI.text = ability.abilityName;
    }
    public void UpgradeExcute()
    {
        coreInteractUI.UpgradeUserStat(ability);
    }
}