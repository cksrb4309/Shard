using UnityEngine;

public class AAAAAA : MonoBehaviour
{
    public void Excute()
    {
        BaseBlock block = transform.parent.GetComponent<BaseBlock>();

        if (block != null) block.isNearest = true;
    }
}
