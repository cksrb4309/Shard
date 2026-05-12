using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinimapBlockGraphic : MaskableGraphic
{
    [SerializeField] Sprite markerSprite;
    [SerializeField] Vector2 markerSize = new Vector2(10f, 5f);
    [SerializeField] Vector2 worldExtents = new Vector2(90f, 90f);

    readonly List<Vector2> worldOffsets = new List<Vector2>();

    public override Texture mainTexture
    {
        get
        {
            if (markerSprite != null)
                return markerSprite.texture;

            return s_WhiteTexture;
        }
    }

    public void SetMarkers(IReadOnlyList<Vector2> offsets, Sprite sprite, Vector2 size, Vector2 extents)
    {
        markerSprite = sprite;
        markerSize = size;
        worldExtents = new Vector2(Mathf.Max(1f, extents.x), Mathf.Max(1f, extents.y));

        worldOffsets.Clear();
        for (int i = 0; i < offsets.Count; i++)
            worldOffsets.Add(offsets[i]);

        SetVerticesDirty();
        SetMaterialDirty();
    }

    public void ClearMarkers()
    {
        if (worldOffsets.Count == 0)
            return;

        worldOffsets.Clear();
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        if (worldOffsets.Count == 0)
            return;

        Rect rect = rectTransform.rect;
        Vector2 halfSize = rect.size * 0.5f;
        Vector2 drawableHalfSize = new Vector2(
            Mathf.Max(1f, halfSize.x - markerSize.x * 0.5f),
            Mathf.Max(1f, halfSize.y - markerSize.y * 0.5f));
        Rect uv = GetSpriteUv();
        Color32 vertexColor = color;

        for (int i = 0; i < worldOffsets.Count; i++)
        {
            Vector2 localPosition = new Vector2(
                worldOffsets[i].x / worldExtents.x * drawableHalfSize.x,
                worldOffsets[i].y / worldExtents.y * drawableHalfSize.y);
            AddQuad(vh, localPosition, markerSize, uv, vertexColor);
        }
    }

    Rect GetSpriteUv()
    {
        if (markerSprite == null || markerSprite.texture == null)
            return new Rect(0f, 0f, 1f, 1f);

        Rect textureRect = markerSprite.textureRect;
        Texture texture = markerSprite.texture;

        return new Rect(
            textureRect.xMin / texture.width,
            textureRect.yMin / texture.height,
            textureRect.width / texture.width,
            textureRect.height / texture.height);
    }

    static void AddQuad(VertexHelper vh, Vector2 center, Vector2 size, Rect uv, Color32 vertexColor)
    {
        int startIndex = vh.currentVertCount;
        Vector2 half = size * 0.5f;

        vh.AddVert(new Vector3(center.x - half.x, center.y - half.y), vertexColor, new Vector2(uv.xMin, uv.yMin));
        vh.AddVert(new Vector3(center.x - half.x, center.y + half.y), vertexColor, new Vector2(uv.xMin, uv.yMax));
        vh.AddVert(new Vector3(center.x + half.x, center.y + half.y), vertexColor, new Vector2(uv.xMax, uv.yMax));
        vh.AddVert(new Vector3(center.x + half.x, center.y - half.y), vertexColor, new Vector2(uv.xMax, uv.yMin));

        vh.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
        vh.AddTriangle(startIndex + 2, startIndex + 3, startIndex);
    }
}
