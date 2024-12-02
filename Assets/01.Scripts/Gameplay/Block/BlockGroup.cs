using System.Collections;
using UnityEngine;

public class BlockGroup : MonoBehaviour
{
    public BaseBlock[] blocks;
    public void StageClear()
    {
        StartCoroutine(StageClearCoroutine());
    }
    IEnumerator StageClearCoroutine()
    {
        BaseBlock.isSetting = true;

        float t = 0;

        int beforeCount = 0;
        int count = 0;

        while (count <= 7569)
        {
            yield return null;

            // t [0 ~ 1]
            t += Time.deltaTime * 3f;

            count = (int)(t * 7569);

            for (; beforeCount <= count && beforeCount != 7570; beforeCount++)
            {
                blocks[beforeCount].ReSetting();
            }
        }

        /*
        for (int i = 0; i < blocks.Length; i++)
        {
            yield return new WaitForSeconds(0.001f);
            blocks[i].ReSetting();
        }*/

        BaseBlock.isSetting = false;
    }
}
