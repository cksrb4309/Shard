using Sirenix.OdinInspector;
using UnityEngine;

public class Test_BlockGroup : MonoBehaviour
{
    [SerializeField] Test_Break_1[] test_1_blocks;
    [SerializeField] Test_Break_2[] test_2_blocks;
    [SerializeField] Test_Break_3[] test_3_blocks;
    [SerializeField] Test_Break_4[] test_4_blocks;
    [SerializeField] Test_Break_5[] test_5_blocks;
    [SerializeField] GpuBreakBlockEffect[] test_6_blocks;

    [Button]
    public void Setting()
    {
        Test_Break_1[] test_break_1s = transform.GetComponentsInChildren<Test_Break_1>();
        test_1_blocks = new Test_Break_1[test_break_1s.Length];

        for (int i = 0; i < test_1_blocks.Length; i++)
        {
            test_break_1s[i].Setting();
            test_1_blocks[i] = test_break_1s[i];
        }

        Test_Break_2[] test_break_2s = transform.GetComponentsInChildren<Test_Break_2>();
        test_2_blocks = new Test_Break_2[test_break_2s.Length];

        for (int i = 0; i < test_2_blocks.Length; i++)
        {
            test_break_2s[i].Setting();
            test_2_blocks[i] = test_break_2s[i];
        }


        Test_Break_3[] test_break_3s = transform.GetComponentsInChildren<Test_Break_3>();
        test_3_blocks = new Test_Break_3[test_break_3s.Length];

        for (int i = 0; i < test_3_blocks.Length; i++)
        {
            test_break_3s[i].Setting();
            test_3_blocks[i] = test_break_3s[i];
        }

        Test_Break_4[] test_break_4s = transform.GetComponentsInChildren<Test_Break_4>();
        test_4_blocks = new Test_Break_4[test_break_4s.Length];

        for (int i = 0; i < test_4_blocks.Length; i++)
        {
            test_break_4s[i].Setting();
            test_4_blocks[i] = test_break_4s[i];
        }

        Test_Break_5[] test_break_5s = transform.GetComponentsInChildren<Test_Break_5>();
        test_5_blocks = new Test_Break_5[test_break_5s.Length];

        for (int i = 0; i < test_5_blocks.Length; i++)
        {
            test_break_5s[i].Setting();
            test_5_blocks[i] = test_break_5s[i];
        }

        GpuBreakBlockEffect[] test_break_6s = transform.GetComponentsInChildren<GpuBreakBlockEffect>();
        test_6_blocks = new GpuBreakBlockEffect[test_break_6s.Length];

        for (int i = 0; i < test_6_blocks.Length; i++)
        {
            test_break_6s[i].Setup();
            test_6_blocks[i] = test_break_6s[i];
        }
    }

    [Button]
    public void ResetShard()
    {
        if (test_1_blocks != null)
        {
            for (int i = 0; i < test_1_blocks.Length; i++)
            {
                test_1_blocks[i].gameObject.SetActive(true);
                test_1_blocks[i].ResetShard();
            }
        }

        if (test_2_blocks != null)
        {
            for (int i = 0; i < test_2_blocks.Length; i++)
            {
                test_2_blocks[i].gameObject.SetActive(true);
                test_2_blocks[i].ResetShard();
            }
        }

        if (test_3_blocks != null)
        {
            for (int i = 0; i < test_3_blocks.Length; i++)
            {
                test_3_blocks[i].gameObject.SetActive(true);
                test_3_blocks[i].ResetShard();
            }
        }

        if (test_4_blocks != null)
        {
            for (int i = 0; i < test_4_blocks.Length; i++)
            {
                test_4_blocks[i].gameObject.SetActive(true);
                test_4_blocks[i].ResetShard();
            }
        }

        if (test_5_blocks != null)
        {
            for (int i = 0; i < test_5_blocks.Length; i++)
            {
                test_5_blocks[i].gameObject.SetActive(true);
                test_5_blocks[i].ResetShard();
            }
        }

        if (test_6_blocks != null)
        {
            for (int i = 0; i < test_6_blocks.Length; i++)
            {
                test_6_blocks[i].gameObject.SetActive(true);
                test_6_blocks[i].ResetEffect();
            }
        }
    }

    [Button]
    public void Test(int testNumber)
    {
        if (test_1_blocks == null || test_2_blocks == null || test_3_blocks == null || test_4_blocks == null || test_5_blocks == null || test_6_blocks == null)
            Setting();

        ResetShard();

        switch (testNumber)
        {
            case 1:
                for (int i = 0; i < test_1_blocks.Length; i++) test_1_blocks[i].Break(); break;
            case 2:
                for (int i = 0; i < test_2_blocks.Length; i++) test_2_blocks[i].Break(); break;
            case 3:
                for (int i = 0; i < test_3_blocks.Length; i++) test_3_blocks[i].Break(); break;
            case 4:
                for (int i = 0; i < test_4_blocks.Length; i++) test_4_blocks[i].Break(); break;
            case 5:
                for (int i = 0; i < test_5_blocks.Length; i++) test_5_blocks[i].Break(); break;
            case 6:
                for (int i = 0; i < test_6_blocks.Length; i++) test_6_blocks[i].PlayBreak(); break;
        }
    }
}
