using System;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityCliConnector;
using UnityCliTools.Core;
using UnityCliTools.Infrastructure.Responses;
using UnityCliTools.Infrastructure.Ui;

namespace UnityCliTools.Visual
{

[UnityCliTool(
    Name = UnityCliToolNames.SetCanvasGroup,
    Description = "Add or configure CanvasGroup alpha, interactable, and blocksRaycasts.",
    Group = UnityCliToolGroups.Visual)]
public static class SetCanvasGroupTool
{
    public sealed class Parameters
    {
        [ToolParameter("Loaded scene path containing the target object.")]
        public string ScenePath { get; set; }

        [ToolParameter("Target GameObject hierarchy path.", Required = true)]
        public string HierarchyPath { get; set; }

        [ToolParameter("CanvasGroup alpha.")]
        public float Alpha { get; set; }

        [ToolParameter("CanvasGroup interactable value.")]
        public string Interactable { get; set; }

        [ToolParameter("CanvasGroup blocksRaycasts value.")]
        public string BlocksRaycasts { get; set; }

        [ToolParameter("CanvasGroup ignoreParentGroups value.")]
        public string IgnoreParentGroups { get; set; }

        [ToolParameter("Add CanvasGroup if missing.", DefaultValue = "true")]
        public bool AddIfMissing { get; set; }

        [ToolParameter("Preview only without modifying the target.", DefaultValue = "true")]
        public bool DryRun { get; set; }
    }

    public static object HandleCommand(JObject parameters)
    {
        try
        {
            var p = new ToolParams(parameters);
            var scenePath = p.Get("scenePath", string.Empty);
            var hierarchyPath = p.Get("hierarchyPath", string.Empty)?.Trim();
            var addIfMissing = p.GetBool("addIfMissing", true);
            var dryRun = p.GetBool("dryRun", true);
            if (!UiToolUtility.TryResolveGameObject(scenePath, hierarchyPath, out var target, out var error))
            {
                return ToolResult.Error(error, new { scenePath = scenePath ?? string.Empty, hierarchyPath = hierarchyPath ?? string.Empty });
            }

            var group = target.GetComponent<CanvasGroup>();
            if (group == null && !addIfMissing)
            {
                return ToolResult.Error("CanvasGroup is missing and addIfMissing is false.", UiToolUtility.ElementSnapshot(target));
            }

            var before = group != null ? Snapshot(group) : null;
            if (!dryRun)
            {
                if (group == null)
                {
                    group = Undo.AddComponent<CanvasGroup>(target);
                }

                Undo.RecordObject(group, "Set CanvasGroup");
                var alpha = p.GetFloat("alpha");
                if (alpha.HasValue) group.alpha = Mathf.Clamp01(alpha.Value);
                ApplyBool(p.Get("interactable", string.Empty), v => group.interactable = v);
                ApplyBool(p.Get("blocksRaycasts", string.Empty), v => group.blocksRaycasts = v);
                ApplyBool(p.Get("ignoreParentGroups", string.Empty), v => group.ignoreParentGroups = v);
                EditorUtility.SetDirty(group);
                UiToolUtility.MarkUiObjectDirty(target, "Set CanvasGroup");
            }

            return ToolResult.Success(
                dryRun ? "CanvasGroup change previewed." : "CanvasGroup updated.",
                new { dryRun, target = UiToolUtility.ElementSnapshot(target), before, after = dryRun ? before : Snapshot(group) });
        }
        catch (Exception ex)
        {
            return ToolResult.FromException(ex, "Failed to set CanvasGroup.");
        }
    }

    private static void ApplyBool(string raw, Action<bool> apply)
    {
        if (bool.TryParse(raw, out var value)) apply(value);
    }

    private static object Snapshot(CanvasGroup group)
    {
        return new
        {
            alpha = group.alpha,
            interactable = group.interactable,
            blocksRaycasts = group.blocksRaycasts,
            ignoreParentGroups = group.ignoreParentGroups
        };
    }
}
}
