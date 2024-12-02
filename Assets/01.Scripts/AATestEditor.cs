using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AAAAAA))]
[CanEditMultipleObjects]
public class AATestEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        AAAAAA myScript = (AAAAAA)target;

        if (GUILayout.Button("½ÇÇà"))
        {
            foreach (var i in targets)
            {
                ((AAAAAA)i).Excute();
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}
