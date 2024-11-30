using UnityEngine;

public class StatusEffect : ScriptableObject // ���� ��ȭ ȿ��
{
    [Header("Base")]
    public string effectName; // ��Ī
    public float duration; // ���ӽð�

    [HideInInspector] public int maxCount; // ��ø Ƚ��
    public int startCount;
    public int stackCount;
    public virtual void SetCount(int count)
    {
        maxCount = startCount + stackCount * (count - 1);
    }
}