using UnityEngine;

[CreateAssetMenu(fileName = "Ability", menuName = "Ability/StatEffect")]
public class StatEffect : StatusEffect
{
    [Header("Stat")]
    public Attribute attribute; // �Ӽ� ����
    public float startValue; // �Ӽ� ���� �ʱⰪ ��) 1.1 
    public float stackValue; // �Ӽ� ���� ������ ��) 0.1
    public float startCountValue;
    public float stackCountValue;
    [HideInInspector] public float value; // ���� ���������� �����ؾ��ϴ� ��
    [HideInInspector] public float countValue; // ���� ��ø��ŭ ���� ���������� �����ؾ��ϴ� ��

    public override void SetCount(int count)
    {
        base.SetCount(count);

        value = startValue + stackValue * (count - 1);

        countValue = startCountValue + stackCountValue * (count - 1);
    }

}