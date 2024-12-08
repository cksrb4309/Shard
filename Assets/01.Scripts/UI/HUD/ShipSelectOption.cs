using UnityEngine;

[CreateAssetMenu(fileName = "ShipSelectOption", menuName = "UI/ShipSelectOption")]
public class ShipSelectOption : ScriptableObject
{
    public string shipName;

    [Multiline] public string shipDescriptionText;

    public Sprite shipImage;

    public int shipID;
}
