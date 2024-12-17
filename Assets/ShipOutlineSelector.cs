using UnityEngine;

public class ShipOutlineSelector : MonoBehaviour
{
    public Outline outline;

    private void Awake()
    {
        Color color = Color.white;

        color.r = PlayerPrefs.GetFloat("Jetr", 1f);
        color.g = PlayerPrefs.GetFloat("Jetg", 1f);
        color.b = PlayerPrefs.GetFloat("Jetb", 1f);
        color.a = 1f;
        outline.OutlineColor = color;
    }
}
