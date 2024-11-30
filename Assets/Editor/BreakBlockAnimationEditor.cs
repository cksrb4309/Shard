using UnityEngine;
using UnityEditor;
using System.IO;

public class BreakBlockAnimationEditor : EditorWindow
{
    private GameObject parentObject;
    private string savePath = "Assets/Animations/BreakAnimation.anim";

    private float animationDuration = 1.0f; // 애니메이션 길이 (초)
    private float samplingInterval = 0.1f; // 키프레임 샘플링 간격 (초)

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
        // 디렉토리 생성
        string directory = Path.GetDirectoryName(savePath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
            AssetDatabase.Refresh();
        }

        // Animation Clip 생성
        AnimationClip clip = new AnimationClip
        {
            frameRate = 30 // 프레임 속도
        };

        Transform[] fragments = parentObject.GetComponentsInChildren<Transform>();

        foreach (Transform fragment in fragments)
        {
            if (fragment == parentObject.transform) continue;

            string path = AnimationUtility.CalculateTransformPath(fragment, parentObject.transform);

            // 파츠별 초기값을 반영하여 애니메이션 생성
            AddAnimatedKeyframes(clip, path, fragment);
        }

        // Animation Clip 저장
        AssetDatabase.CreateAsset(clip, savePath);
        AssetDatabase.SaveAssets();

        Debug.Log($"Animation Clip saved at: {savePath}");
    }

    private void AddAnimatedKeyframes(AnimationClip clip, string path, Transform fragment)
    {
        // AnimationCurve 초기화
        AnimationCurve posX = new AnimationCurve();
        AnimationCurve posY = new AnimationCurve();
        AnimationCurve posZ = new AnimationCurve();

        AnimationCurve scaleX = new AnimationCurve();
        AnimationCurve scaleY = new AnimationCurve();
        AnimationCurve scaleZ = new AnimationCurve();

        // 초기 위치, 크기
        Vector3 initialPosition = fragment.localPosition;

        Vector3 initialScale = fragment.localScale;

        // 0~1초 동안 샘플링
        for (float t = 0; t <= animationDuration; t += samplingInterval)
        {
            // 초기값을 기반으로 애니메이션 값 계산
            Vector3 position = CalculatePosition(fragment, t, initialPosition);

            Vector3 scale = CalculateScale(fragment, t, initialScale);

            // 키프레임 추가
            posX.AddKey(t, position.x);
            posY.AddKey(t, position.y);
            posZ.AddKey(t, position.z);

            scaleX.AddKey(t, scale.x);
            scaleY.AddKey(t, scale.y);
            scaleZ.AddKey(t, scale.z);
        }

        // AnimationCurve를 Animation Clip에 설정
        clip.SetCurve(path, typeof(Transform), "localPosition.x", posX);
        clip.SetCurve(path, typeof(Transform), "localPosition.y", posY);
        clip.SetCurve(path, typeof(Transform), "localPosition.z", posZ);


        clip.SetCurve(path, typeof(Transform), "localScale.x", scaleX);
        clip.SetCurve(path, typeof(Transform), "localScale.y", scaleY);
        clip.SetCurve(path, typeof(Transform), "localScale.z", scaleZ);
    }

    private Vector3 CalculatePosition(Transform fragment, float time, Vector3 initialPosition)
    {
        // BreakBaseBlockParts의 위치 계산 로직 (예: 바깥으로 퍼짐)
        Vector3 direction = (fragment.position - parentObject.transform.position).normalized;
        float speed = 2.0f; // 파편이 퍼지는 속도
        return initialPosition + direction * speed * time;
    }


    private Vector3 CalculateScale(Transform fragment, float time, Vector3 initialScale)
    {
        return Vector3.Lerp(initialScale, Vector3.zero, time);
    }
}