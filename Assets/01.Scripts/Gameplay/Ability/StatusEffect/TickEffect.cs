using UnityEngine;

[CreateAssetMenu(fileName = "Ability", menuName = "Ability/TickEffect")]
public class TickEffect : StatusEffect
{
    [Header("Tick")]

    public float interval; // ���� ������

    // ƽ ���� ���ϴ� ������ ( ������ ��� ƽ���� ���� �Ѵ� )
    public float startValue; // �� ���� (���� : 1~..., ����� : ...~0)
    public float stackValue; // ��ø���� �����ϴ� ��

    [HideInInspector] public float value; // ������ �����ϴ� ��

    public override void SetCount(int count)
    {
        base.SetCount(count);

        value = startValue + stackValue * (count - 1);
    }
}