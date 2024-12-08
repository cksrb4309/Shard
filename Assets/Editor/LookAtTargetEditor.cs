using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LookAtTarget))] // 타겟 클래스
[CanEditMultipleObjects] // 다중 선택 허용
public class LookAtTargetEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        base.OnInspectorGUI();
        
        LookAtTarget lookAtTarget = (LookAtTarget)target;

        if (GUILayout.Button("쳐다보기"))
        {
            foreach (var target in targets)
            {
                LookAtTarget myScript = (LookAtTarget)target;
                myScript.LookAt();
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}
