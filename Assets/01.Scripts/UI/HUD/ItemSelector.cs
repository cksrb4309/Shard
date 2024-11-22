using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ItemSelector : MonoBehaviour
{
    static ItemSelector instance = null;

    public SelectGroup[] selectGroups;

    public Ability[] itemList;

    HashSet<Ability> selectedItems = new HashSet<Ability>();

    private void Awake()
    {
        instance = this;
    }

    public static void ShowSelectItem()
    {
        instance.ShowSelectItemApply();
    }
    public void ShowSelectItemApply()
    {
        for (int i = 0; i < selectGroups.Length; i++)
        {
            Ability item = itemList[Random.Range(0, itemList.Length)];

            if (!selectedItems.Add(item))
            {
                i--;
                continue;
            }
            selectGroups[i].gameObject.SetActive(true);
            selectGroups[i].item = item;
            selectGroups[i].itemNameText.text = item.abilityName;
            selectGroups[i].explainText.text = item.explain;
        }

        selectedItems.Clear();
    }
}

[System.Serializable]
public class SelectGroup
{
    [HideInInspector] public Ability item;
    public GameObject gameObject;
    public TMP_Text itemNameText;
    public TMP_Text explainText;
}