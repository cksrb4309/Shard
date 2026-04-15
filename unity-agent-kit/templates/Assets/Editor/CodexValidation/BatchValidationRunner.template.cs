using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class BatchValidationRunner
{
    private const string ValidationScenePath = "{{VALIDATION_SCENE_PATH}}";
    private const string ValidationPrefabPath = "{{VALIDATION_PREFAB_PATH}}";

    private static readonly string[] RequiredAssetPaths =
    {
        ValidationScenePath,
        ValidationPrefabPath,
        "{{OPTIONAL_REQUIRED_ASSET_PATH_1}}",
        "{{OPTIONAL_REQUIRED_ASSET_PATH_2}}",
    };

    public static void RunProjectSmokeValidation()
    {
        RunProjectSmokeValidationCore(requireEnabledBuildScene: false);
    }

    public static void RunProjectSmokeValidationStrict()
    {
        RunProjectSmokeValidationCore(requireEnabledBuildScene: true);
    }

    public static void RunProjectEditModeSmokeTests()
    {
        // Replace this with 1-3 deterministic project-specific smoke scenarios.
        // Good candidates:
        // - damage application
        // - progression request consumption
        // - unlock/stat application
        // - inventory/state transition without scene objects
        Debug.Log("[BatchValidation] No project-specific EditMode smoke tests configured yet.");
    }

    [MenuItem("Tools/Harness Validation/Run Smoke Validation", priority = 1800)]
    public static void RunSmokeValidationFromMenu()
    {
        RunMenuValidation("Smoke Validation", RunProjectSmokeValidation);
    }

    [MenuItem("Tools/Harness Validation/Run Strict Smoke Validation", priority = 1801)]
    public static void RunStrictSmokeValidationFromMenu()
    {
        RunMenuValidation("Strict Smoke Validation", RunProjectSmokeValidationStrict);
    }

    [MenuItem("Tools/Harness Validation/Run EditMode Smoke Tests", priority = 1802)]
    public static void RunEditModeSmokeTestsFromMenu()
    {
        RunMenuValidation("EditMode Smoke Tests", RunProjectEditModeSmokeTests);
    }

    [MenuItem("Tools/Harness Validation/Run Full Validation", priority = 1803)]
    public static void RunFullValidationFromMenu()
    {
        try
        {
            EditorUtility.DisplayProgressBar("Harness Validation", "Running strict smoke validation...", 0.33f);
            RunProjectSmokeValidationStrict();

            EditorUtility.DisplayProgressBar("Harness Validation", "Running EditMode smoke tests...", 0.9f);
            RunProjectEditModeSmokeTests();

            Debug.Log("[HarnessValidationMenu] Full validation passed.");
            EditorUtility.DisplayDialog("Harness Validation", "Full validation passed.", "OK");
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            EditorUtility.DisplayDialog("Harness Validation", $"Full validation failed.\n\n{exception.Message}", "OK");
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }

    [MenuItem("Tools/Harness Validation/Open Validation Log Folder", priority = 1820)]
    public static void OpenValidationLogFolder()
    {
        string logFolder = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Logs", "validation"));
        Directory.CreateDirectory(logFolder);
        EditorUtility.RevealInFinder(logFolder);
    }

    private static void RunProjectSmokeValidationCore(bool requireEnabledBuildScene)
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        foreach (string assetPath in RequiredAssetPaths)
        {
            if (string.IsNullOrWhiteSpace(assetPath) || assetPath.Contains("{{")) {
                continue;
            }

            ValidateRequiredAsset(assetPath, errors);
        }

        if (!string.IsNullOrWhiteSpace(ValidationScenePath) && !ValidationScenePath.Contains("{{"))
        {
            ValidateSceneForMissingScripts(ValidationScenePath, errors);
            ValidateBuildSettings(ValidationScenePath, requireEnabledBuildScene, errors, warnings);
        }

        if (!string.IsNullOrWhiteSpace(ValidationPrefabPath) && !ValidationPrefabPath.Contains("{{"))
            ValidatePrefabForMissingScripts(ValidationPrefabPath, errors);

        ValidateTestCoverage(warnings);

        foreach (string warning in warnings)
            Debug.LogWarning($"[BatchValidation] {warning}");

        if (errors.Count > 0)
        {
            foreach (string error in errors)
                Debug.LogError($"[BatchValidation] {error}");

            throw new BuildFailedException($"Project smoke validation failed with {errors.Count} error(s).");
        }

        Debug.Log($"[BatchValidation] Smoke validation passed with {warnings.Count} warning(s).");
    }

    private static void ValidateRequiredAsset(string assetPath, List<string> errors)
    {
        if (AssetDatabase.LoadMainAssetAtPath(assetPath) == null)
            errors.Add($"Missing required asset: {assetPath}");
    }

    private static void ValidateSceneForMissingScripts(string scenePath, List<string> errors)
    {
        if (AssetDatabase.LoadMainAssetAtPath(scenePath) == null)
            return;

        Scene scene = default;
        bool opened = false;

        try
        {
            scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            opened = true;

            int missingScriptCount = 0;
            foreach (GameObject root in scene.GetRootGameObjects())
                missingScriptCount += CountMissingScripts(root);

            if (missingScriptCount > 0)
                errors.Add($"{scenePath} contains {missingScriptCount} missing MonoBehaviour reference(s).");
        }
        catch (Exception exception)
        {
            errors.Add($"{scenePath} could not be opened for validation: {exception.Message}");
        }
        finally
        {
            if (opened && scene.IsValid())
                EditorSceneManager.CloseScene(scene, true);
        }
    }

    private static void ValidatePrefabForMissingScripts(string prefabPath, List<string> errors)
    {
        if (AssetDatabase.LoadMainAssetAtPath(prefabPath) == null)
            return;

        GameObject prefabRoot = null;

        try
        {
            prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
            int missingScriptCount = CountMissingScripts(prefabRoot);

            if (missingScriptCount > 0)
                errors.Add($"{prefabPath} contains {missingScriptCount} missing MonoBehaviour reference(s).");
        }
        catch (Exception exception)
        {
            errors.Add($"{prefabPath} could not be opened for validation: {exception.Message}");
        }
        finally
        {
            if (prefabRoot != null)
                PrefabUtility.UnloadPrefabContents(prefabRoot);
        }
    }

    private static void ValidateBuildSettings(
        string scenePath,
        bool requireEnabledBuildScene,
        List<string> errors,
        List<string> warnings)
    {
        foreach (EditorBuildSettingsScene buildScene in EditorBuildSettings.scenes)
        {
            if (buildScene.path == scenePath && buildScene.enabled)
                return;
        }

        string message = $"Scene is not enabled in ProjectSettings/EditorBuildSettings.asset: {scenePath}";
        if (requireEnabledBuildScene)
            errors.Add(message);
        else
            warnings.Add(message);
    }

    private static void ValidateTestCoverage(List<string> warnings)
    {
        string[] scripts = AssetDatabase.FindAssets("t:Script", new[] { "Assets/99_Tests" });
        if (scripts.Length == 0)
            warnings.Add("Assets/99_Tests does not contain project test scripts yet.");
    }

    private static int CountMissingScripts(GameObject gameObject)
    {
        int count = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(gameObject);

        foreach (Transform child in gameObject.transform)
            count += CountMissingScripts(child.gameObject);

        return count;
    }

    private static void RunMenuValidation(string title, Action action)
    {
        try
        {
            EditorUtility.DisplayProgressBar("Harness Validation", $"{title} running...", 0.5f);
            action();

            Debug.Log($"[HarnessValidationMenu] {title} passed.");
            EditorUtility.DisplayDialog("Harness Validation", $"{title} passed.", "OK");
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            EditorUtility.DisplayDialog("Harness Validation", $"{title} failed.\n\n{exception.Message}", "OK");
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }
}
