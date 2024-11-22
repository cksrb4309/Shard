using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour
{
    public Image slotIcon;
    public TMP_Text countText;

    public void SetItemSlot(Sprite itemIcon, int count)
    {
        slotIcon.sprite = itemIcon;
        countText.text = count.ToString();
    }
}
