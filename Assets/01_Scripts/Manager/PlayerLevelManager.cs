using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerLevelManager : MonoBehaviour
{
    static PlayerLevelManager instance = null;

    public TMP_Text levelText;
    public Slider experienceSlider;

    float currentXP = 0;
    float xpToNextLevel = 100f;
    int currentLevel = 1;

    float CurrentXP 
    {
        get
        {
            return currentXP;
        }
        set
        {
            currentXP = value;

            experienceSlider.value = currentXP;
        }
    }

    float XPToNextLevel
    {
        get
        {
            return xpToNextLevel;
        }
        set
        {
            xpToNextLevel = value;

            experienceSlider.maxValue = xpToNextLevel;
        }
    }

    int CurrentLevel
    {
        get
        {
            return currentLevel;
        }
        set
        {
            currentLevel = value;

            levelText.text = "LV : " + currentLevel.ToString();
        }
    }

    private void Awake()
    {
        instance = this;

        experienceSlider.value = currentXP;
        experienceSlider.maxValue = xpToNextLevel;

        levelText.text = "LV : 1";
    }

    public static void AddExperience(float xp)
    {
        instance.AddExperienceApply(xp);
    }
    public void AddExperienceApply(float xp)
    {
        CurrentXP += xp;

        while (CurrentXP > XPToNextLevel)
        {
            CurrentXP -= XPToNextLevel;

            LevelUp();
        }
    }
    public void LevelUp()
    {
        CurrentLevel++;

        XPToNextLevel *= 1.2f;

        PlayerAttributes.LevepUp();

        AbilitySelector.ShowSelectAbility();
    }
}
