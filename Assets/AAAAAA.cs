using UnityEngine;

public class AAAAAA : MonoBehaviour
{
    public void Excute()
    {
        if (transform.parent != null)
        {
            BaseBlock block = transform.parent.GetComponent<BaseBlock>();

            if (block != null) block.isNearest = true;
        }
    }
}
