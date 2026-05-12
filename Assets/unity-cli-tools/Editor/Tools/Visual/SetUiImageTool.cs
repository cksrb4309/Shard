using System;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityCliConnector;
using UnityCliTools.Core;
using UnityCliTools.Infrastructure.Json;
using UnityCliTools.Infrastructure.Responses;
using UnityCliTools.Infrastructure.Ui;

namespace UnityCliTools.Visual
{

[UnityCliTool(
    Name = UnityCliToolNames.SetUiImage,
    Description = "Set Image sprite, color, material, raycast target, or image type.",
    Group = UnityCliToolGroups.Visual)]
public static class SetUiImageTool
{
    public sealed class Parameters
    {
        [ToolParameter("Loaded scene path containing the target object.")]
        public string ScenePath { get; set; }

        [ToolParameter("Target UI GameObject hierarchy path.", Required = true)]
        public string HierarchyPath { get; set; }

        [ToolParameter("Sprite asset path.")]
        public string SpritePath { get; set; }

        [ToolParameter("Sprite asset GUID.")]
        public string SpriteGuid { get; set; }

        [ToolParameter("Color as r,g,b,a.")]
        public string Color { get; set; }

        [ToolParameter("Material asset path.")]
        public string MaterialPath { get; set; }

        [ToolParameter("Image type enum value.")]
        public string ImageType { get; set; }

        [ToolParameter("Graphic raycastTarget value.")]
        public string RaycastTarget { get; set; }

        [ToolParameter("Preview only without modifying the object.", DefaultValue = "true")]
        public bool DryRun { get; set; }
    }

    public static object HandleCommand(JObject parameters)
    {
        try
        {
            var p = new ToolParams(parameters);
            var scenePath = p.Get("scenePath", string.Empty);
            var hierarchyPath = p.Get("hierarchyPath", string.Empty)?.Trim();
            var spritePath = p.Get("spritePath", string.Empty)?.Trim();
            var spriteGuid = p.Get("spriteGuid", string.Empty)?.Trim();
            var materialPath = UnityCliToolShared.TryConvertToAssetPath(p.Get("materialPath", string.Empty));
            var imageTypeText = p.Get("imageType", string.Empty)?.Trim();
            var raycastTarget = ParseOptionalBool(p.Get("raycastTarget", string.Empty));
            var dryRun = p.GetBool("dryRun", true);

            if (!UiToolUtility.TryResolveGameObject(scenePath, hierarchyPath, out var target, out var error))
            {
                return ToolResult.Error(error, new { scenePath = scenePath ?? string.Empty, hierarchyPath = hierarchyPath ?? string.Empty });
            }

            var image = target.GetComponent<Image>();
            if (image == null)
            {
                return ToolResult.Error("Target has no Image component.", UiToolUtility.ElementSnapshot(target));
            }

            var hasColor = ParamUtility.TryReadColor(parameters, "color", out var color);
            var hasSprite = !string.IsNullOrWhiteSpace(spritePath) || !string.IsNullOrWhiteSpace(spriteGuid);
            var hasMaterial = !string.IsNullOrWhiteSpace(materialPath);
            var hasImageType = Enum.TryParse<Image.Type>(imageTypeText, true, out var imageType);
            if (!hasColor && !hasSprite && !hasMaterial && !hasImageType && !raycastTarget.HasValue)
            {
                return ToolResult.Error("At least one Image field is required.");
            }

            Sprite sprite = null;
            string resolvedSpritePath = string.Empty;
            if (hasSprite && !UiToolUtility.TryResolveSprite(spritePath, spriteGuid, out sprite, out resolvedSpritePath))
            {
                return ToolResult.Error("Sprite could not be resolved.", new { spritePath = spritePath ?? string.Empty, spriteGuid = spriteGuid ?? string.Empty });
            }

            Material material = null;
            if (hasMaterial)
            {
                material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
                if (material == null)
                {
                    return ToolResult.Error("Material could not be resolved.", new { materialPath });
                }
            }

            var before = UiToolUtility.ElementSnapshot(target);
            if (!dryRun)
            {
                Undo.RecordObject(image, "Set UI Image");
                if (hasSprite) image.sprite = sprite;
                if (hasColor) image.color = color;
                if (hasMaterial) image.material = material;
                if (hasImageType) image.type = imageType;
                if (raycastTarget.HasValue) image.raycastTarget = raycastTarget.Value;
                EditorUtility.SetDirty(image);
                UiToolUtility.MarkUiObjectDirty(target, "Set UI Image");
            }

            return ToolResult.Success(
                dryRun ? "UI image change previewed." : "UI image updated.",
                new
                {
                    dryRun,
                    resolvedSpritePath,
                    materialPath = hasMaterial ? materialPath : string.Empty,
                    before,
                    after = UiToolUtility.ElementSnapshot(target)
                });
        }
        catch (Exception ex)
        {
            return ToolResult.FromException(ex, "Failed to set UI image.");
        }
    }

    private static bool? ParseOptionalBool(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (bool.TryParse(value.Trim(), out var parsed))
        {
            return parsed;
        }

        return null;
    }
}
}
