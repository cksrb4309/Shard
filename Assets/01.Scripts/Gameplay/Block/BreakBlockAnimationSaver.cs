using UnityEngine;
using UnityEditor; // Animation Clip �������� Editor ��ũ��Ʈ �ʿ�
using System.Collections.Generic;

public class BreakBlockAnimationSaver : MonoBehaviour
{
    public GameObject parentObject; // ���� �θ� ������Ʈ (���)

    public string savePath = "Assets/Animations/BreakAnimation.anim"; // ���� ���

    public void GenerateAndSaveAnimation()
    {
        // 1. Animation Clip ����
        AnimationClip clip = new AnimationClip
        {
            frameRate = 30 // ������ �ӵ� ���� (�⺻: 30FPS)
        };

        // 2. �θ� ������Ʈ�� �ڽ� ����� ��������
        Transform[] fragments = parentObject.GetComponentsInChildren<Transform>();

        // 3. �� ������ ��ġ, ȸ��, ũ�⸦ Ű���������� ���
        foreach (Transform fragment in fragments)
        {
            if (fragment == parentObject.transform) continue; // �θ� ������Ʈ�� ��ŵ

            string path = AnimationUtility.CalculateTransformPath(fragment, parentObject.transform);

            // ��ġ ���
            AddKeyframesToClip(clip, path, "localPosition", fragment.localPosition);

            // ȸ�� ���
            AddKeyframesToClip(clip, path, "localRotation", fragment.localRotation);

            // ũ�� ���
            AddKeyframesToClip(clip, path, "localScale", fragment.localScale);
        }

        // 4. Animation Clip ����
        AssetDatabase.CreateAsset(clip, savePath);

        AssetDatabase.SaveAssets();

        Debug.Log($"Animation Clip saved at: {savePath}");
    }

    private void AddKeyframesToClip(AnimationClip clip, string path, string property, Vector3 value)
    {
        // X, Y, Z ������ Ű������ �߰�
        AnimationCurve curveX = new AnimationCurve();
        AnimationCurve curveY = new AnimationCurve();
        AnimationCurve curveZ = new AnimationCurve();

        // 0���� �ʱ� �� ���
        curveX.AddKey(0, value.x);
        curveY.AddKey(0, value.y);
        curveZ.AddKey(0, value.z);

        // � �߰�
        clip.SetCurve(path, typeof(Transform), $"{property}.x", curveX);
        clip.SetCurve(path, typeof(Transform), $"{property}.y", curveY);
        clip.SetCurve(path, typeof(Transform), $"{property}.z", curveZ);
    }

    private void AddKeyframesToClip(AnimationClip clip, string path, string property, Quaternion value)
    {
        // ȸ�� ����� Quaternion�� Euler�� ��ȯ
        Vector3 euler = value.eulerAngles;
        AddKeyframesToClip(clip, path, property, euler);
    }
}