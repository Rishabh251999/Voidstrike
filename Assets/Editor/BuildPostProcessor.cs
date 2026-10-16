using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System.IO;
using UnityEditor;
using System;
using System.Text.RegularExpressions;

public class BuildPostProcessor : IPostprocessBuildWithReport
{
    public int callbackOrder { get { return 0; } }

    public void OnPostprocessBuild(BuildReport report)
    {
        if (report.summary.platform == BuildTarget.WebGL)
        {
            string outputPath = report.summary.outputPath;
            AddOrUpdateTimestampInIndexHtml(outputPath);
        }
    }

    private void AddOrUpdateTimestampInIndexHtml(string buildPath)
    {
        string indexPath = Path.Combine(buildPath, "index.html");

        if (File.Exists(indexPath))
        {
            string htmlContent = File.ReadAllText(indexPath);
            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            // Regex to find existing timestamp or placeholder
            string pattern = "(<!-- Version: )(.*?)( -->)";
            string replacement = $"<!-- Version: {timestamp} -->";

            // Replace existing timestamp or placeholder with new timestamp
            string modifiedHtmlContent = Regex.Replace(htmlContent, pattern, replacement);

            File.WriteAllText(indexPath, modifiedHtmlContent);
            // Debug.Log("Updated timestamp in index.html: " + timestamp);
        }
        else
        {
            // Debug.LogError("index.html not found in build path: " + indexPath);
        }
    }
}