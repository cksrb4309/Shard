using UnityEngine;

public abstract class Ability : ScriptableObject
{
    public string abilityName; // 아이템 명칭
    public string explain; // 아이템 설명
    public Sprite abilityIcon; // 아이템 아이콘
    public abstract ICondition GetCondition(); // 1. 무슨 능력인지 알아야함
    // 2. 어떤 상호작용 ( 패시브 , 버프 , 특수효과 ) 를 발생시키는 지 알고 있을 필요는 없을 조건에 따라 적용하니까...
    // 그렇다면 상호작용을 어떻게 발생시킬지는 위치를 정해야함.
    // Ability의 하위 클래스로 발생시키는 위치를 만든다 가정
    // ICondition의 하위 조건으로 확인해서 실행한 부분에서 내용을 넣을거임
    // 내용은 패시브 능력 추가, 버프 발생, 특수효과 발생

}