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
    public List<Button> rebindButtons;  // �� Ű�� �缳���ϴ� UI ��ư��
    //public List<TMP_Text> keyDisplayTexts;  // �� Ű�� ���� ������ Ű�� ǥ���� UI �ؽ�Ʈ��
    public List<MainButton> keyDisplayTexts;  // �� Ű�� ���� ������ Ű�� ǥ���� UI �ؽ�Ʈ��

    private const string BindingFilePath = "inputBindings.json";
    private InputActionRebindingExtensions.RebindingOperation rebindingOperation;

    void Start()
    {
        LoadBindingOverrides();  // ���� ���� �� ����� ���ε� �ҷ�����
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

    // ���ε��� Ű ���� JSON ���Ϸ� ����
    private void SaveBindingOverrides()
    {
        Dictionary<string, string> bindingData = new Dictionary<string, string>();

        // �� InputAction�� ���� ���ε� �����͸� JSON���� ��ȯ�Ͽ� ����
        foreach (InputActionReference actionRef in inputActions)
        {
            string json = actionRef.action.SaveBindingOverridesAsJson();
            bindingData[actionRef.action.name] = json;
        }

        // JSON ���Ϸ� ���ε� ������ ����
        string jsonData = JsonUtility.ToJson(new SerializableDictionary<string, string>(bindingData));
        File.WriteAllText(Path.Combine(Application.persistentDataPath, BindingFilePath), jsonData);
    }

    private void StartRebinding(InputAction action, MainButton keyText)
    {
        // ���� �����ε� �۾��� �ִٸ� ����
        if (rebindingOperation != null)
        {
            rebindingOperation.Dispose();
            UpdateKeyTexts();
        }

        keyText.TextSetting("...");


        action.Dispose();

        // �����ε� ����
        rebindingOperation = action.PerformInteractiveRebinding()
            .WithControlsExcluding("<Mouse>/delta")  // ���콺 �������� ������ ��
            .WithCancelingThrough("<Keyboard>/escape") // Escape�� �����ε� ��� ����
            .OnMatchWaitForAnother(0.1f)               // �ߺ� ����
            .OnComplete(operation =>                   // �����ε� �Ϸ�
            {
                UpdateKeyText(action, keyText);  // �ؽ�Ʈ ������Ʈ
                SaveBindingOverrides();          // ����
                operation.Dispose();             // ���ҽ� ����
                action.Enable(); // �����ε��� ���� �� �ٽ� Ȱ��ȭ
            })
            .Start();
    }

    // JSON ���Ϸκ��� ���ε��� Ű �� �ҷ�����
    private void LoadBindingOverrides()
    {
        string path = Path.Combine(Application.persistentDataPath, BindingFilePath);

        if (File.Exists(path))
        {
            string jsonData = File.ReadAllText(path);
            var bindingData = JsonUtility.FromJson<SerializableDictionary<string, string>>(jsonData);

            // �� InputAction�� ���� JSON ���ε� �ҷ�����
            foreach (var actionRef in inputActions)
            {
                if (bindingData.dictionary.TryGetValue(actionRef.action.name, out var json))
                {
                    actionRef.action.LoadBindingOverridesFromJson(json);
                }
            }
        }
    }

    // ���� ���� �� ���ε� ����
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

    // JSON ��ȯ �� �ʿ�
    public SerializableDictionary() { }
}