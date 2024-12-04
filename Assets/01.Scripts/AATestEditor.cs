using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BlockGroup))]
[CanEditMultipleObjects]
public class AATestEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        BlockGroup myScript = (BlockGroup)target;

        if (GUILayout.Button("½ÇÇà"))
        {
            foreach (var i in targets)
            {
                ((BlockGroup)i).Excute();
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}
