using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ZoneUA.EditorValidation
{
    public enum CompositionMigrationStatus { Missing, Added, Error }

    [Serializable]
    public sealed class CompositionMigrationIssue
    {
        public CompositionMigrationStatus status;
        public string assetPath;
        public string hierarchyPath;
        public string message;
    }

    [Serializable]
    public sealed class CompositionMigrationReport
    {
        public readonly List<CompositionMigrationIssue> Issues = new List<CompositionMigrationIssue>();
        public int MissingCount => Issues.Count(issue => issue.status == CompositionMigrationStatus.Missing);
        public int AddedCount => Issues.Count(issue => issue.status == CompositionMigrationStatus.Added);
        public int ErrorCount => Issues.Count(issue => issue.status == CompositionMigrationStatus.Error);
        public bool IsClean => MissingCount == 0 && ErrorCount == 0;

        public void Add(CompositionMigrationStatus status, string assetPath, string hierarchyPath, string message)
        {
            Issues.Add(new CompositionMigrationIssue
            {
                status = status,
                assetPath = assetPath ?? string.Empty,
                hierarchyPath = hierarchyPath ?? string.Empty,
                message = message ?? string.Empty
            });
        }
    }

    public sealed class CompositionMigrationWindow : EditorWindow
    {
        private CompositionMigrationReport report;
        private Vector2 scroll;

        public static void ShowReport(CompositionMigrationReport report)
        {
            CompositionMigrationWindow window = GetWindow<CompositionMigrationWindow>("Composition Migration");
            window.report = report;
            window.Show();
        }

        private void OnGUI()
        {
            if (report == null)
            {
                EditorGUILayout.HelpBox("Run a Zone UA migration or audit command.", MessageType.Info);
                return;
            }

            EditorGUILayout.LabelField("Production composition", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Missing: {report.MissingCount}   Added: {report.AddedCount}   Errors: {report.ErrorCount}");
            scroll = EditorGUILayout.BeginScrollView(scroll);
            foreach (CompositionMigrationIssue issue in report.Issues)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField(issue.status.ToString(), EditorStyles.boldLabel);
                EditorGUILayout.LabelField(issue.message, EditorStyles.wordWrappedLabel);
                EditorGUILayout.LabelField(issue.assetPath, EditorStyles.miniLabel);
                if (!string.IsNullOrEmpty(issue.hierarchyPath))
                    EditorGUILayout.LabelField(issue.hierarchyPath, EditorStyles.miniLabel);
                if (GUILayout.Button("Select asset"))
                {
                    UnityEngine.Object asset = AssetDatabase.LoadMainAssetAtPath(issue.assetPath);
                    Selection.activeObject = asset;
                    EditorGUIUtility.PingObject(asset);
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndScrollView();
        }
    }
}
