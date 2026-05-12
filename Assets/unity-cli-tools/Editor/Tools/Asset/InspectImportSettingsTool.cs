using System;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityCliConnector;
using UnityCliTools.Core;
using UnityCliTools.Infrastructure.Responses;

namespace UnityCliTools.Asset
{

[UnityCliTool(
    Name = UnityCliToolNames.InspectImportSettings,
    Description = "Inspect AssetImporter settings for textures, models, audio, and generic assets.",
    Group = UnityCliToolGroups.Asset)]
public static class InspectImportSettingsTool
{
    public sealed class Parameters
    {
        [ToolParameter("Asset path under Assets.")]
        public string Path { get; set; }

        [ToolParameter("Asset GUID.")]
        public string Guid { get; set; }
    }

    public static object HandleCommand(JObject parameters)
    {
        try
        {
            var p = new ToolParams(parameters);
            var path = ResolvePath(p.Get("path", string.Empty), p.Get("guid", string.Empty));
            if (string.IsNullOrWhiteSpace(path))
            {
                return ToolResult.Error("Asset path or GUID is required.");
            }

            var importer = AssetImporter.GetAtPath(path);
            if (importer == null)
            {
                return ToolResult.Error("Asset importer not found.", new { path });
            }

            return ToolResult.Success(
                "Import settings inspected.",
                new
                {
                    path,
                    guid = AssetDatabase.AssetPathToGUID(path),
                    importerType = importer.GetType().Name,
                    assetBundleName = importer.assetBundleName,
                    assetBundleVariant = importer.assetBundleVariant,
                    userData = importer.userData,
                    settings = ReadImporterSettings(importer)
                });
        }
        catch (Exception ex)
        {
            return ToolResult.FromException(ex, "Failed to inspect import settings.");
        }
    }

    private static string ResolvePath(string path, string guid)
    {
        if (!string.IsNullOrWhiteSpace(path))
        {
            return UnityCliToolShared.TryConvertToAssetPath(path);
        }

        return string.IsNullOrWhiteSpace(guid) ? string.Empty : AssetDatabase.GUIDToAssetPath(guid.Trim());
    }

    private static object ReadImporterSettings(AssetImporter importer)
    {
        var texture = importer as TextureImporter;
        if (texture != null)
        {
            return new
            {
                kind = "texture",
                textureType = texture.textureType.ToString(),
                textureShape = texture.textureShape.ToString(),
                sRGBTexture = texture.sRGBTexture,
                alphaSource = texture.alphaSource.ToString(),
                alphaIsTransparency = texture.alphaIsTransparency,
                mipmapEnabled = texture.mipmapEnabled,
                isReadable = texture.isReadable,
                maxTextureSize = texture.maxTextureSize,
                textureCompression = texture.textureCompression.ToString(),
                compressionQuality = texture.compressionQuality,
                wrapMode = texture.wrapMode.ToString(),
                filterMode = texture.filterMode.ToString()
            };
        }

        var model = importer as ModelImporter;
        if (model != null)
        {
            return new
            {
                kind = "model",
                importAnimation = model.importAnimation,
                importCameras = model.importCameras,
                importLights = model.importLights,
                importBlendShapes = model.importBlendShapes,
                importVisibility = model.importVisibility,
                globalScale = model.globalScale,
                meshCompression = model.meshCompression.ToString(),
                isReadable = model.isReadable,
                optimizeMeshPolygons = model.optimizeMeshPolygons,
                optimizeMeshVertices = model.optimizeMeshVertices,
                materialImportMode = model.materialImportMode.ToString()
            };
        }

        var audio = importer as AudioImporter;
        if (audio != null)
        {
            var settings = audio.defaultSampleSettings;
            return new
            {
                kind = "audio",
                forceToMono = audio.forceToMono,
                loadInBackground = audio.loadInBackground,
                ambisonic = audio.ambisonic,
                loadType = settings.loadType.ToString(),
                compressionFormat = settings.compressionFormat.ToString(),
                quality = settings.quality,
                sampleRateSetting = settings.sampleRateSetting.ToString(),
                sampleRateOverride = settings.sampleRateOverride
            };
        }

        return new
        {
            kind = "generic"
        };
    }
}
}
