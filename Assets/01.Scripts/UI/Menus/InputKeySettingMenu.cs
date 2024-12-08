using System.Collections.Generic;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using Michsky.UI.Shift;

public class InputKeySettingMenu : MonoBehaviour
{
    public InputActionAsset inputActionAsset;

    [Header("Input Actions")]
    public List<InputActionReference> inputActions;

    [Header("UI Elements")]
    public List<Button> rebindButtons;  // 각 키를 재설정하는 UI 버튼들
    //public List<TMP_Text> keyDisplayTexts;  // 각 키의 현재 설정된 키를 표시할 UI 텍스트들
    public List<MainButton> keyDisplayTexts;  // 각 키의 현재 설정된 키를 표시할 UI 텍스트들

    private const string BindingFilePath = "inputBindings.json";
    private InputActionRebindingExtensions.RebindingOperation rebindingOperation;

    private void OnEnable()
    {
        for (int i = 0; i < inputActions.Count; i++)
        {
            bool isEnabled = false;

            if (inputActions[i].action.enabled)
            {
                isEnabled = true;

                inputActions[i].action.Disable();
            }

            SetDefaultBinding(inputActions[i].action);

            if (isEnabled) inputActions[i].action.Enable();
        }

        var rebinds = PlayerPrefs.GetString("rebinds");

        if (!string.IsNullOrEmpty(rebinds))
        {
            inputActionAsset.LoadBindingOverridesFromJson(rebinds);
        }

    }
    private void OnDisable()
    {
        var rebinds = inputActionAsset.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString("rebinds", rebinds);
    }
    void Start()
    {
        //StartCoroutine(LoadBindingOverridesCoroutine());
        //LoadBindingOverrides();  // 게임 시작 시 저장된 바인딩 불러오기
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

    private void StartRebinding(InputAction action, MainButton keyText)
    {
        // 이전 리바인딩 작업이 진행 중이라면 먼저 종료
        if (rebindingOperation != null)
        {
            rebindingOperation.Dispose();
        }

        // 리바인딩 전에 액션을 비활성화
        action.Disable();

        // UI 업데이트
        keyText.TextSetting("...");

        // 리바인딩 작업 시작
        rebindingOperation = action.PerformInteractiveRebinding()
            .WithControlsExcluding("<Mouse>/delta") // 마우스 움직임 제외
            .WithCancelingThrough("<Keyboard>/escape") // ESC로 취소
            .OnMatchWaitForAnother(0.1f) // 중복 방지
            .OnCancel(operation =>
            {
                UpdateKeyText(action, keyText); // UI 복구
                operation.Dispose(); // 리소스 해제
                action.Enable(); // 리바인딩 취소 후 액션 활성화
            })
            .OnComplete(operation =>
            {
                UpdateKeyText(action, keyText); // UI 업데이트
                //SaveBindingOverrides(); // 변경사항 저장
                operation.Dispose(); // 리소스 해제

                // 리바인딩 후 새로운 바인딩 적용
                action.Enable(); // 리바인딩 후 액션 활성화
            })
            .Start();
    }

    private void LoadBindingOverrides()
    {
        string path = Path.Combine(Application.persistentDataPath, BindingFilePath);

        bool[] isEnabled = new bool[inputActions.Count];

        for (int i = 0; i < inputActions.Count; i++)
            isEnabled[i] = inputActions[i].action.enabled;

        // 파일이 존재하면 바인딩 데이터 읽기
        if (File.Exists(path))
        {
            string jsonData = File.ReadAllText(path);
            MyBindingData bindingData = JsonUtility.FromJson<MyBindingData>(jsonData);

            for (int i = 0; i < inputActions.Count; i++)
            {
                if (bindingData.keys.Contains(inputActions[i].action.name))
                {
                    int index = bindingData.keys.IndexOf(inputActions[i].action.name);
                    if (!string.IsNullOrEmpty(bindingData.values[index]))  // 바인딩 값이 있을 경우
                    {
                        //if (isEnabled[i]) inputActions[i].action.Disable();

                        inputActions[i].action.ApplyBindingOverride(bindingData.values[index]);

                        if (isEnabled[i]) inputActions[i].action.Enable();
                    }
                    else
                    {
                        if (isEnabled[i]) inputActions[i].action.Disable();

                        SetDefaultBinding(inputActions[i].action);

                        if (isEnabled[i]) inputActions[i].action.Enable();
                    }
                }
                else
                {
                    //if (isEnabled[i]) inputActions[i].action.Disable();

                    SetDefaultBinding(inputActions[i].action);

                    if (isEnabled[i]) inputActions[i].action.Enable();
                }
            }
        }
        else
        {
            for (int i = 0; i < inputActions.Count; i++)
            {
                //if (isEnabled[i]) inputActions[i].action.Disable();

                SetDefaultBinding(inputActions[i].action);

                if (isEnabled[i]) inputActions[i].action.Enable();
            }
        }
    }

    // 게임 종료 시 바인딩 저장
    private void OnApplicationQuit()
    {
        var rebinds = inputActionAsset.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString("rebinds", rebinds);
    }
    private void SetDefaultBinding(InputAction action)
    {
        if (action.name == "MoveUp")
        {
            action.ApplyBindingOverride(new InputBinding { path = "<Keyboard>/w" });
        }
        else if (action.name == "MoveDown")
        {
            action.ApplyBindingOverride(new InputBinding { path = "<Keyboard>/s" });
        }
        else if (action.name == "MoveLeft")
        {
            action.ApplyBindingOverride(new InputBinding { path = "<Keyboard>/a" });
        }
        else if (action.name == "MoveRight")
        {
            action.ApplyBindingOverride(new InputBinding { path = "<Keyboard>/d" });
        }
        else if (action.name == "Interact")
        {
            action.ApplyBindingOverride(new InputBinding { path = "<Keyboard>/f" });
        }
        else if (action.name == "Skill1")
        {
            action.ApplyBindingOverride(new InputBinding { path = "<Mouse>/rightButton" });
        }
        else if (action.name == "Skill2")
        {
            action.ApplyBindingOverride(new InputBinding { path = "<Keyboard>/r" });
        }
    }
}
[System.Serializable]
public class MyBindingData
{
    public List<string> keys = new List<string>();
    public List<string> values = new List<string>();

    public Dictionary<string, string> GetDictionary()
    {
        Dictionary<string, string> dict = new Dictionary<string, string>();
        for (int i = 0; i < keys.Count; i++)
        {
            dict[keys[i]] = values[i];
        }
        return dict;
    }
}