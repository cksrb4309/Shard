using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using Michsky.UI.Shift;

public class InputKeySettingMenu : MonoBehaviour
{
    [Header("Input Actions")]
    public List<InputActionReference> inputActions;

    [Header("UI Elements")]
    public List<Button> rebindButtons;  // 각 키를 재설정하는 UI 버튼들
    //public List<TMP_Text> keyDisplayTexts;  // 각 키의 현재 설정된 키를 표시할 UI 텍스트들
    public List<MainButton> keyDisplayTexts;  // 각 키의 현재 설정된 키를 표시할 UI 텍스트들

    private const string BindingFilePath = "inputBindings.json";
    private InputActionRebindingExtensions.RebindingOperation rebindingOperation;

    void Start()
    {
        LoadBindingOverrides();  // 게임 시작 시 저장된 바인딩 불러오기
        InitializeUI();          // UI 버튼과 텍스트 초기화
    }
    private void InitializeUI()
    {
        // 각 버튼에 대한 클릭 이벤트 리스너를 추가하고, 현재 바인딩 상태를 표시
        for (int i = 0; i < rebindButtons.Count; i++)
        {
            int index = i;  // Lambda 캡처 문제 방지
            rebindButtons[i].onClick.AddListener(() => StartRebinding(inputActions[index].action, keyDisplayTexts[index]));
            UpdateKeyText(inputActions[index].action, keyDisplayTexts[index]);  // 현재 키를 표시
        }
    }
    private void UpdateKeyTexts()
    {
        for (int i = 0; i < rebindButtons.Count; i++)
            UpdateKeyText(inputActions[i].action, keyDisplayTexts[i]);  // 현재 키를 표시
    }
    private void UpdateKeyText(InputAction action, MainButton keyText)
    {
        // 첫 번째 바인딩된 키의 이름을 가져와 표시 (예: "W", "A" 등)
        if (action.bindings.Count > 0)
        {
            var binding = action.bindings[0];

            string str = InputControlPath.ToHumanReadableString(
                binding.effectivePath,
                InputControlPath.HumanReadableStringOptions.OmitDevice);

            if (str.Contains("Button", System.StringComparison.OrdinalIgnoreCase))
            {
                str = str.Replace("Button", "Mouse", System.StringComparison.OrdinalIgnoreCase);
            }

            keyText.TextSetting(str);
        }
    }

    // 바인딩된 키 값을 JSON 파일로 저장
    private void SaveBindingOverrides()
    {
        Dictionary<string, string> bindingData = new Dictionary<string, string>();

        // 각 InputAction에 대해 바인딩 데이터를 JSON으로 변환하여 저장
        foreach (InputActionReference actionRef in inputActions)
        {
            string json = actionRef.action.SaveBindingOverridesAsJson();
            bindingData[actionRef.action.name] = json;
        }

        // JSON 파일로 바인딩 데이터 저장
        string jsonData = JsonUtility.ToJson(new SerializableDictionary<string, string>(bindingData));
        File.WriteAllText(Path.Combine(Application.persistentDataPath, BindingFilePath), jsonData);
    }

    private void StartRebinding(InputAction action, MainButton keyText)
    {
        // 기존 리바인딩 작업이 있다면 중지
        if (rebindingOperation != null)
        {
            rebindingOperation.Dispose();
            UpdateKeyTexts();
        }

        keyText.TextSetting("...");


        action.Dispose();

        // 리바인딩 시작
        rebindingOperation = action.PerformInteractiveRebinding()
            .WithControlsExcluding("<Mouse>/delta")  // 마우스 움직임은 제약을 검
            .WithCancelingThrough("<Keyboard>/escape") // Escape로 리바인딩 취소 가능
            .OnMatchWaitForAnother(0.1f)               // 중복 방지
            .OnComplete(operation =>                   // 리바인딩 완료
            {
                UpdateKeyText(action, keyText);  // 텍스트 업데이트
                SaveBindingOverrides();          // 저장
                operation.Dispose();             // 리소스 해제
                action.Enable(); // 리바인딩이 끝난 후 다시 활성화
            })
            .Start();
    }

    // JSON 파일로부터 바인딩된 키 값 불러오기
    private void LoadBindingOverrides()
    {
        string path = Path.Combine(Application.persistentDataPath, BindingFilePath);

        if (File.Exists(path))
        {
            string jsonData = File.ReadAllText(path);
            var bindingData = JsonUtility.FromJson<SerializableDictionary<string, string>>(jsonData);

            // 각 InputAction에 대해 JSON 바인딩 불러오기
            foreach (var actionRef in inputActions)
            {
                if (bindingData.dictionary.TryGetValue(actionRef.action.name, out var json))
                {
                    actionRef.action.LoadBindingOverridesFromJson(json);
                }
            }
        }
    }

    // 게임 종료 시 바인딩 저장
    private void OnApplicationQuit()
    {
        SaveBindingOverrides();
    }
}

[System.Serializable]
public class SerializableDictionary<TKey, TValue>
{
    public List<TKey> keys = new List<TKey>();
    public List<TValue> values = new List<TValue>();
    public Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();

    public SerializableDictionary(Dictionary<TKey, TValue> dict)
    {
        dictionary = dict;
        foreach (var kvp in dict)
        {
            keys.Add(kvp.Key);
            values.Add(kvp.Value);
        }
    }

    // JSON 변환 시 필요
    public SerializableDictionary() { }
}