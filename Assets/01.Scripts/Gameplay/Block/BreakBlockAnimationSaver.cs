using UnityEngine;
using UnityEditor; // Animation Clip 생성에는 Editor 스크립트 필요
using System.Collections.Generic;

public class BreakBlockAnimationSaver : MonoBehaviour
{
    public GameObject parentObject; // 파편 부모 오브젝트 (블록)

    public string savePath = "Assets/Animations/BreakAnimation.anim"; // 저장 경로

    public void GenerateAndSaveAnimation()
    {
        // 1. Animation Clip 생성
        AnimationClip clip = new AnimationClip
        {
            frameRate = 30 // 프레임 속도 설정 (기본: 30FPS)
        };

        // 2. 부모 오브젝트의 자식 파편들 가져오기
        Transform[] fragments = parentObject.GetComponentsInChildren<Transform>();

        // 3. 각 파편의 위치, 회전, 크기를 키프레임으로 기록
        foreach (Transform fragment in fragments)
        {
            if (fragment == parentObject.transform) continue; // 부모 오브젝트는 스킵

            string path = AnimationUtility.CalculateTransformPath(fragment, parentObject.transform);

            // 위치 기록
            AddKeyframesToClip(clip, path, "localPosition", fragment.localPosition);

            // 회전 기록
            AddKeyframesToClip(clip, path, "localRotation", fragment.localRotation);

            // 크기 기록
            AddKeyframesToClip(clip, path, "localScale", fragment.localScale);
        }

        // 4. Animation Clip 저장
        AssetDatabase.CreateAsset(clip, savePath);

        AssetDatabase.SaveAssets();

        Debug.Log($"Animation Clip saved at: {savePath}");
    }

    private void AddKeyframesToClip(AnimationClip clip, string path, string property, Vector3 value)
    {
        // X, Y, Z 각각의 키프레임 추가
        AnimationCurve curveX = new AnimationCurve();
        AnimationCurve curveY = new AnimationCurve();
        AnimationCurve curveZ = new AnimationCurve();

        // 0초의 초기 값 기록
        curveX.AddKey(0, value.x);
        curveY.AddKey(0, value.y);
        curveZ.AddKey(0, value.z);

        // 곡선 추가
        clip.SetCurve(path, typeof(Transform), $"{property}.x", curveX);
        clip.SetCurve(path, typeof(Transform), $"{property}.y", curveY);
        clip.SetCurve(path, typeof(Transform), $"{property}.z", curveZ);
    }

    private void AddKeyframesToClip(AnimationClip clip, string path, string property, Quaternion value)
    {
        // 회전 기록은 Quaternion을 Euler로 변환
        Vector3 euler = value.eulerAngles;
        AddKeyframesToClip(clip, path, property, euler);
    }
}