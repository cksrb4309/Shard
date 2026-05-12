using System;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityCliConnector;
using UnityCliTools.Core;
using UnityCliTools.Infrastructure.Json;
using UnityCliTools.Infrastructure.Responses;
using UnityCliTools.Infrastructure.Selection;

namespace UnityCliTools.Scene
{

[UnityCliTool(
    Name = UnityCliToolNames.SetTransform,
    Description = "Set world or local transform values for a scene object.",
    Group = UnityCliToolGroups.Scene)]
public static class SetTransformTool
{
    public sealed class Parameters
    {
        [ToolParameter("Loaded scene path containing the target object.")]
        public string ScenePath { get; set; }

        [ToolParameter("Target GameObject hierarchy path.", Required = true)]
        public string HierarchyPath { get; set; }

        [ToolParameter("Space to edit: world or local.", DefaultValue = "world")]
        public string Space { get; set; }

        [ToolParameter("Position as x,y,z.")]
        public string Position { get; set; }

        [ToolParameter("Rotation as euler x,y,z.")]
        public string Rotation { get; set; }

        [ToolParameter("Scale as x,y,z.")]
        public string Scale { get; set; }

        [ToolParameter("Preview only without modifying the object.", DefaultValue = "true")]
        public bool DryRun { get; set; }
    }

    public static object HandleCommand(JObject parameters)
    {
        try
        {
            var p = new ToolParams(parameters);
            var scenePath = UnityCliToolShared.TryConvertToAssetPath(p.Get("scenePath", string.Empty));
            var hierarchyPath = p.Get("hierarchyPath", string.Empty)?.Trim();
            var space = p.Get("space", "world")?.Trim();
            var dryRun = p.GetBool("dryRun", true);

            UnityCliToolShared.GuardRequired(hierarchyPath, nameof(hierarchyPath));

            if (!TryResolveTarget(scenePath, hierarchyPath, out var target, out var error))
            {
                return ToolResult.Error(error, new
                {
                    scenePath = scenePath ?? string.Empty,
                    hierarchyPath
                });
            }

            var hasPosition = ParamUtility.TryReadVector3(parameters, "position", out var position);
            var hasRotation = ParamUtility.TryReadVector3(parameters, "rotation", out var rotation);
            var hasScale = ParamUtility.TryReadVector3(parameters, "scale", out var scale);

            if (!hasPosition && !hasRotation && !hasScale)
            {
                return ToolResult.Error("At least one of position, rotation, or scale is required.");
            }

            var isLocal = string.Equals(space, "local", StringComparison.OrdinalIgnoreCase);
            var before = Snapshot(target.transform);

            if (!dryRun)
            {
                Undo.RecordObject(target.transform, "Set Transform");

                if (hasPosition)
                {
                    if (isLocal)
                    {
                        target.transform.localPosition = position;
                    }
                    else
                    {
                        target.transform.position = position;
                    }
                }

                if (hasRotation)
                {
                    var quaternion = Quaternion.Euler(rotation);
                    if (isLocal)
                    {
                        target.transform.localRotation = quaternion;
                    }
                    else
                    {
                        target.transform.rotation = quaternion;
                    }
                }

                if (hasScale)
                {
                    target.transform.localScale = scale;
                }

                EditorUtility.SetDirty(target.transform);
                EditorSceneManager.MarkSceneDirty(target.scene);
            }

            return ToolResult.Success(
                dryRun ? "Transform change previewed." : "Transform updated.",
                new
                {
                    dryRun,
                    target = new
                    {
                        scenePath = target.scene.path,
                        hierarchyPath = HierarchyUtility.GetHierarchyPath(target.transform),
                        instanceId = target.GetInstanceID()
                    },
                    space = isLocal ? "local" : "world",
                    before,
                    after = Snapshot(target.transform)
                });
        }
        catch (Exception ex)
        {
            return ToolResult.FromException(ex, "Failed to set transform.");
        }
    }

    private static object Snapshot(Transform transform)
    {
        return new
        {
            position = new { x = transform.position.x, y = transform.position.y, z = transform.position.z },
            rotation = new { x = transform.rotation.x, y = transform.rotation.y, z = transform.rotation.z, w = transform.rotation.w },
            localPosition = new { x = transform.localPosition.x, y = transform.localPosition.y, z = transform.localPosition.z },
            localRotation = new { x = transform.localRotation.x, y = transform.localRotation.y, z = transform.localRotation.z, w = transform.localRotation.w },
            scale = new { x = transform.localScale.x, y = transform.localScale.y, z = transform.localScale.z }
        };
    }

    private static bool TryResolveTarget(string scenePath, string hierarchyPath, out GameObject target, out string error)
    {
        target = null;
        error = string.Empty;

        if (!string.IsNullOrWhiteSpace(scenePath))
        {
            foreach (var loadedScene in HierarchyUtility.EnumerateLoadedScenes())
            {
                if (string.Equals(loadedScene.path, scenePath, StringComparison.OrdinalIgnoreCase))
                {
                    target = HierarchyUtility.FindByHierarchyPath(hierarchyPath, loadedScene);
                    if (target != null)
                    {
                        return true;
                    }

                    error = "Target GameObject was not found in the requested scene.";
                    return false;
                }
            }

            error = "Requested scene is not loaded.";
            return false;
        }

        target = HierarchyUtility.FindByHierarchyPath(hierarchyPath, null);
        if (target == null)
        {
            error = "Target GameObject was not found in loaded scenes.";
            return false;
        }

        return true;
    }
}
}
