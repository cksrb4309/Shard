using TMPro;
using UnityEngine;

public sealed class UnityCliUiSmokePager : MonoBehaviour
{
    [SerializeField] private GameObject[] pages;
    [SerializeField] private TMP_Text previousLabel;
    [SerializeField] private TMP_Text nextLabel;
    [SerializeField] private string previousText = "<";
    [SerializeField] private string nextText = ">";
    [SerializeField] private string closeText = "X";

    private int pageIndex;

    private void OnEnable()
    {
        pageIndex = Mathf.Clamp(pageIndex, 0, Mathf.Max(0, PageCount - 1));
        Refresh();
    }

    public void Previous()
    {
        pageIndex = Mathf.Max(0, pageIndex - 1);
        Refresh();
    }

    public void NextOrClose()
    {
        if (PageCount == 0)
        {
            gameObject.SetActive(false);
            return;
        }

        if (pageIndex >= PageCount - 1)
        {
            gameObject.SetActive(false);
            return;
        }

        pageIndex++;
        Refresh();
    }

    public void Refresh()
    {
        if (pages != null)
        {
            for (var i = 0; i < pages.Length; i++)
            {
                if (pages[i] != null)
                {
                    pages[i].SetActive(i == pageIndex);
                }
            }
        }

        if (previousLabel != null)
        {
            previousLabel.text = previousText;
        }

        if (nextLabel != null)
        {
            nextLabel.text = PageCount > 0 && pageIndex >= PageCount - 1 ? closeText : nextText;
        }
    }

    private int PageCount
    {
        get { return pages == null ? 0 : pages.Length; }
    }
}
