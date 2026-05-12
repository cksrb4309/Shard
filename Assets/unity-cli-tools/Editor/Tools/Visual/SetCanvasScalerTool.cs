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
    Name = UnityCliToolNames.SetCanvasScaler,
    Description = "Set CanvasScaler settings for responsive UI layout.",
    Group = UnityCliToolGroups.Visual)]
public static class SetCanvasScalerTool
{
    public sealed class Parameters
    {
        [ToolParameter("Loaded scene path containing the Canvas.")]
        public string ScenePath { get; set; }

        [ToolParameter("Canvas hierarchy path.", Required = true)]
        public string HierarchyPath { get; set; }

        [ToolParameter("Scale mode: ConstantPixelSize, ScaleWithScreenSize, ConstantPhysicalSize.")]
        public string UiScaleMode { get; set; }

        [ToolParameter("Reference resolution as x,y.")]
        public string ReferenceResolution { get; set; }

        [ToolParameter("Screen match mode: MatchWidthOrHeight, Expand, Shrink.")]
        public string ScreenMatchMode { get; set; }

        [ToolParameter("Width/height match value from 0 to 1.")]
        public float MatchWidthOrHeight { get; set; }

        [ToolParameter("Scale factor for ConstantPixelSize.")]
        public float ScaleFactor { get; set; }

        [ToolParameter("Reference pixels per unit.")]
        public float ReferencePixelsPerUnit { get; set; }

        [ToolParameter("Add CanvasScaler if missing.", DefaultValue = "true")]
        public bool AddIfMissing { get; set; }

        [ToolParameter("Preview only without modifying the Canvas.", DefaultValue = "true")]
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

            if (target.GetComponent<Canvas>() == null)
            {
                return ToolResult.Error("Target has no Canvas component.", UiToolUtility.ElementSnapshot(target));
            }

            var scaler = target.GetComponent<CanvasScaler>();
            if (scaler == null && !addIfMissing)
            {
                return ToolResult.Error("CanvasScaler is missing and addIfMissing is false.", UiToolUtility.ElementSnapshot(target));
            }

            var before = scaler != null ? Snapshot(scaler) : null;
            if (!dryRun)
            {
                if (scaler == null)
                {
                    scaler = Undo.AddComponent<CanvasScaler>(target);
                }

                Undo.RecordObject(scaler, "Set CanvasScaler");
                Apply(parameters, p, scaler);
                EditorUtility.SetDirty(scaler);
                UiToolUtility.MarkUiObjectDirty(target, "Set CanvasScaler");
            }

            return ToolResult.Success(
                dryRun ? "CanvasScaler change previewed." : "CanvasScaler updated.",
                new
                {
                    dryRun,
                    target = UiToolUtility.ElementSnapshot(target),
                    before,
                    after = dryRun ? before : Snapshot(scaler)
                });
        }
        catch (Exception ex)
        {
            return ToolResult.FromException(ex, "Failed to set CanvasScaler.");
        }
    }

    private static void Apply(JObject parameters, ToolParams p, CanvasScaler scaler)
    {
        var scaleModeText = p.Get("uiScaleMode", string.Empty);
        if (Enum.TryParse<CanvasScaler.ScaleMode>(scaleModeText, true, out var scaleMode))
        {
            scaler.uiScaleMode = scaleMode;
        }

        if (ParamUtility.TryReadVector2(parameters, "referenceResolution", out var referenceResolution))
        {
            scaler.referenceResolution = referenceResolution;
        }

        var screenMatchText = p.Get("screenMatchMode", string.Empty);
        if (Enum.TryParse<CanvasScaler.ScreenMatchMode>(screenMatchText, true, out var screenMatchMode))
        {
            scaler.screenMatchMode = screenMatchMode;
        }

        var match = p.GetFloat("matchWidthOrHeight");
        if (match.HasValue)
        {
            scaler.matchWidthOrHeight = Mathf.Clamp01(match.Value);
        }

        var scaleFactor = p.GetFloat("scaleFactor");
        if (scaleFactor.HasValue)
        {
            scaler.scaleFactor = Mathf.Max(0.01f, scaleFactor.Value);
        }

        var referencePixelsPerUnit = p.GetFloat("referencePixelsPerUnit");
        if (referencePixelsPerUnit.HasValue)
        {
            scaler.referencePixelsPerUnit = Mathf.Max(0.01f, referencePixelsPerUnit.Value);
        }
    }

    private static object Snapshot(CanvasScaler scaler)
    {
        return new
        {
            uiScaleMode = scaler.uiScaleMode.ToString(),
            referenceResolution = UiToolUtility.Vector2Snapshot(scaler.referenceResolution),
            screenMatchMode = scaler.screenMatchMode.ToString(),
            matchWidthOrHeight = scaler.matchWidthOrHeight,
            scaleFactor = scaler.scaleFactor,
            referencePixelsPerUnit = scaler.referencePixelsPerUnit
        };
    }
}
}
