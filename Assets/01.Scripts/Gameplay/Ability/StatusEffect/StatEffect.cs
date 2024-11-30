using UnityEngine;

[CreateAssetMenu(fileName = "Ability", menuName = "Ability/StatEffect")]
public class StatEffect : StatusEffect
{
    [Header("Stat")]
    public Attribute attribute; // 속성 선택
    public float startValue; // 속성 적용 초기값 예) 1.1 
    public float stackValue; // 속성 적용 누적값 예) 0.1
    public float startCountValue;
    public float stackCountValue;
    [HideInInspector] public float value; // 실제 내부적으로 적용해야하는 값
    [HideInInspector] public float countValue; // 버프 중첩만큼 실제 내부적으로 적용해야하는 값

    public override void SetCount(int count)
    {
        base.SetCount(count);

        value = startValue + stackValue * (count - 1);

        countValue = startCountValue + stackCountValue * (count - 1);
    }

}