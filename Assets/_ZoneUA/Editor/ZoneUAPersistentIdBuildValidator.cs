using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace ZoneUA.EditorValidation
{
    public sealed class ZoneUAPersistentIdBuildValidator : IPreprocessBuildWithReport
    {
        public int callbackOrder => -760;

        public void OnPreprocessBuild(BuildReport report)
        {
            var issues = ZoneUAPersistentIdTools.CollectIssues();
            if (issues.Count == 0) return;
            throw new BuildFailedException($"Persistent stable ID validation failed with {issues.Count} issue(s). Run Zone UA -> Persistence -> Validate Stable IDs.");
        }
    }
}