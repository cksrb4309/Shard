using System;
using System.IO;
using System.Text;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityCliConnector;
using UnityCliTools.Core;
using UnityCliTools.Infrastructure.Responses;

namespace UnityCliTools.Automation
{

[UnityCliTool(
    Name = UnityCliToolNames.CreateMonoScript,
    Description = "Create a reusable MonoBehaviour script from a small set of safe templates.",
    Group = UnityCliToolGroups.Automation)]
public static class CreateMonoScriptTool
{
    public sealed class Parameters
    {
        [ToolParameter("Script path under Assets, e.g. Assets/Scripts/TutorialCanvasController.cs.", Required = true)]
        public string Path { get; set; }

        [ToolParameter("Class name. Defaults to file name.")]
        public string ClassName { get; set; }

        [ToolParameter("Template: empty_mono, ui_pager.", DefaultValue = "empty_mono")]
        public string Template { get; set; }

        [ToolParameter("Namespace name. Optional.")]
        public string Namespace { get; set; }

        [ToolParameter("Overwrite existing file.", DefaultValue = "false")]
        public bool Overwrite { get; set; }

        [ToolParameter("Preview only without writing the file.", DefaultValue = "true")]
        public bool DryRun { get; set; }
    }

    public static object HandleCommand(JObject parameters)
    {
        try
        {
            var p = new ToolParams(parameters);
            var path = UnityCliToolShared.TryConvertToAssetPath(p.Get("path", string.Empty));
            var className = p.Get("className", string.Empty)?.Trim();
            var template = (p.Get("template", "empty_mono") ?? "empty_mono").Trim().ToLowerInvariant();
            var namespaceName = p.Get("namespace", string.Empty)?.Trim();
            var overwrite = p.GetBool("overwrite", false);
            var dryRun = p.GetBool("dryRun", true);

            UnityCliToolShared.GuardRequired(path, nameof(path));
            if (!path.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase) || !path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
            {
                return ToolResult.Error("path must be a C# file under Assets.", new { path });
            }

            if (string.IsNullOrWhiteSpace(className))
            {
                className = System.IO.Path.GetFileNameWithoutExtension(path);
            }

            if (!IsValidIdentifier(className))
            {
                return ToolResult.Error("className must be a valid C# identifier.", new { className });
            }

            var absolutePath = System.IO.Path.GetFullPath(path);
            var exists = File.Exists(absolutePath);
            if (exists && !overwrite)
            {
                return ToolResult.Error("Script already exists and overwrite is false.", new { path });
            }

            var source = Generate(template, className, namespaceName);
            if (!dryRun)
            {
                var directory = System.IO.Path.GetDirectoryName(absolutePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(absolutePath, source, Encoding.UTF8);
                AssetDatabase.ImportAsset(path);
                AssetDatabase.Refresh();
            }

            return ToolResult.Success(
                dryRun ? "MonoBehaviour script creation previewed." : "MonoBehaviour script created.",
                new
                {
                    dryRun,
                    path,
                    className,
                    template,
                    namespaceName = namespaceName ?? string.Empty,
                    existsBefore = exists,
                    source
                });
        }
        catch (Exception ex)
        {
            return ToolResult.FromException(ex, "Failed to create MonoBehaviour script.");
        }
    }

    private static string Generate(string template, string className, string namespaceName)
    {
        var body = template == "ui_pager" ? UiPagerBody(className) : EmptyMonoBody(className);
        if (string.IsNullOrWhiteSpace(namespaceName))
        {
            return body;
        }

        return "namespace " + namespaceName + "\n{\n" + Indent(body) + "}\n";
    }

    private static string EmptyMonoBody(string className)
    {
        return "using UnityEngine;\n\npublic sealed class " + className + " : MonoBehaviour\n{\n}\n";
    }

    private static string UiPagerBody(string className)
    {
        return @"using TMPro;
using UnityEngine;

public sealed class " + className + @" : MonoBehaviour
{
    [SerializeField] private GameObject[] pages;
    [SerializeField] private TMP_Text previousLabel;
    [SerializeField] private TMP_Text nextLabel;
    [SerializeField] private string previousText = ""<"";
    [SerializeField] private string nextText = "">"";
    [SerializeField] private string closeText = ""X"";

    private int pageIndex;

    private void OnEnable()
    {
        pageIndex = Mathf.Clamp(pageIndex, 0, Mathf.Max(0, PageCount - 1));
        Refresh();
    }

    public void Previous()
    {
        pageIndex = Mathf.Max(0, pageIndex - 1);
        Refresh();
    }

    public void NextOrClose()
    {
        if (PageCount == 0)
        {
            gameObject.SetActive(false);
            return;
        }

        if (pageIndex >= PageCount - 1)
        {
            gameObject.SetActive(false);
            return;
        }

        pageIndex++;
        Refresh();
    }

    public void Refresh()
    {
        if (pages != null)
        {
            for (var i = 0; i < pages.Length; i++)
            {
                if (pages[i] != null)
                {
                    pages[i].SetActive(i == pageIndex);
                }
            }
        }

        if (previousLabel != null)
        {
            previousLabel.text = previousText;
        }

        if (nextLabel != null)
        {
            nextLabel.text = PageCount > 0 && pageIndex >= PageCount - 1 ? closeText : nextText;
        }
    }

    private int PageCount
    {
        get { return pages == null ? 0 : pages.Length; }
    }
}
";
    }

    private static string Indent(string text)
    {
        return "    " + text.Replace("\n", "\n    ").TrimEnd() + "\n";
    }

    private static bool IsValidIdentifier(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || !(char.IsLetter(value[0]) || value[0] == '_'))
        {
            return false;
        }

        for (var i = 1; i < value.Length; i++)
        {
            if (!char.IsLetterOrDigit(value[i]) && value[i] != '_')
            {
                return false;
            }
        }

        return true;
    }
}
}
