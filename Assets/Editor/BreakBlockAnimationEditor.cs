using UnityEngine;
using UnityEditor;
using System.IO;

public class BreakBlockAnimationEditor : EditorWindow
{
    private GameObject parentObject;
    private string savePath = "Assets/Animations/BreakAnimation.anim";

    private float animationDuration = 1.0f; // �ִϸ��̼� ���� (��)
    private float samplingInterval = 0.1f; // Ű������ ���ø� ���� (��)

    [MenuItem("Tools/Break Block Animation Editor")]
    public static void ShowWindow()
    {
        GetWindow<BreakBlockAnimationEditor>("Break Block Animation");
    }

    private void OnGUI()
    {
        GUILayout.Label("Break Block Animation Settings", EditorStyles.boldLabel);

        parentObject = (GameObject)EditorGUILayout.ObjectField("Parent Object", parentObject, typeof(GameObject), true);
        savePath = EditorGUILayout.TextField("Save Path", savePath);
        animationDuration = EditorGUILayout.FloatField("Animation Duration (s)", animationDuration);
        samplingInterval = EditorGUILayout.FloatField("Sampling Interval (s)", samplingInterval);

        if (GUILayout.Button("Generate and Save Animation"))
        {
            if (parentObject != null)
            {
                GenerateAndSaveAnimation();
            }
            else
            {
                Debug.LogError("Parent object is not assigned.");
            }
        }
    }

    private void GenerateAndSaveAnimation()
    {
        // ���丮 ����
        string directory = Path.GetDirectoryName(savePath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
            AssetDatabase.Refresh();
        }

        // Animation Clip ����
        AnimationClip clip = new AnimationClip
        {
            frameRate = 30 // ������ �ӵ�
        };

        Transform[] fragments = parentObject.GetComponentsInChildren<Transform>();

        foreach (Transform fragment in fragments)
        {
            if (fragment == parentObject.transform) continue;

            string path = AnimationUtility.CalculateTransformPath(fragment, parentObject.transform);

            // ������ �ʱⰪ�� �ݿ��Ͽ� �ִϸ��̼� ����
            AddAnimatedKeyframes(clip, path, fragment);
        }

        // Animation Clip ����
        AssetDatabase.CreateAsset(clip, savePath);
        AssetDatabase.SaveAssets();

        Debug.Log($"Animation Clip saved at: {savePath}");
    }

    private void AddAnimatedKeyframes(AnimationClip clip, string path, Transform fragment)
    {
        // AnimationCurve �ʱ�ȭ
        AnimationCurve posX = new AnimationCurve();
        AnimationCurve posY = new AnimationCurve();
        AnimationCurve posZ = new AnimationCurve();

        AnimationCurve scaleX = new AnimationCurve();
        AnimationCurve scaleY = new AnimationCurve();
        AnimationCurve scaleZ = new AnimationCurve();

        // �ʱ� ��ġ, ũ��
        Vector3 initialPosition = fragment.localPosition;

        Vector3 initialScale = fragment.localScale;

        // 0~1�� ���� ���ø�
        for (float t = 0; t <= animationDuration; t += samplingInterval)
        {
            // �ʱⰪ�� ������� �ִϸ��̼� �� ���
            Vector3 position = CalculatePosition(fragment, t, initialPosition);

            Vector3 scale = CalculateScale(fragment, t, initialScale);

            // Ű������ �߰�
            posX.AddKey(t, position.x);
            posY.AddKey(t, position.y);
            posZ.AddKey(t, position.z);

            scaleX.AddKey(t, scale.x);
            scaleY.AddKey(t, scale.y);
            scaleZ.AddKey(t, scale.z);
        }

        // AnimationCurve�� Animation Clip�� ����
        clip.SetCurve(path, typeof(Transform), "localPosition.x", posX);
        clip.SetCurve(path, typeof(Transform), "localPosition.y", posY);
        clip.SetCurve(path, typeof(Transform), "localPosition.z", posZ);


        clip.SetCurve(path, typeof(Transform), "localScale.x", scaleX);
        clip.SetCurve(path, typeof(Transform), "localScale.y", scaleY);
        clip.SetCurve(path, typeof(Transform), "localScale.z", scaleZ);
    }

    private Vector3 CalculatePosition(Transform fragment, float time, Vector3 initialPosition)
    {
        // BreakBaseBlockParts�� ��ġ ��� ���� (��: �ٱ����� ����)
        Vector3 direction = (fragment.position - parentObject.transform.position).normalized;
        float speed = 2.0f; // ������ ������ �ӵ�
        return initialPosition + direction * speed * time;
    }


    private Vector3 CalculateScale(Transform fragment, float time, Vector3 initialScale)
    {
        return Vector3.Lerp(initialScale, Vector3.zero, time);
    }
}