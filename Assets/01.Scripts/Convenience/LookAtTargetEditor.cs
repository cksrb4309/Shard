using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LookAtTarget))] // Ÿ�� Ŭ����
[CanEditMultipleObjects] // ���� ���� ���
public class LookAtTargetEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        base.OnInspectorGUI();
        
        LookAtTarget lookAtTarget = (LookAtTarget)target;

        if (GUILayout.Button("�Ĵٺ���"))
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
