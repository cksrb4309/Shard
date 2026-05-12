using System;
using TMPro;
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
    Name = UnityCliToolNames.SetUiText,
    Description = "Set TMP text on a UI object.",
    Group = UnityCliToolGroups.Visual)]
public static class SetUiTextTool
{
    public sealed class Parameters
    {
        [ToolParameter("Loaded scene path containing the target object.")]
        public string ScenePath { get; set; }

        [ToolParameter("Target UI GameObject hierarchy path.", Required = true)]
        public string HierarchyPath { get; set; }

        [ToolParameter("New TMP text value.", Required = true)]
        public string Text { get; set; }

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
            var text = p.Get("text", string.Empty);
            var dryRun = p.GetBool("dryRun", true);
            if (!UiToolUtility.TryResolveGameObject(scenePath, hierarchyPath, out var target, out var error))
            {
                return ToolResult.Error(error, new { scenePath = scenePath ?? string.Empty, hierarchyPath = hierarchyPath ?? string.Empty });
            }

            var tmp = target.GetComponent<TMP_Text>();
            if (tmp == null)
            {
                return ToolResult.Error("Target has no TMP_Text component.", UiToolUtility.ElementSnapshot(target));
            }

            var before = tmp.text;
            if (!dryRun)
            {
                Undo.RecordObject(tmp, "Set UI Text");
                tmp.text = text ?? string.Empty;
                EditorUtility.SetDirty(tmp);
                UiToolUtility.MarkUiObjectDirty(target, "Set UI Text");
            }

            return ToolResult.Success(
                dryRun ? "UI text change previewed." : "UI text updated.",
                new
                {
                    dryRun,
                    target = UiToolUtility.ElementSnapshot(target),
                    before,
                    after = dryRun ? text ?? string.Empty : tmp.text
                });
        }
        catch (Exception ex)
        {
            return ToolResult.FromException(ex, "Failed to set UI text.");
        }
    }
}
}
