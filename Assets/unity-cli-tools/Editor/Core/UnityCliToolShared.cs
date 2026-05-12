using System;
using System.IO;
using UnityEngine;

namespace UnityCliTools.Core
{

public static class UnityCliToolShared
{
    public static string NormalizeAssetPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return string.Empty;
        }

        return path.Replace('\\', '/').Trim();
    }

    public static void GuardRequired(string value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value is required.", paramName);
        }
    }

    public static string TryConvertToAssetPath(string path)
    {
        var normalized = NormalizeAssetPath(path);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return string.Empty;
        }

        if (normalized.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
        {
            return "Assets/" + normalized.Substring("Assets/".Length);
        }

        var projectPath = GetProjectPath();
        if (string.IsNullOrWhiteSpace(projectPath))
        {
            return normalized;
        }

        var normalizedProject = NormalizeAssetPath(projectPath).TrimEnd('/');
        if (normalized.StartsWith(normalizedProject + "/", StringComparison.OrdinalIgnoreCase))
        {
            var relative = normalized.Substring(normalizedProject.Length + 1);
            return relative;
        }

        return normalized;
    }

    public static string GetProjectPath()
    {
        var dataPath = NormalizeAssetPath(Application.dataPath);
        if (dataPath.EndsWith("/Assets", StringComparison.OrdinalIgnoreCase))
        {
            return dataPath.Substring(0, dataPath.Length - "/Assets".Length);
        }

        try
        {
            var directory = Directory.GetParent(dataPath);
            return directory?.FullName?.Replace('\\', '/');
        }
        catch
        {
            return dataPath;
        }
    }
}
}
