using System.Linq;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace ZoneUA.EditorValidation
{
    public sealed class ZoneUASceneArchitectureBuildValidator : IPreprocessBuildWithReport
    {
        public int callbackOrder => -900;

        public void OnPreprocessBuild(BuildReport report)
        {
            var issues = ZoneUASceneArchitectureTools.CollectValidationIssues();
            if (issues.Count == 0) return;

            string message = "Scene architecture validation failed:\n" +
                             string.Join("\n", issues.Select(issue => "- " + issue));
            throw new BuildFailedException(message);
        }
    }
}
