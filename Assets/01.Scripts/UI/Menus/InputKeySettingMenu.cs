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
    public List<Button> rebindButtons;  // �� Ű�� �缳���ϴ� UI ��ư��
    //public List<TMP_Text> keyDisplayTexts;  // �� Ű�� ���� ������ Ű�� ǥ���� UI �ؽ�Ʈ��
    public List<MainButton> keyDisplayTexts;  // �� Ű�� ���� ������ Ű�� ǥ���� UI �ؽ�Ʈ��

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
        //LoadBindingOverrides();  // ���� ���� �� ����� ���ε� �ҷ�����
        InitializeUI();          // UI ��ư�� �ؽ�Ʈ �ʱ�ȭ
    }
    private void InitializeUI()
    {
        // �� ��ư�� ���� Ŭ�� �̺�Ʈ �����ʸ� �߰��ϰ�, ���� ���ε� ���¸� ǥ��
        for (int i = 0; i < rebindButtons.Count; i++)
        {
            int index = i;  // Lambda ĸó ���� ����
            rebindButtons[i].onClick.AddListener(() => StartRebinding(inputActions[index].action, keyDisplayTexts[index]));
            UpdateKeyText(inputActions[index].action, keyDisplayTexts[index]);  // ���� Ű�� ǥ��
        }
    }
    private void UpdateKeyTexts()
    {
        for (int i = 0; i < rebindButtons.Count; i++)
            UpdateKeyText(inputActions[i].action, keyDisplayTexts[i]);  // ���� Ű�� ǥ��
    }
    private void UpdateKeyText(InputAction action, MainButton keyText)
    {
        // ù ��° ���ε��� Ű�� �̸��� ������ ǥ�� (��: "W", "A" ��)
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
        // ���� �����ε� �۾��� ���� ���̶�� ���� ����
        if (rebindingOperation != null)
        {
            rebindingOperation.Dispose();
        }

        // �����ε� ���� �׼��� ��Ȱ��ȭ
        action.Disable();

        // UI ������Ʈ
        keyText.TextSetting("...");

        // �����ε� �۾� ����
        rebindingOperation = action.PerformInteractiveRebinding()
            .WithControlsExcluding("<Mouse>/delta") // ���콺 ������ ����
            .WithCancelingThrough("<Keyboard>/escape") // ESC�� ���
            .OnMatchWaitForAnother(0.1f) // �ߺ� ����
            .OnCancel(operation =>
            {
                UpdateKeyText(action, keyText); // UI ����
                operation.Dispose(); // ���ҽ� ����
                action.Enable(); // �����ε� ��� �� �׼� Ȱ��ȭ
            })
            .OnComplete(operation =>
            {
                UpdateKeyText(action, keyText); // UI ������Ʈ
                //SaveBindingOverrides(); // ������� ����
                operation.Dispose(); // ���ҽ� ����

                // �����ε� �� ���ο� ���ε� ����
                action.Enable(); // �����ε� �� �׼� Ȱ��ȭ
            })
            .Start();
    }

    private void LoadBindingOverrides()
    {
        string path = Path.Combine(Application.persistentDataPath, BindingFilePath);

        bool[] isEnabled = new bool[inputActions.Count];

        for (int i = 0; i < inputActions.Count; i++)
            isEnabled[i] = inputActions[i].action.enabled;

        // ������ �����ϸ� ���ε� ������ �б�
        if (File.Exists(path))
        {
            string jsonData = File.ReadAllText(path);
            MyBindingData bindingData = JsonUtility.FromJson<MyBindingData>(jsonData);

            for (int i = 0; i < inputActions.Count; i++)
            {
                if (bindingData.keys.Contains(inputActions[i].action.name))
                {
                    int index = bindingData.keys.IndexOf(inputActions[i].action.name);
                    if (!string.IsNullOrEmpty(bindingData.values[index]))  // ���ε� ���� ���� ���
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

    // ���� ���� �� ���ε� ����
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