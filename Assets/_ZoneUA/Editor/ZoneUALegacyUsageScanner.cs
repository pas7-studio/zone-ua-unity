using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace ZoneUA.EditorValidation
{
    [Serializable]
    public sealed class LegacyUsageFinding
    {
        public string code;
        public LegacyUsageSeverity severity;
        public string assetPath;
        public int line;
        public string message;
        public string source;
    }

    [Serializable]
    public sealed class LegacyUsageReport
    {
        public string generatedAtUtc;
        public List<LegacyUsageFinding> findings = new List<LegacyUsageFinding>();

        public int ErrorCount => findings.Count(item => item.severity == LegacyUsageSeverity.Error);
        public int WarningCount => findings.Count(item => item.severity == LegacyUsageSeverity.Warning);
        public bool IsValid => ErrorCount == 0;
    }

    public static class ZoneUALegacyUsageScanner
    {
        private static readonly string[] RuntimeRoots =
        {
            "Assets/Script",
            "Assets/_ZoneUA/Runtime"
        };

        [MenuItem("Zone UA/Validation/Scan Legacy Integration", priority = 30)]
        public static void ScanMenu()
        {
            LegacyUsageReport report = ScanProject();
            WriteReport(report, "Logs/ZoneUALegacyUsage.json");
            LogReport(report);
            EditorUtility.DisplayDialog(
                "Legacy integration scan",
                $"Errors: {report.ErrorCount}\nWarnings: {report.WarningCount}\nTotal: {report.findings.Count}\n\nReport: Logs/ZoneUALegacyUsage.json",
                "OK");
        }

        public static LegacyUsageReport ScanProject()
        {
            var report = new LegacyUsageReport { generatedAtUtc = DateTime.UtcNow.ToString("O") };
            foreach (string file in EnumerateRuntimeSourceFiles())
            {
                ScanFile(file, File.ReadAllLines(file), report);
            }
            return report;
        }

        public static void ScanFile(string assetPath, IReadOnlyList<string> lines, LegacyUsageReport report)
        {
            if (report == null || lines == null || string.IsNullOrWhiteSpace(assetPath)) return;

            string normalisedPath = assetPath.Replace('\\', '/');
            foreach (LegacyUsageRule rule in LegacyUsageCatalog.Rules)
            {
                if (rule.IsAllowedPath(normalisedPath)) continue;

                for (int lineIndex = 0; lineIndex < lines.Count; lineIndex++)
                {
                    string source = lines[lineIndex] ?? string.Empty;
                    string trimmed = source.TrimStart();
                    if (trimmed.StartsWith("//", StringComparison.Ordinal)) continue;

                    for (int tokenIndex = 0; tokenIndex < rule.Tokens.Count; tokenIndex++)
                    {
                        string token = rule.Tokens[tokenIndex];
                        if (source.IndexOf(token, StringComparison.Ordinal) < 0) continue;

                        report.findings.Add(new LegacyUsageFinding
                        {
                            code = rule.Code,
                            severity = rule.Severity,
                            assetPath = normalisedPath,
                            line = lineIndex + 1,
                            message = rule.Description,
                            source = source.Trim()
                        });
                        break;
                    }
                }
            }
        }

        public static void WriteReport(LegacyUsageReport report, string outputPath)
        {
            string directory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
            File.WriteAllText(outputPath, JsonUtility.ToJson(report, true));
        }

        private static IEnumerable<string> EnumerateRuntimeSourceFiles()
        {
            foreach (string root in RuntimeRoots)
            {
                if (!Directory.Exists(root)) continue;
                foreach (string path in Directory.GetFiles(root, "*.cs", SearchOption.AllDirectories))
                {
                    string normalised = path.Replace('\\', '/');
                    if (normalised.Contains("/Editor/") || normalised.Contains("/Tests/")) continue;
                    yield return normalised;
                }
            }
        }

        private static void LogReport(LegacyUsageReport report)
        {
            foreach (LegacyUsageFinding finding in report.findings)
            {
                string message = $"[{finding.code}] {finding.assetPath}:{finding.line} — {finding.message}\n{finding.source}";
                switch (finding.severity)
                {
                    case LegacyUsageSeverity.Error:
                        Debug.LogError(message);
                        break;
                    case LegacyUsageSeverity.Warning:
                        Debug.LogWarning(message);
                        break;
                    default:
                        Debug.Log(message);
                        break;
                }
            }

            Debug.Log($"Legacy integration scan complete. Errors: {report.ErrorCount}; warnings: {report.WarningCount}; total: {report.findings.Count}.");
        }
    }

    public sealed class ZoneUALegacyUsageBuildValidator : IPreprocessBuildWithReport
    {
        public int callbackOrder => -700;

        public void OnPreprocessBuild(BuildReport buildReport)
        {
            LegacyUsageReport report = ZoneUALegacyUsageScanner.ScanProject();
            ZoneUALegacyUsageScanner.WriteReport(report, "Logs/ZoneUALegacyUsage.json");
            if (!report.IsValid)
            {
                throw new BuildFailedException(
                    $"Zone UA legacy integration scan found {report.ErrorCount} blocking error(s). See Logs/ZoneUALegacyUsage.json.");
            }
        }
    }
}
