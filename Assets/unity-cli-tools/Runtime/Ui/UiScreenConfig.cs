using UnityEngine;

namespace UnityCliTools.Runtime.Ui
{

[CreateAssetMenu(menuName = "Unity CLI Tools/UI Screen Config", fileName = "UiScreenConfig")]
public sealed class UiScreenConfig : ScriptableObject
{
    public string title = "Title";
    [TextArea] public string[] pageTexts = new string[0];
    public string previousText = "<";
    public string nextText = ">";
    public string closeText = "X";
    public Vector4 panelMargins = new Vector4(32f, 32f, 32f, 32f);
    public Vector2 panelMinSize = new Vector2(280f, 360f);
    public Vector2 panelMaxSize = new Vector2(1320f, 900f);
}
}
