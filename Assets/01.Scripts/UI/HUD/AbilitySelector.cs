using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine;

public class AbilitySelector : MonoBehaviour
{
    static AbilitySelector instance = null;

    public SelectGroup[] selectGroups;

    public GameObject panelGroup;

    public Ability[] tear_1_Ability;
    public Ability[] tear_2_Ability;
    public Ability[] tear_3_Ability;

    public Color[] tearColors;
    public string[] tearShowNames;
    HashSet<Ability> selectedItems = new HashSet<Ability>();

    Ability selectAbility = null;

    // 패널을 다 띄우고 true로 전환 (버튼 클릭을 잠시 막기 위한 트리거 변수)
    bool isCheck = false;
    
    // 어빌리티 패널을 보는 중일 땐 True / 어빌리티 패널이 꺼졌다면 False
    bool isSelect = false;

    int levelUpCount = 0;

    Coroutine coroutine = null;

    private void Start()
    {
        instance = this;
    }

    public static void ShowSelectAbility()
    {
        instance.levelUpCount++;

        instance.ShowSelectAbilityApply();
    }
    IEnumerator AbilityGetDelayCoroutine()
    {
        while (levelUpCount > 0)
        {
            if (!isSelect)
            {
                ShowSelectAbilityApply();
            }

            yield return null;
        }
        coroutine = null;
    }

    void ShowSelectAbilityApply()
    {
        if (isSelect)
        {
            if (coroutine == null)
            {
                coroutine = StartCoroutine(AbilityGetDelayCoroutine());
            }

            return;
        }

        SoundManager.Play("LevelUpSound");

        isSelect = true;

        for (int i = 0; i < selectGroups.Length; i++)
        {
            Ability ability;

            float randomValue = LuckManager.GetValue();

            if (randomValue > 0.95f) ability = tear_3_Ability[Random.Range(0, tear_3_Ability.Length)];
            else if (randomValue > 0.65f) ability = tear_2_Ability[Random.Range(0, tear_2_Ability.Length)];
            else ability = tear_1_Ability[Random.Range(0, tear_1_Ability.Length)];

            if (!selectedItems.Add(ability))
            {
                i--;

                continue;
            }
            selectGroups[i].ability = ability;
            selectGroups[i].abilityNameText.text = ability.abilityName;
            selectGroups[i].abilityTearText.text = tearShowNames[ability.tear-1];
            selectGroups[i].abilityTearText.color = tearColors[ability.tear - 1];
        }
        selectedItems.Clear();

        StartCoroutine(SelectCoroutine());
    }
    IEnumerator SelectCoroutine()
    {
        Time.timeScale = 0; // 시간을 멈춘다

        panelGroup.SetActive(true); // 패널을 띄운다

        for (int i = 0; i < selectGroups.Length; i++)
        {
            yield return new WaitForSecondsRealtime(0.25f);

            selectGroups[i].gameObject.SetActive(true);
        }
        isCheck = true;

        while (selectAbility == null) yield return null;

        for (int i = 0; i < selectGroups.Length; i++)
        {
            selectGroups[i].gameObject.SetActive(false);
        }
        panelGroup.SetActive(false);

        Inventory.GetAbility(selectAbility);

        selectAbility = null;

        isSelect = false;

        if (--levelUpCount == 0) Time.timeScale = 1;
    }
    public void SelectAbility(int index)
    {
        if (!isCheck) return;

        SoundManager.Play("AbilitySelectSFX");

        isCheck = false;

        selectAbility = selectGroups[index].ability;
    }
}

[System.Serializable]
public class SelectGroup
{
    [HideInInspector] public Ability ability;
    public GameObject gameObject;
    public TMP_Text abilityNameText;
    public TMP_Text abilityTearText;
}