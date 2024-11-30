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

    HashSet<Ability> selectedItems = new HashSet<Ability>();

    Ability selectAbility = null;

    // �г��� �� ���� true�� ��ȯ (��ư Ŭ���� ��� ���� ���� Ʈ���� ����)
    bool isCheck = false;
    
    // �����Ƽ �г��� ���� ���� �� True / �����Ƽ �г��� �����ٸ� False
    bool isSelect = false;

    int levelUpCount = 0;

    Coroutine coroutine = null;

    private void Awake()
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
                Debug.Log("IsSelectExcute");

                ShowSelectAbilityApply();
            }

            yield return null;
        }
        Debug.Log("IsSelect Exit");
        coroutine = null;
    }

    void ShowSelectAbilityApply()
    {
        if (isSelect)
        {
            Debug.Log("If IsSelect");
            if (coroutine == null)
            {
                Debug.Log("StartCoroutine");
                coroutine = StartCoroutine(AbilityGetDelayCoroutine());
            }

            Debug.Log("Return");
            return;
        }

        isSelect = true;

        for (int i = 0; i < selectGroups.Length; i++)
        {
            Ability ability;

            float randomValue = Random.value;

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
        }
        selectedItems.Clear();

        StartCoroutine(SelectCoroutine());
    }
    IEnumerator SelectCoroutine()
    {
        Time.timeScale = 0; // �ð��� �����

        panelGroup.SetActive(true); // �г��� ����

        for (int i = 0; i < selectGroups.Length; i++)
        {
            yield return new WaitForSecondsRealtime(0.5f);

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

        Debug.Log("LevelUpCount : " + levelUpCount.ToString());

        if (--levelUpCount == 0) Time.timeScale = 1;
    }
    public void SelectAbility(int index)
    {
        if (!isCheck) return;

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
}