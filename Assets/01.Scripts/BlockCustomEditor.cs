using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BaseBlock), true)] // 'true'�� �߰��ϸ� ���� Ŭ������ ����
[CanEditMultipleObjects] // ���� ���� ���
public class BlockCustomEditor : Editor
{
    public override void OnInspectorGUI()
    { 
        // ���� �⺻ Inspector UI ǥ��
        serializedObject.Update();
        base.OnInspectorGUI();
        BaseBlock baseBlock = (BaseBlock)target;

        if (GUILayout.Button("��ġ�� ���� �浵 ���ϱ�"))
        {
            foreach (var targetObject in targets) // ���� ��ü�� ��ȸ
            {
                BaseBlock myScript = (BaseBlock)targetObject;
                myScript.Setting();
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}
