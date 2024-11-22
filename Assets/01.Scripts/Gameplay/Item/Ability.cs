using UnityEngine;

public abstract class Ability : ScriptableObject
{
    public string abilityName; // ������ ��Ī
    public string explain; // ������ ����
    public Sprite abilityIcon; // ������ ������
    public abstract ICondition GetCondition(); // 1. ���� �ɷ����� �˾ƾ���
    // 2. � ��ȣ�ۿ� ( �нú� , ���� , Ư��ȿ�� ) �� �߻���Ű�� �� �˰� ���� �ʿ�� ���� ���ǿ� ���� �����ϴϱ�...
    // �׷��ٸ� ��ȣ�ۿ��� ��� �߻���ų���� ��ġ�� ���ؾ���.
    // Ability�� ���� Ŭ������ �߻���Ű�� ��ġ�� ����� ����
    // ICondition�� ���� �������� Ȯ���ؼ� ������ �κп��� ������ ��������
    // ������ �нú� �ɷ� �߰�, ���� �߻�, Ư��ȿ�� �߻�

}