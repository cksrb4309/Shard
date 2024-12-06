using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Michsky.UI.Shift
{
    [ExecuteInEditMode]
    public class UIElementSound : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler
    {
        public string pointerEnterSound = "UI_Pointer_Enter";
        public string pointerClickSound = "UI_Pointer_Click";

        public void OnPointerEnter(PointerEventData eventData)
        {
            SoundManager.Play(pointerEnterSound, SoundType.Effect);
        }
        public void OnPointerClick(PointerEventData eventData)
        {
            SoundManager.Play(pointerClickSound, SoundType.Effect);
        }
    }
}