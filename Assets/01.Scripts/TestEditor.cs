using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Test))]
public class TestEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        Test myScript = (Test)target;

        if (GUILayout.Button("Å×½ºÆ®"))
        {
            myScript.Excute();
        }
    }
}
