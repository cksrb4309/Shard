using UnityEngine;

[CreateAssetMenu(fileName = "Ability", menuName = "Ability/TickEffect")]
public class TickEffect : StatusEffect
{
    [Header("Tick")]

    public float interval; // 적용 딜레이

    // 틱 마다 가하는 데미지 ( 음수일 경우 틱마다 힐을 한다 )
    public float startValue; // 값 선택 (버프 : 1~..., 디버프 : ...~0)
    public float stackValue; // 중첩마다 증가하는 값

    [HideInInspector] public float value; // 실제로 적용하는 값

    public override void SetCount(int count)
    {
        base.SetCount(count);

        value = startValue + stackValue * (count - 1);
    }
}