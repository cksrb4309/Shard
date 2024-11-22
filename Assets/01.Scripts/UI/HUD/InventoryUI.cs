using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryUI : MonoBehaviour
{
    static InventoryUI instance = null;

    public GameObject inventoryPanel;

    public Transform itemSlotParent;

    public InputActionReference showInventoryAction;

    public TMP_Text energyCoreText;
    public TMP_Text soulShardText;

    Dictionary<string, ItemSlot> slots = new Dictionary<string, ItemSlot>();

    private void OnEnable()
    {
        showInventoryAction.action.Enable();
        showInventoryAction.action.started += show => inventoryPanel.SetActive(!inventoryPanel.activeSelf);
    }
    private void OnDisable()
    {
        showInventoryAction.action.started -= show => inventoryPanel.SetActive(!inventoryPanel.activeSelf);
        showInventoryAction.action.Disable();
    }
    private void Start()
    {
        instance = this;
    }
    public static void SetItem(Ability item, int count)
    {
        instance.SetItemApply(item, count);
    }
    public void SetItemApply(Ability item, int count)
    {
        ItemSlot slot;

        if (!slots.ContainsKey(item.abilityName))
        {
            slot = PoolingManager.Instance.GetObject<ItemSlot>("ItemSlot");
            slot.transform.parent = itemSlotParent;
        }
        else
        {
            slot = slots[item.abilityName];
        }

        slot.SetItemSlot(item.abilityIcon, 1);
    }
    public static void SetEnergyCoreText(int amount)
    {
        instance.SetEnergyCoreTextApply(amount);
    }
    public void SetEnergyCoreTextApply(int amount)
    {
        energyCoreText.text = "Energy Core: " + amount.ToString();
    }
    public static void SetSoulShardText(int amount)
    {
        instance.SetSoulShardTextApply(amount);
    }
    public void SetSoulShardTextApply(int amount)
    {
        soulShardText.text = "Soul Shard: " + amount.ToString();
    }
}
