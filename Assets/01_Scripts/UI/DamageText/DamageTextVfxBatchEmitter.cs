using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.VFX;


public class DamageTextVfxBatchEmitter : MonoBehaviour
{
    public int MaxChars = 15;
    public int MaxTextCount = 100;
    public int MaxTotalChars = 1500;

    [Header("VFX Graph")]
    [SerializeField] private VisualEffect textVFX;

    [Header("Character Mapping")]
    [SerializeField] private SymbolsTextureData textureData;

    // Character index lookup table (x = charIndexInRow, y = textIndex)
    private Texture2D charIndexInRowTable;

    // Packed glyph lookup table (x = charIndex, y = textIndex)
    private Texture2D charTable;

    // Text parameter table
    private Texture2D paramTable;

    private readonly List<DefaultTextRequest> pendingDefaultRequests = new();
    private readonly List<DamageTextRequest> pendingDamageRequests = new();

    static readonly ProfilerMarker marker_WriteTextTexture_String = new("TextVFX.WriteTexture.String");
    static readonly ProfilerMarker marker_WriteTextTexture_Damage = new("TextVFX.WriteTexture.Damage");


    // Test button in Odin Inspector
    int testCounter = 0;
    [Button]
    public void Test()
    {
        Profiler.BeginSample("Editor.BakeSomething");

        try
        {
            testCounter++;

            for (int i = 0; i < 20; i++)
            {
                for (int j = 0; j < 20; j++)
                {
                    var emitParams = new TextEmitParams(
                        position: new Vector3((i * 10) + (testCounter % 2 == 0 ? 3 : 0f), j * 5f + (testCounter % 2 == 0 ? 1.5f : 0f), 0f),
                        lifetime: 2f,
                        fontSize: 1f,
                        fontColor: new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value, 1),
                        outlineColor: Color.black
                    );

                    var damageRequest = new DamageTextRequest(
                        damage: UnityEngine.Random.Range(-2000, 2000),
                        emitParams: emitParams
                    );

                    EnqueueText(damageRequest);
                }
            }
        }
        finally
        {
            Profiler.EndSample();
        }
    }
    private void InitializeTextures()
    {
        Debug.Log("InitializeTextures called");
        // Recreate textures on initialization and dispose previous allocations first.
        if (charIndexInRowTable != null) Destroy(charIndexInRowTable);
        if (charTable != null) Destroy(charTable);
        if (paramTable != null) Destroy(paramTable);

        MaxTotalChars = MaxChars * MaxTextCount;

        // Global spawn table (textIndex, charIndexInRow)
        charIndexInRowTable = new Texture2D(
            MaxTotalChars, 1,
            TextureFormat.RGFloat, false, true
        );
        charIndexInRowTable.filterMode = FilterMode.Point;
        charIndexInRowTable.wrapMode = TextureWrapMode.Clamp;

        // Packed glyph table
        charTable = new Texture2D(
            MaxChars, MaxTextCount,
            TextureFormat.RFloat, false, true
        );
        charTable.filterMode = FilterMode.Point;
        charTable.wrapMode = TextureWrapMode.Clamp;

        // Row 0: Position.xyz + Lifetime
        // Row 1: Color.rgb + FontSize
        // Row 2: CharLength
        paramTable = new Texture2D(
            MaxTextCount, 3,
            TextureFormat.RGBAFloat, false, true
        );
        paramTable.filterMode = FilterMode.Point;
        paramTable.wrapMode = TextureWrapMode.Clamp;
    }
    //public void EnqueueText(DefaultTextRequest request)
    //    => pendingDefaultRequests.Add(request);
    //public void EnqueueText(DamageTextRequest request)
    //    => pendingDamageRequests.Add(request);

    public void EnqueueText(DefaultTextRequest request) { }
    public void EnqueueText(DamageTextRequest request) { }

    private void LateUpdate()
    {
        if (pendingDefaultRequests.Count > 0 || pendingDamageRequests.Count > 0)
            FlushRequests();
    }
    private void FlushRequests()
    {
        #region Write String Text Requests To Texture

        //marker_WriteTextTexture_String.Begin();

        int textCount = Mathf.Min(pendingDefaultRequests.Count, MaxTextCount);
        int textIndex = 0;
        int spawnIndex = 0;

        for (; textIndex < textCount; textIndex++)
        {
            DefaultTextRequest req = pendingDefaultRequests[textIndex];

            int length = Mathf.Min(req.Message.Length, MaxChars);

            WriteCharRow(req.Message, length, textIndex);
            WriteParamRow(req.TextEmitParams, length, textIndex);

            // Populate the global spawn table (textIndex, charIndexInRow).
            for (int charIndex = 0; charIndex < length; charIndex++)
            {
                if (spawnIndex >= MaxTotalChars) break;

                charIndexInRowTable.SetPixel(
                    spawnIndex, 0,
                    new Color(textIndex, charIndex, 0, 0)
                );

                spawnIndex++;
            }
        }

        //marker_WriteTextTexture_String.End();

        #endregion

        #region Write Damage Text Requests To Texture

        textCount = Mathf.Min(textCount + pendingDamageRequests.Count, MaxTextCount);

        //marker_WriteTextTexture_Damage.Begin();

        for (; textIndex < textCount; textIndex++)
        {
            var req = pendingDamageRequests[textIndex - pendingDefaultRequests.Count];

            int damage = req.Damage;
            int digitCount = Mathf.Min(GetSignedDigitCount(damage), MaxChars);

            WriteDigitRow(damage, digitCount, textIndex);
            WriteParamRow(req.TextEmitParams, digitCount, textIndex);

            for (int charIndex = 0; charIndex < digitCount; charIndex++)
            {
                if (spawnIndex >= MaxTotalChars) break;

                charIndexInRowTable.SetPixel(
                    x: spawnIndex, y: 0,
                    new Color(textIndex, charIndex, 0, 0)
                );

                spawnIndex++;
            }
        }
        //marker_WriteTextTexture_Damage.End();

        #endregion

        #region Upload Textures And Dispatch VFX

        charIndexInRowTable.Apply(false, false);
        charTable.Apply(false, false);
        paramTable.Apply(false, false);

        textVFX.SetInt("TextCount", textCount);
        textVFX.SetInt("TotalCharCount", spawnIndex);

        textVFX.SetTexture("CharIndexInRowTable", charIndexInRowTable);
        textVFX.SetTexture("CharTable", charTable);
        textVFX.SetTexture("ParamTable", paramTable);

        textVFX.SendEvent("SpawnBatchText");

        pendingDamageRequests.Clear();
        pendingDefaultRequests.Clear();

        #endregion
    }
    private void WriteCharRow(string message, int length, int row)
    {
        for (int x = 0; x < length; x++)
        {
            float packed = 0f;

            if (x < length)
            {
                var uv = textureData.GetTextureCoordinates(message[x]);

                packed = Mathf.RoundToInt(uv.x) * 10 + Mathf.RoundToInt(uv.y);
            }

            charTable.SetPixel(x, row, new Color(packed, 0, 0, 0));
        }
    }
    void WriteDigitRow(int value, int length, int row)
    {
        bool isNegative = value < 0;
        int absValue = isNegative ? -value : value;

        for (int x = 0; x < length; x++)
        {
            float packed = 0f;

            if (isNegative && x == 0)
            {
                // First character is '-'
                char c = '-';
                var uv = textureData.GetTextureCoordinates(c);
                packed = Mathf.RoundToInt(uv.x) * 10 + Mathf.RoundToInt(uv.y);
            }
            else
            {
                int digitIndex = length - 1 - x;

                if (isNegative)
                    digitIndex--; // Reserve one slot for '-'

                int digit = GetDigitAt(absValue, digitIndex);
                char c = (char)('0' + digit);

                var uv = textureData.GetTextureCoordinates(c);
                packed = Mathf.RoundToInt(uv.x) * 10 + Mathf.RoundToInt(uv.y);
            }

            charTable.SetPixel(x, row, new Color(packed, 0, 0, 0));
        }
    }
    private int GetDigitAt(int value, int indexFromRight)
    {
        for (int i = 0; i < indexFromRight; i++) value /= 10;

        return value % 10;
    }
    private int GetSignedDigitCount(int value)
    {
        if (value == 0) return 1;

        int count = 0;

        if (value < 0)
        {
            count++;        // '-' slot
            value = -value; // absolute value
        }

        while (value != 0)
        {
            value /= 10;
            count++;
        }

        return count;
    }
    private void WriteParamRow(TextEmitParams emitParams, int length, int index)
    {
        // Position rgb + lifetime a
        paramTable.SetPixel(
            index, 0,
            new Color(emitParams.Position.x, emitParams.Position.y, emitParams.Position.z, emitParams.Lifetime)
        );

        // Font color rgb + font size a
        paramTable.SetPixel(
            index, 1,
            new Color(emitParams.FontColor.r, emitParams.FontColor.g, emitParams.FontColor.b, emitParams.FontSize)
        );
        // Outline color rgb + character count a
        paramTable.SetPixel(
            index, 2,
            new Color(emitParams.OutlineColor.r, emitParams.OutlineColor.g, emitParams.OutlineColor.b, length)
        );
    }
    #region Unity Callbacks
    private void Awake()
    {
        InitializeTextures();
    }
    private void OnValidate()
    {
        //InitializeTextures();

        //EditorUtility.SetDirty(this);
    }
    private void OnDestroy()
    {
        if (charIndexInRowTable != null)
        {
            Destroy(charIndexInRowTable);
            charIndexInRowTable = null;
        }

        if (charTable != null)
        {
            Destroy(charTable);
            charTable = null;
        }

        if (paramTable != null)
        {
            Destroy(paramTable);
            paramTable = null;
        }
    }
    #endregion

    #region Text Emit Request Data Structures
    // Text emit request data structures passed to VFX / UI output.
    // - TextEmitParams: common visual parameters for emitted text.
    // - DefaultTextRequest: request for generic string text.
    // - DamageTextRequest: request for integer damage text.

    [Serializable]
    public struct TextEmitParams
    {
        public Color FontColor;
        public Color OutlineColor;
        public float FontSize;
        public float Lifetime;
        public Vector3 Position;

        public TextEmitParams(Vector3 position, float lifetime, float fontSize, Color fontColor, Color? outlineColor)
        {
            Position = position;
            Lifetime = lifetime;
            FontSize = fontSize;
            FontColor = fontColor;
            OutlineColor = outlineColor ?? fontColor;
        }
    }
    public struct DefaultTextRequest
    {
        public string Message;
        public TextEmitParams TextEmitParams;
        public DefaultTextRequest(string message, TextEmitParams emitParams)
        {
            Message = message;

            TextEmitParams = emitParams;
        }
    }
    public struct DamageTextRequest
    {
        public int Damage;
        public TextEmitParams TextEmitParams;
        public DamageTextRequest(int damage, TextEmitParams emitParams)
        {
            Damage = damage;

            TextEmitParams = emitParams;
        }
    }

    #endregion

    [Serializable]
    public class SymbolsTextureData
    {
        public char[] chars;

        private Dictionary<char, Vector2> charsDict;

        public void Initialize()
        {
            charsDict = new Dictionary<char, Vector2>();

            charsDict.EnsureCapacity(56);

            for (int i = 0; i < chars.Length; i++)
            {
                var c = char.ToLowerInvariant(chars[i]);
                if (charsDict.ContainsKey(c)) continue;

                var uv = new Vector2(i % 10, 9 - i / 10);
                charsDict.Add(c, uv);
            }
        }
        public Vector2 GetTextureCoordinates(char c)
        {
            c = char.ToLowerInvariant(c);
            if (charsDict == null) Initialize();
            return charsDict.TryGetValue(c, out var uv) ? uv : Vector2.zero;
        }
    }
}
