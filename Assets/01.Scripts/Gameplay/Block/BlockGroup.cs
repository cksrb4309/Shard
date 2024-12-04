using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockGroup : MonoBehaviour
{
    public BaseBlock[] blocks;

    public List<BaseBlock> isNearBlockList = new List<BaseBlock>();
    public List<BaseBlock> otherBlockList = new List<BaseBlock>();

    public BlockSettingEffect blockSettingEffect;
    public bool isCheck = false;
    public float speed = 1f;

    public float t = 0;
    public int beforeCount = 0;
    public int count = 0;
    public float blockCount;


    private void Update()
    {
        if (isCheck == true)
        {
            isCheck = false;
            StartCoroutine(StageClearCoroutine());
        }
    }

    public void StageClear()
    {
        StartCoroutine(StageClearCoroutine());
    }
    IEnumerator StageClearCoroutine()
    {
        BaseBlock.isSetting = true;

        foreach (BaseBlock block in otherBlockList) block.ReSetting();

        t = 0;
        beforeCount = 0;
        count = 0;

        blockCount = isNearBlockList.Count - 1;

        while (t < blockCount)
        {
            yield return null; // �� ������ �ѱ�

            // t [0 ~ 1]
            t += Time.deltaTime * speed;

            if (t > blockCount) t = blockCount;

            count = (int)t;

            for (; beforeCount <= count; beforeCount++)
            {
                isNearBlockList[beforeCount].ReSetting();
            }
        }

        for (; beforeCount <= blockCount; beforeCount++)
        {
            isNearBlockList[beforeCount].ReSetting();
        }

        BaseBlock.isSetting = false;
    }
    public void Excute()
    {
        List<BaseBlock> sortList = new List<BaseBlock>(isNearBlockList);

        // Z�� �������� -> Z�� ������ X�� ������������ ����
        sortList.Sort((a, b) =>
        {
            // Z�� ���� �������� ����
            int zComparison = b.transform.position.z.CompareTo(a.transform.position.z);

            if (zComparison != 0)
            {
                return zComparison;
            }

            // Z���� ���ٸ� X�� ���� �������� ����
            return a.transform.position.x.CompareTo(b.transform.position.x);
        });

        // ���ĵ� ����Ʈ�� isNearBlockList�� �ٽ� �Ҵ�
        isNearBlockList = sortList;

        //isNearBlockList.Clear();
        //otherBlockList.Clear();

        //for (int i = 0; i < blocks.Length; i++)
        //{
        //    if (blocks[i].isNearest)
        //    {
        //        isNearBlockList.Add(blocks[i]);
        //    }
        //    else
        //    {
        //        otherBlockList.Add(blocks[i]);
        //    }
        //}
    }
}
