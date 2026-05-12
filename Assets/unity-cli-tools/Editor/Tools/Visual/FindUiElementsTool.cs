using System;
using System.Collections.Generic;
using TMPro;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityCliConnector;
using UnityCliTools.Core;
using UnityCliTools.Infrastructure.Json;
using UnityCliTools.Infrastructure.Responses;
using UnityCliTools.Infrastructure.Selection;
using UnityCliTools.Infrastructure.Ui;

namespace UnityCliTools.Visual
{

[UnityCliTool(
    Name = UnityCliToolNames.FindUiElements,
    Description = "Find UI elements by name, text content, component type, interactable state, or raycast target.",
    Group = UnityCliToolGroups.Visual)]
public static class FindUiElementsTool
{
    public sealed class Parameters
    {
        [ToolParameter("Loaded scene path to search.")]
        public string ScenePath { get; set; }

        [ToolParameter("Optional Canvas or UI root hierarchy path.")]
        public string CanvasHierarchyPath { get; set; }

        [ToolParameter("Name substring filter.")]
        public string Name { get; set; }

        [ToolParameter("TMP text substring filter.")]
        public string Text { get; set; }

        [ToolParameter("Component type name filter, e.g. Button, Image, TMP_Text.")]
        public string ComponentType { get; set; }

        [ToolParameter("Selectable interactable filter: true or false.")]
        public string Interactable { get; set; }

        [ToolParameter("Graphic raycastTarget filter: true or false.")]
        public string RaycastTarget { get; set; }

        [ToolParameter("Include inactive objects.", DefaultValue = "true")]
        public bool IncludeInactive { get; set; }

        [ToolParameter("Max number of returned elements.", DefaultValue = "200")]
        public int Limit { get; set; }
    }

    public static object HandleCommand(JObject parameters)
    {
        try
        {
            var p = new ToolParams(parameters);
            var scenePath = UnityCliToolShared.TryConvertToAssetPath(p.Get("scenePath", string.Empty));
            var rootPath = p.Get("canvasHierarchyPath", string.Empty)?.Trim();
            var name = p.Get("name", string.Empty)?.Trim();
            var text = p.Get("text", string.Empty)?.Trim();
            var componentType = p.Get("componentType", string.Empty)?.Trim();
            var interactable = ParseOptionalBool(p.Get("interactable", string.Empty));
            var raycastTarget = ParseOptionalBool(p.Get("raycastTarget", string.Empty));
            var includeInactive = p.GetBool("includeInactive", true);
            var limit = ParamUtility.ClampOrDefault(p.GetInt("limit"), 200, 1, 5000);
            var roots = ResolveRoots(scenePath, rootPath, includeInactive);
            var rows = new List<object>();

            foreach (var root in roots)
            {
                foreach (var go in UiToolUtility.EnumerateChildren(root, includeInactive, 128))
                {
                    if (rows.Count >= limit)
                    {
                        break;
                    }

                    if (!Matches(go, name, text, componentType, interactable, raycastTarget))
                    {
                        continue;
                    }

                    rows.Add(UiToolUtility.ElementSnapshot(go, root.transform));
                }
            }

            return ToolResult.Success(
                "UI elements searched.",
                new
                {
                    query = new
                    {
                        scenePath = scenePath ?? string.Empty,
                        canvasHierarchyPath = rootPath ?? string.Empty,
                        name = name ?? string.Empty,
                        text = text ?? string.Empty,
                        componentType = componentType ?? string.Empty,
                        interactable,
                        raycastTarget,
                        includeInactive,
                        limit
                    },
                    returned = rows.Count,
                    results = rows
                });
        }
        catch (Exception ex)
        {
            return ToolResult.FromException(ex, "Failed to find UI elements.");
        }
    }

    private static List<GameObject> ResolveRoots(string scenePath, string rootPath, bool includeInactive)
    {
        var roots = new List<GameObject>();
        if (!string.IsNullOrWhiteSpace(rootPath)
            && UiToolUtility.TryResolveGameObject(scenePath, rootPath, out var root, out _))
        {
            roots.Add(root);
            return roots;
        }

        foreach (var go in UiToolUtility.EnumerateLoadedSceneObjects(scenePath, includeInactive))
        {
            if (go.GetComponent<Canvas>() != null)
            {
                roots.Add(go);
            }
        }

        return roots;
    }

    private static bool Matches(GameObject go, string name, string text, string componentType, bool? interactable, bool? raycastTarget)
    {
        if (!string.IsNullOrWhiteSpace(name)
            && go.name.IndexOf(name, StringComparison.OrdinalIgnoreCase) < 0)
        {
            return false;
        }

        var tmp = go.GetComponent<TMP_Text>();
        if (!string.IsNullOrWhiteSpace(text)
            && (tmp == null || (tmp.text ?? string.Empty).IndexOf(text, StringComparison.OrdinalIgnoreCase) < 0))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(componentType) && go.GetComponent(componentType) == null)
        {
            return false;
        }

        var selectable = go.GetComponent<Selectable>();
        if (interactable.HasValue && (selectable == null || selectable.interactable != interactable.Value))
        {
            return false;
        }

        var graphic = go.GetComponent<Graphic>();
        if (raycastTarget.HasValue && (graphic == null || graphic.raycastTarget != raycastTarget.Value))
        {
            return false;
        }

        return true;
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
