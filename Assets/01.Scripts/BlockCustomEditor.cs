using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BaseBlock), true)] // 'true'를 추가하면 하위 클래스도 지원
[CanEditMultipleObjects] // 다중 선택 허용
public class BlockCustomEditor : Editor
{
    public override void OnInspectorGUI()
    { 
        // 기존 기본 Inspector UI 표시
        serializedObject.Update();
        base.OnInspectorGUI();
        BaseBlock baseBlock = (BaseBlock)target;

        if (GUILayout.Button("위치를 통한 경도 구하기"))
        {
            foreach (var targetObject in targets) // 여러 객체를 순회
            {
                BaseBlock myScript = (BaseBlock)targetObject;
                myScript.Setting();
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}
