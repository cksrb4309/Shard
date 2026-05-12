using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityCliConnector;
using UnityCliTools.Core;
using UnityCliTools.Infrastructure.Json;
using UnityCliTools.Infrastructure.Responses;
using PackageManagerInfo = UnityEditor.PackageManager.PackageInfo;

namespace UnityCliTools.Project
{

[UnityCliTool(
    Name = UnityCliToolNames.ListPackages,
    Description = "List packages registered in the Unity Package Manager.",
    Group = UnityCliToolGroups.Project)]
public static class ListPackagesTool
{
    public sealed class Parameters
    {
        [ToolParameter("Package name or display name substring filter.")]
        public string Name { get; set; }

        [ToolParameter("Max number of returned packages.", DefaultValue = "200")]
        public int Limit { get; set; }
    }

    public static object HandleCommand(JObject parameters)
    {
        try
        {
            var p = new ToolParams(parameters);
            var name = p.Get("name", string.Empty)?.Trim();
            var limit = ParamUtility.ClampOrDefault(p.GetInt("limit"), 200, 1, 1000);
            var packages = PackageManagerInfo.GetAllRegisteredPackages();
            var rows = new List<object>(Math.Min(limit, packages.Length));
            foreach (var package in packages)
            {
                if (!Matches(package, name))
                {
                    continue;
                }

                if (rows.Count >= limit)
                {
                    break;
                }

                rows.Add(new
                {
                    name = package.name,
                    displayName = package.displayName,
                    version = package.version,
                    source = package.source.ToString(),
                    assetPath = package.assetPath,
                    resolvedPath = package.resolvedPath,
                    packageId = package.packageId
                });
            }

            return ToolResult.Success(
                "Packages listed.",
                new
                {
                    query = new { name = name ?? string.Empty, limit },
                    totalRegistered = packages.Length,
                    returned = rows.Count,
                    packages = rows
                });
        }
        catch (Exception ex)
        {
            return ToolResult.FromException(ex, "Failed to list packages.");
        }
    }

    private static bool Matches(PackageManagerInfo package, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return true;
        }

        return (package.name != null && package.name.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0)
            || (package.displayName != null && package.displayName.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0);
    }
}
}
