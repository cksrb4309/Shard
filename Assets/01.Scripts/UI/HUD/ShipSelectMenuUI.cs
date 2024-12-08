using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShipSelectMenuUI : MonoBehaviour
{
    public PlayModeSelectUI playModeSelectUI;

    public ShipSelectOption[] shipSelectOptions;

    public Image shipImage;
    public TMP_Text shipNameText;
    public TMP_Text shipDescriptionText;

    public int beforeShowShipId = 0;

    bool isSceneLoading = false;

    private void Start()
    {
        shipImage.sprite = shipSelectOptions[beforeShowShipId].shipImage;
        shipNameText.text = shipSelectOptions[beforeShowShipId].shipName;
        shipDescriptionText.text = shipSelectOptions[beforeShowShipId].shipDescriptionText;
    }

    public void SelectShip(int shipID)
    {
        if (shipID == beforeShowShipId) return;

        beforeShowShipId = shipID;

        shipImage.sprite = shipSelectOptions[shipID].shipImage;
        shipNameText.text = shipSelectOptions[shipID].shipName;
        shipDescriptionText.text = shipSelectOptions[shipID].shipDescriptionText;
    }

    public void Play()
    {
        if (isSceneLoading) return;

        isSceneLoading = true;

        playModeSelectUI.SetShipID(shipSelectOptions[beforeShowShipId].shipID);
    }
    public void Cancel()
    {
        if (isSceneLoading) return;

        playModeSelectUI.Cancel();

        transform.gameObject.SetActive(false);
    }
}
