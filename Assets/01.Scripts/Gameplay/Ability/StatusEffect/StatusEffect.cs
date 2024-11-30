using UnityEngine;

public class StatusEffect : ScriptableObject // 상태 변화 효과
{
    [Header("Base")]
    public string effectName; // 명칭
    public float duration; // 지속시간

    [HideInInspector] public int maxCount; // 중첩 횟수
    public int startCount;
    public int stackCount;
    public virtual void SetCount(int count)
    {
        maxCount = startCount + stackCount * (count - 1);
    }
}