using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinimapUI : MonoBehaviour
{
    [Header("Layout")]
    [SerializeField] RectTransform markerRoot;
    [SerializeField] MinimapBlockGraphic blockGraphic;
    [SerializeField] float worldRadius = 70f;
    [SerializeField] float worldBoundsPadding = 1.5f;
    [SerializeField] bool hideOutOfRangeMarkers = true;

    [Header("Scan")]
    [SerializeField] float scanInterval = 0.4f;
    [SerializeField] int maxEnemyMarkers = 80;

    [Header("Sprites")]
    [SerializeField] Sprite playerSprite;
    [SerializeField] Sprite enemySprite;
    [SerializeField] Sprite bossSprite;
    [SerializeField] Sprite crystalSprite;
    [SerializeField] Sprite blockSprite;

    [Header("Marker Size")]
    [SerializeField] Vector2 playerMarkerSize = new Vector2(18f, 22f);
    [SerializeField] Vector2 enemyMarkerSize = new Vector2(12f, 14f);
    [SerializeField] Vector2 bossMarkerSize = new Vector2(20f, 22f);
    [SerializeField] Vector2 crystalMarkerSize = new Vector2(14f, 22f);
    [SerializeField] Vector2 blockMarkerSize = new Vector2(10f, 5f);

    readonly Dictionary<CustomMonster, Image> enemyMarkers = new Dictionary<CustomMonster, Image>();
    readonly Stack<Image> dynamicMarkerPool = new Stack<Image>();
    readonly List<Vector2> blockWorldOffsets = new List<Vector2>();
    Vector2 mapWorldExtents = new Vector2(70f, 70f);

    Transform playerTransform;
    Transform crystalTransform;
    Image playerMarker;
    Image crystalMarker;
    float nextScanTime;

    void Awake()
    {
        if (markerRoot == null)
            markerRoot = transform as RectTransform;

        if (blockGraphic == null)
            blockGraphic = GetComponentInChildren<MinimapBlockGraphic>(true);

        EnsureStaticMarkers();
    }

    void OnEnable()
    {
        nextScanTime = 0f;
    }

    void Update()
    {
        if (markerRoot == null)
            return;

        if (Time.unscaledTime >= nextScanTime)
        {
            ResolveStaticTargets();
            SyncBlockMarkers();
            SyncEnemyMarkers();
            nextScanTime = Time.unscaledTime + scanInterval;
        }

        UpdateStaticMarkers();
        UpdateEnemyMarkerPositions();
    }

    void EnsureStaticMarkers()
    {
        if (markerRoot == null)
            return;

        if (playerMarker == null)
            playerMarker = CreateMarker("PlayerMarker", playerSprite, playerMarkerSize);

        if (crystalMarker == null)
            crystalMarker = CreateMarker("CrystalMarker", crystalSprite, crystalMarkerSize);
    }

    void ResolveStaticTargets()
    {
        if (GameManager.Instance != null)
            playerTransform = GameManager.GetUserTransform();

        if (playerTransform == null)
        {
            PlayerAttributes playerAttributes = FindFirstObjectByType<PlayerAttributes>();
            if (playerAttributes != null)
                playerTransform = playerAttributes.transform;
        }

        if (crystalTransform == null)
        {
            CoreHealth coreHealth = FindFirstObjectByType<CoreHealth>();
            if (coreHealth != null)
                crystalTransform = coreHealth.transform;
        }
    }

    void SyncBlockMarkers()
    {
        if (crystalTransform == null)
        {
            blockWorldOffsets.Clear();
            mapWorldExtents = new Vector2(worldRadius, worldRadius);
            if (blockGraphic != null)
                blockGraphic.ClearMarkers();
            return;
        }

        blockWorldOffsets.Clear();
        mapWorldExtents = Vector2.one;
        BaseBlock[] blocks = FindObjectsByType<BaseBlock>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        for (int i = 0; i < blocks.Length; i++)
        {
            BaseBlock block = blocks[i];
            if (block == null || !block.isActiveAndEnabled || !block.IsAlive())
                continue;

            Vector3 offset = block.transform.position - crystalTransform.position;
            Vector2 offset2 = new Vector2(offset.x, offset.z);
            blockWorldOffsets.Add(offset2);
            mapWorldExtents.x = Mathf.Max(mapWorldExtents.x, Mathf.Abs(offset2.x));
            mapWorldExtents.y = Mathf.Max(mapWorldExtents.y, Mathf.Abs(offset2.y));
        }

        mapWorldExtents += Vector2.one * Mathf.Max(0f, worldBoundsPadding);

        if (blockGraphic != null)
            blockGraphic.SetMarkers(blockWorldOffsets, blockSprite, blockMarkerSize, mapWorldExtents);
    }

    void SyncEnemyMarkers()
    {
        Transform centerTransform = GetMapCenterTransform();
        if (centerTransform == null)
        {
            RemoveAllMonsterMarkers();
            return;
        }

        List<CustomMonster> visibleMonsters = new List<CustomMonster>();
        CustomMonster[] monsters = FindObjectsByType<CustomMonster>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        float radiusSqr = worldRadius * worldRadius;

        for (int i = 0; i < monsters.Length; i++)
        {
            CustomMonster monster = monsters[i];
            if (monster == null || !monster.isActiveAndEnabled || !monster.IsAlive())
                continue;

            Vector3 offset = monster.transform.position - centerTransform.position;
            if (offset.sqrMagnitude > radiusSqr)
                continue;

            visibleMonsters.Add(monster);
            if (visibleMonsters.Count >= maxEnemyMarkers)
                break;
        }

        HashSet<CustomMonster> visibleMonsterSet = new HashSet<CustomMonster>(visibleMonsters);
        RemoveMissingMonsterMarkers(visibleMonsterSet);

        for (int i = 0; i < visibleMonsters.Count; i++)
        {
            CustomMonster monster = visibleMonsters[i];
            if (!enemyMarkers.ContainsKey(monster))
            {
                Sprite markerSprite = monster.isBossMonster ? bossSprite : enemySprite;
                Vector2 markerSize = monster.isBossMonster ? bossMarkerSize : enemyMarkerSize;
                enemyMarkers.Add(monster, GetDynamicMarker(monster.isBossMonster ? "BossMarker" : "EnemyMarker", markerSprite, markerSize));
            }
        }
    }

    void UpdateStaticMarkers()
    {
        if (playerMarker != null)
        {
            UpdateMarker(playerMarker, playerTransform, playerMarkerSize, false);
            RotatePlayerMarker();
        }

        if (crystalMarker != null)
            UpdateMarker(crystalMarker, crystalTransform, crystalMarkerSize, false);
    }

    void UpdateEnemyMarkerPositions()
    {
        foreach (KeyValuePair<CustomMonster, Image> marker in enemyMarkers)
        {
            CustomMonster monster = marker.Key;
            Vector2 size = monster != null && monster.isBossMonster ? bossMarkerSize : enemyMarkerSize;
            UpdateMarker(marker.Value, monster != null ? monster.transform : null, size, hideOutOfRangeMarkers);
        }
    }

    void UpdateMarker(Image image, Transform target, Vector2 size, bool hideWhenOutOfRange)
    {
        if (image == null)
            return;

        RectTransform rectTransform = image.rectTransform;
        rectTransform.sizeDelta = size;

        Transform centerTransform = GetMapCenterTransform();
        if (centerTransform == null || target == null || !target.gameObject.activeInHierarchy)
        {
            image.enabled = false;
            return;
        }

        Vector3 offset3 = target.position - centerTransform.position;
        Vector2 offset = new Vector2(offset3.x, offset3.z);
        float distance = offset.magnitude;

        if (hideWhenOutOfRange && distance > worldRadius)
        {
            image.enabled = false;
            return;
        }

        image.enabled = true;

        Vector2 halfSize = markerRoot.rect.size * 0.5f;
        Vector2 drawableHalfSize = new Vector2(
            Mathf.Max(1f, halfSize.x - size.x * 0.5f),
            Mathf.Max(1f, halfSize.y - size.y * 0.5f));
        Vector2 extents = GetMapWorldExtents();
        Vector2 localOffset = new Vector2(
            offset.x / extents.x * drawableHalfSize.x,
            offset.y / extents.y * drawableHalfSize.y);

        if (!hideWhenOutOfRange)
        {
            localOffset.x = Mathf.Clamp(localOffset.x, -drawableHalfSize.x, drawableHalfSize.x);
            localOffset.y = Mathf.Clamp(localOffset.y, -drawableHalfSize.y, drawableHalfSize.y);
        }

        rectTransform.anchoredPosition = localOffset;
    }

    Vector2 GetMapWorldExtents()
    {
        return new Vector2(
            Mathf.Max(1f, mapWorldExtents.x),
            Mathf.Max(1f, mapWorldExtents.y));
    }

    Transform GetMapCenterTransform()
    {
        return crystalTransform != null ? crystalTransform : playerTransform;
    }

    void RotatePlayerMarker()
    {
        if (playerMarker == null || playerTransform == null)
            return;

        playerMarker.rectTransform.localEulerAngles = new Vector3(0f, 0f, -playerTransform.eulerAngles.y);
    }

    Image CreateMarker(string markerName, Sprite sprite, Vector2 size)
    {
        GameObject markerObject = new GameObject(markerName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        markerObject.transform.SetParent(markerRoot, false);

        Image image = markerObject.GetComponent<Image>();
        image.sprite = sprite;
        image.raycastTarget = false;
        image.preserveAspect = true;

        RectTransform rectTransform = image.rectTransform;
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = size;
        rectTransform.localEulerAngles = Vector3.zero;

        return image;
    }

    Image GetDynamicMarker(string markerName, Sprite sprite, Vector2 size)
    {
        Image image = dynamicMarkerPool.Count > 0 ? dynamicMarkerPool.Pop() : CreateMarker(markerName, sprite, size);
        image.gameObject.name = markerName;
        image.gameObject.SetActive(true);
        image.sprite = sprite;
        image.enabled = true;
        image.rectTransform.sizeDelta = size;
        image.rectTransform.localEulerAngles = Vector3.zero;
        return image;
    }

    void ReleaseDynamicMarker(Image marker)
    {
        if (marker == null)
            return;

        marker.enabled = false;
        marker.gameObject.SetActive(false);
        dynamicMarkerPool.Push(marker);
    }

    void RemoveMissingMonsterMarkers(HashSet<CustomMonster> aliveTargets)
    {
        List<CustomMonster> removeTargets = new List<CustomMonster>();

        foreach (CustomMonster monster in enemyMarkers.Keys)
        {
            if (monster == null || !aliveTargets.Contains(monster))
                removeTargets.Add(monster);
        }

        for (int i = 0; i < removeTargets.Count; i++)
        {
            CustomMonster monster = removeTargets[i];
            ReleaseDynamicMarker(enemyMarkers[monster]);

            enemyMarkers.Remove(monster);
        }
    }

    void RemoveAllMonsterMarkers()
    {
        foreach (Image marker in enemyMarkers.Values)
            ReleaseDynamicMarker(marker);

        enemyMarkers.Clear();
    }
}
