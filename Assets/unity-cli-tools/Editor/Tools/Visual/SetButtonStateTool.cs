using System;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine.UI;
using UnityCliConnector;
using UnityCliTools.Core;
using UnityCliTools.Infrastructure.Responses;
using UnityCliTools.Infrastructure.Ui;

namespace UnityCliTools.Visual
{

[UnityCliTool(
    Name = UnityCliToolNames.SetButtonState,
    Description = "Inspect or set Button interactable state and target graphic raycast behavior.",
    Group = UnityCliToolGroups.Visual)]
public static class SetButtonStateTool
{
    public sealed class Parameters
    {
        [ToolParameter("Loaded scene path containing the target object.")]
        public string ScenePath { get; set; }

        [ToolParameter("Target Button GameObject hierarchy path.", Required = true)]
        public string HierarchyPath { get; set; }

        [ToolParameter("Button interactable value.")]
        public string Interactable { get; set; }

        [ToolParameter("Target Graphic raycastTarget value.")]
        public string TargetGraphicRaycastTarget { get; set; }

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
            var interactable = ParseOptionalBool(p.Get("interactable", string.Empty));
            var targetGraphicRaycastTarget = ParseOptionalBool(p.Get("targetGraphicRaycastTarget", string.Empty));
            var dryRun = p.GetBool("dryRun", true);
            if (!UiToolUtility.TryResolveGameObject(scenePath, hierarchyPath, out var target, out var error))
            {
                return ToolResult.Error(error, new { scenePath = scenePath ?? string.Empty, hierarchyPath = hierarchyPath ?? string.Empty });
            }

            var button = target.GetComponent<Button>();
            if (button == null)
            {
                return ToolResult.Error("Target has no Button component.", UiToolUtility.ElementSnapshot(target));
            }

            var before = Snapshot(button);
            if (!dryRun && (interactable.HasValue || targetGraphicRaycastTarget.HasValue))
            {
                Undo.RecordObject(button, "Set Button State");
                if (interactable.HasValue)
                {
                    button.interactable = interactable.Value;
                }

                if (targetGraphicRaycastTarget.HasValue && button.targetGraphic != null)
                {
                    Undo.RecordObject(button.targetGraphic, "Set Button Target Graphic");
                    button.targetGraphic.raycastTarget = targetGraphicRaycastTarget.Value;
                    EditorUtility.SetDirty(button.targetGraphic);
                }

                EditorUtility.SetDirty(button);
                UiToolUtility.MarkUiObjectDirty(target, "Set Button State");
            }

            return ToolResult.Success(
                dryRun ? "Button state inspected or previewed." : "Button state updated.",
                new
                {
                    dryRun,
                    target = UiToolUtility.ElementSnapshot(target),
                    before,
                    after = Snapshot(button)
                });
        }
        catch (Exception ex)
        {
            return ToolResult.FromException(ex, "Failed to set button state.");
        }
    }

    private static object Snapshot(Button button)
    {
        return new
        {
            interactable = button.interactable,
            onClickCount = button.onClick.GetPersistentEventCount(),
            transition = button.transition.ToString(),
            targetGraphic = button.targetGraphic != null
                ? new
                {
                    name = button.targetGraphic.name,
                    type = button.targetGraphic.GetType().Name,
                    raycastTarget = button.targetGraphic.raycastTarget
                }
                : null
        };
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
