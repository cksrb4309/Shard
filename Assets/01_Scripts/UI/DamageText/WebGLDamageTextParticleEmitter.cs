using System.Collections.Generic;
using UnityEngine;

public class WebGLDamageTextParticleEmitter : MonoBehaviour
{
    private const string WebGLDamageTextShaderName = "Hidden/Shard/WebGLDamageTextAtlasParticle";
    private const int AtlasGridSize = 10;
    private const int MaxRenderCharacters = 12;
    private const int LetterAtlasStartIndex = 13; // 'A'
    private const int DigitAtlasStartIndex = 46;  // '0'
    private const float AtlasCellSize = 1f / AtlasGridSize;

    [Header("Particle Systems")]
    [SerializeField] private ParticleSystem normalDamageParticleSystem;
    [SerializeField] private ParticleSystem criticalDamageParticleSystem;
    [SerializeField] private ParticleSystem userDamageParticleSystem;
    [SerializeField] private ParticleSystem deadTextParticleSystem;

    [Header("Atlas (monospaced_font.png)")]
    [SerializeField] private Texture2D atlasTexture;
    [SerializeField] private Texture2D outlineAtlasTexture;

    [Header("Outline")]
    [SerializeField] private Color outlineColor = new Color(0f, 0f, 0f, 1f);
    [Range(0f, 2f)]
    [SerializeField] private float outlineStrength = 1f;

    [Header("Emission Tuning")]
    [SerializeField] private float characterSpacing = 0.22f;
    [SerializeField] private float characterSpacingScale = 1f;
    [SerializeField] private float horizontalJitter = 0f;
    [SerializeField] private Vector2 sideDriftSpeedRange = new Vector2(0f, 0f);
    [SerializeField] private Vector2 upwardSpeedRange = new Vector2(1.3f, 1.3f);
    [SerializeField] private float baseLifetime = 0.6f;
    [SerializeField] private float baseSize = 0.2f;
    [SerializeField] private float sizePerCharacter = 0.03f;
    [SerializeField] private float criticalSizeMultiplier = 1.25f;
    [SerializeField] private float userSizeMultiplier = 1f;
    [SerializeField] private float deadSizeMultiplier = 1f;
    [SerializeField] private float textScaleMultiplier = 2f;

    [Header("Per-Type Colors")]
    [SerializeField] private Color normalColor = new Color(0.6f, 0.75f, 1f, 1f);
    [SerializeField] private Color criticalColor = new Color(1f, 0.76f, 0.2f, 1f);
    [SerializeField] private Color userColor = new Color(1f, 0.45f, 0.45f, 1f);
    [SerializeField] private Color deadColor = new Color(1f, 1f, 1f, 1f);

    private readonly Dictionary<ParticleSystem, Dictionary<uint, Vector2>> pendingGlyphCellBySeed = new Dictionary<ParticleSystem, Dictionary<uint, Vector2>>();
    private readonly Dictionary<ParticleSystem, ParticleSystem.Particle[]> particleBuffers = new Dictionary<ParticleSystem, ParticleSystem.Particle[]>();
    private readonly List<Vector4> customDataBuffer = new List<Vector4>();
    private readonly List<uint> removalBuffer = new List<uint>();
    private readonly HashSet<uint> activeSeedBuffer = new HashSet<uint>();
    private readonly List<ParticleSystemVertexStream> vertexStreams = new List<ParticleSystemVertexStream>();
    private uint emitSeedCounter = 10000u;
    private Material runtimeMaterial;
    private bool initializationWarningLogged;
    private bool customDataApplyWarningLogged;

    public static WebGLDamageTextParticleEmitter CreateRuntimeEmitter(Transform parent)
    {
        GameObject emitterObject = new GameObject("WebGLDamageTextEmitter");
        emitterObject.transform.SetParent(parent, false);
        WebGLDamageTextParticleEmitter emitter = emitterObject.AddComponent<WebGLDamageTextParticleEmitter>();
        emitter.InitializeRuntimeParticleSystems();
        return emitter;
    }

    private void Awake()
    {
        EnsureInitialized();
    }

    private void OnValidate()
    {
        ApplyRuntimeMaterialProperties();
    }

    public bool TryEmit(Vector3 worldPosition, float damage, bool isUser, bool isCritical, Vector3 randomOffset)
    {
        EnsureInitialized();

        ParticleSystem targetSystem = SelectParticleSystem(damage, isUser, isCritical);
        if (targetSystem == null || runtimeMaterial == null)
        {
            return false;
        }

        string displayText = GetDisplayText(damage);
        if (!TryBuildGlyphCellList(displayText, out List<Vector2> glyphCells))
        {
            return false;
        }

        Color startColor = GetStartColor(damage, isUser, isCritical);
        float typeScaleMultiplier = GetTypeScaleMultiplier(damage, isUser, isCritical);
        float totalScaleMultiplier = textScaleMultiplier * typeScaleMultiplier;
        float effectiveSpacing = characterSpacing * totalScaleMultiplier * characterSpacingScale;
        float startX = -(glyphCells.Count - 1) * 0.5f * effectiveSpacing;
        float startSize = baseSize + ((glyphCells.Count - 1) * sizePerCharacter);
        startSize *= totalScaleMultiplier;

        Transform cameraTransform = Camera.main != null ? Camera.main.transform : null;
        Vector3 upDirection = cameraTransform != null ? cameraTransform.up : Vector3.up;
        Vector3 rightDirection = cameraTransform != null ? cameraTransform.right : Vector3.right;
        float riseSpeed = Random.Range(upwardSpeedRange.x, upwardSpeedRange.y);
        float sideDriftSpeed = Random.Range(sideDriftSpeedRange.x, sideDriftSpeedRange.y);
        Vector3 textVelocity = (upDirection * riseSpeed) + (rightDirection * sideDriftSpeed);
        float textHorizontalJitter = Random.Range(-horizontalJitter, horizontalJitter);

        Dictionary<uint, Vector2> glyphMap = GetOrCreateGlyphMap(targetSystem);
        for (int i = 0; i < glyphCells.Count; i++)
        {
            uint seed = ++emitSeedCounter;
            if (seed == 0u)
            {
                seed = ++emitSeedCounter;
            }

            float horizontalOffset = startX + (i * effectiveSpacing);

            ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams
            {
                position = worldPosition
                    + randomOffset
                    + (rightDirection * horizontalOffset)
                    + (rightDirection * textHorizontalJitter),
                velocity = textVelocity,
                startLifetime = baseLifetime,
                startSize = startSize,
                startColor = startColor,
                randomSeed = seed
            };

            targetSystem.Emit(emitParams, 1);
            glyphMap[seed] = glyphCells[i];
        }

        return true;
    }

    private void LateUpdate()
    {
        ApplyGlyphCellsToAliveParticles(normalDamageParticleSystem);
        ApplyGlyphCellsToAliveParticles(criticalDamageParticleSystem);
        ApplyGlyphCellsToAliveParticles(userDamageParticleSystem);
        ApplyGlyphCellsToAliveParticles(deadTextParticleSystem);
    }

    private void InitializeRuntimeParticleSystems()
    {
        if (normalDamageParticleSystem != null)
        {
            return;
        }

        normalDamageParticleSystem = CreateChildParticleSystem("NormalTextParticle");
        criticalDamageParticleSystem = CreateChildParticleSystem("CriticalTextParticle");
        userDamageParticleSystem = CreateChildParticleSystem("UserTextParticle");
        deadTextParticleSystem = CreateChildParticleSystem("DeadTextParticle");
    }

    private void EnsureInitialized()
    {
        if (runtimeMaterial != null)
        {
            return;
        }

        Shader shader = Shader.Find(WebGLDamageTextShaderName);
        if (shader == null || atlasTexture == null)
        {
            if (!initializationWarningLogged)
            {
                initializationWarningLogged = true;
                Debug.LogWarning("[WebGLDamageTextParticleEmitter] Shader or atlas texture is missing. Falling back to non-particle damage text path.");
            }
            return;
        }

        runtimeMaterial = new Material(shader)
        {
            name = "WebGLDamageTextMaterial (Runtime)"
        };
        ApplyRuntimeMaterialProperties();

        ConfigureParticleSystem(normalDamageParticleSystem);
        ConfigureParticleSystem(criticalDamageParticleSystem);
        ConfigureParticleSystem(userDamageParticleSystem);
        ConfigureParticleSystem(deadTextParticleSystem);
    }

    private void ConfigureParticleSystem(ParticleSystem particleSystem)
    {
        if (particleSystem == null)
        {
            return;
        }

        var main = particleSystem.main;
        main.playOnAwake = false;
        main.loop = false;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 4096;

        var emission = particleSystem.emission;
        emission.enabled = false;

        var shape = particleSystem.shape;
        shape.enabled = false;

        var customData = particleSystem.customData;
        customData.enabled = true;
        customData.SetMode(ParticleSystemCustomData.Custom1, ParticleSystemCustomDataMode.Vector);
        customData.SetVectorComponentCount(ParticleSystemCustomData.Custom1, 4);

        var renderer = particleSystem.GetComponent<ParticleSystemRenderer>();
        if (renderer == null)
        {
            return;
        }

        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.alignment = ParticleSystemRenderSpace.View;
        renderer.sortMode = ParticleSystemSortMode.YoungestInFront;
        renderer.material = runtimeMaterial;
        ConfigureVertexStreams(renderer);
    }

    private void ConfigureVertexStreams(ParticleSystemRenderer renderer)
    {
        vertexStreams.Clear();
        renderer.GetActiveVertexStreams(vertexStreams);

        EnsureVertexStream(renderer, ParticleSystemVertexStream.Position);
        EnsureVertexStream(renderer, ParticleSystemVertexStream.Normal);
        EnsureVertexStream(renderer, ParticleSystemVertexStream.Color);
        EnsureVertexStream(renderer, ParticleSystemVertexStream.UV);
        EnsureVertexStream(renderer, ParticleSystemVertexStream.Custom1XYZW);
    }

    private void EnsureVertexStream(ParticleSystemRenderer renderer, ParticleSystemVertexStream stream)
    {
        if (!vertexStreams.Contains(stream))
        {
            vertexStreams.Add(stream);
            renderer.SetActiveVertexStreams(vertexStreams);
        }
    }

    private void ApplyGlyphCellsToAliveParticles(ParticleSystem particleSystem)
    {
        if (particleSystem == null || !pendingGlyphCellBySeed.TryGetValue(particleSystem, out Dictionary<uint, Vector2> glyphMap))
        {
            return;
        }

        if (glyphMap.Count == 0)
        {
            return;
        }

        ParticleSystem.Particle[] particles = GetParticleBuffer(particleSystem);
        int aliveCount = particleSystem.GetParticles(particles);
        if (aliveCount <= 0)
        {
            glyphMap.Clear();
            return;
        }

        customDataBuffer.Clear();
        particleSystem.GetCustomParticleData(customDataBuffer, ParticleSystemCustomData.Custom1);
        while (customDataBuffer.Count < aliveCount)
        {
            customDataBuffer.Add(Vector4.zero);
        }
        if (customDataBuffer.Count > aliveCount)
        {
            customDataBuffer.RemoveRange(aliveCount, customDataBuffer.Count - aliveCount);
        }

        activeSeedBuffer.Clear();
        int assignedGlyphCount = 0;

        for (int i = 0; i < aliveCount; i++)
        {
            uint seed = particles[i].randomSeed;
            activeSeedBuffer.Add(seed);

            if (glyphMap.TryGetValue(seed, out Vector2 cell))
            {
                customDataBuffer[i] = new Vector4(cell.x, cell.y, 0f, 0f);
                assignedGlyphCount++;
            }
        }

        particleSystem.SetCustomParticleData(customDataBuffer, ParticleSystemCustomData.Custom1);

        if (!customDataApplyWarningLogged && glyphMap.Count > 0 && assignedGlyphCount == 0)
        {
            customDataApplyWarningLogged = true;
            Debug.LogWarning("[WebGLDamageTextParticleEmitter] Failed to map emitted particles to glyph data. Check randomSeed mapping and custom vertex stream configuration.");
        }

        removalBuffer.Clear();
        foreach (KeyValuePair<uint, Vector2> pair in glyphMap)
        {
            if (!activeSeedBuffer.Contains(pair.Key))
            {
                removalBuffer.Add(pair.Key);
            }
        }

        for (int i = 0; i < removalBuffer.Count; i++)
        {
            glyphMap.Remove(removalBuffer[i]);
        }
    }

    private ParticleSystem.Particle[] GetParticleBuffer(ParticleSystem particleSystem)
    {
        if (particleBuffers.TryGetValue(particleSystem, out ParticleSystem.Particle[] cachedBuffer))
        {
            return cachedBuffer;
        }

        int size = Mathf.Max(512, particleSystem.main.maxParticles);
        ParticleSystem.Particle[] newBuffer = new ParticleSystem.Particle[size];
        particleBuffers[particleSystem] = newBuffer;
        return newBuffer;
    }

    private Dictionary<uint, Vector2> GetOrCreateGlyphMap(ParticleSystem particleSystem)
    {
        if (!pendingGlyphCellBySeed.TryGetValue(particleSystem, out Dictionary<uint, Vector2> glyphMap))
        {
            glyphMap = new Dictionary<uint, Vector2>(512);
            pendingGlyphCellBySeed[particleSystem] = glyphMap;
        }

        return glyphMap;
    }

    private ParticleSystem CreateChildParticleSystem(string systemName)
    {
        GameObject child = new GameObject(systemName);
        child.transform.SetParent(transform, false);
        ParticleSystem particleSystem = child.AddComponent<ParticleSystem>();
        return particleSystem;
    }

    private ParticleSystem SelectParticleSystem(float damage, bool isUser, bool isCritical)
    {
        if (damage == float.MaxValue)
        {
            return deadTextParticleSystem != null ? deadTextParticleSystem : normalDamageParticleSystem;
        }

        if (isUser)
        {
            return userDamageParticleSystem != null ? userDamageParticleSystem : normalDamageParticleSystem;
        }

        if (isCritical)
        {
            return criticalDamageParticleSystem != null ? criticalDamageParticleSystem : normalDamageParticleSystem;
        }

        return normalDamageParticleSystem;
    }

    private Color GetStartColor(float damage, bool isUser, bool isCritical)
    {
        if (damage == float.MaxValue)
        {
            return deadColor;
        }

        if (isUser)
        {
            return userColor;
        }

        return isCritical ? criticalColor : normalColor;
    }

    private float GetTypeScaleMultiplier(float damage, bool isUser, bool isCritical)
    {
        if (damage == float.MaxValue)
        {
            return deadSizeMultiplier;
        }

        if (isUser)
        {
            return userSizeMultiplier;
        }

        if (isCritical)
        {
            return criticalSizeMultiplier;
        }

        return 1f;
    }

    private static string GetDisplayText(float damage)
    {
        if (damage == float.MaxValue)
        {
            return "DEAD";
        }

        return ((int)damage).ToString();
    }

    private bool TryBuildGlyphCellList(string source, out List<Vector2> glyphCells)
    {
        glyphCells = new List<Vector2>(MaxRenderCharacters);
        string upper = source.ToUpperInvariant();

        for (int i = 0; i < upper.Length && glyphCells.Count < MaxRenderCharacters; i++)
        {
            if (TryGetGlyphCell(upper[i], out Vector2 cell))
            {
                glyphCells.Add(cell);
            }
        }

        return glyphCells.Count > 0;
    }

    private bool TryGetGlyphCell(char c, out Vector2 cell)
    {
        if (c >= 'A' && c <= 'Z')
        {
            return TryGetCellFromAtlasIndex(LetterAtlasStartIndex + (c - 'A'), out cell);
        }

        if (c >= '0' && c <= '9')
        {
            return TryGetCellFromAtlasIndex(DigitAtlasStartIndex + (c - '0'), out cell);
        }

        if (c == ' ')
        {
            cell = Vector2.zero;
            return true;
        }

        cell = default;
        return false;
    }

    private bool TryGetCellFromAtlasIndex(int atlasIndex, out Vector2 cell)
    {
        if (atlasIndex < 0 || atlasIndex >= AtlasGridSize * AtlasGridSize)
        {
            cell = default;
            return false;
        }

        int x = atlasIndex % AtlasGridSize;
        int y = (AtlasGridSize - 1) - (atlasIndex / AtlasGridSize);
        cell = new Vector2(x, y);
        return true;
    }
    private void ApplyRuntimeMaterialProperties()
    {
        if (runtimeMaterial == null)
        {
            return;
        }

        runtimeMaterial.SetTexture("_MainTex", atlasTexture);
        runtimeMaterial.SetTexture("_OutlineTex", outlineAtlasTexture);
        runtimeMaterial.SetColor("_OutlineColor", outlineColor);
        runtimeMaterial.SetFloat("_OutlineStrength", outlineStrength);
        runtimeMaterial.SetFloat("_CellSize", AtlasCellSize);
    }
}
