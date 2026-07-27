using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZoneUA.SceneManagement
{
    [CreateAssetMenu(menuName = "Zone UA/Scenes/Scene Catalog", fileName = "SceneCatalog")]
    public sealed class SceneCatalog : ScriptableObject
    {
        [SerializeField] private string bootstrapScene = "Bootstrap";
        [SerializeField] private string initialProductionScene = "Production";
        [SerializeField] private string loadingScene = "";
        [SerializeField] private List<string> developmentScenes = new();
        [SerializeField] private List<string> testScenes = new();

        public string BootstrapScene => bootstrapScene;
        public string InitialProductionScene => initialProductionScene;
        public string LoadingScene => loadingScene;
        public IReadOnlyList<string> DevelopmentScenes => developmentScenes;
        public IReadOnlyList<string> TestScenes => testScenes;

        public IEnumerable<string> EnumerateConfiguredScenes()
        {
            if (!string.IsNullOrWhiteSpace(bootstrapScene)) yield return bootstrapScene.Trim();
            if (!string.IsNullOrWhiteSpace(initialProductionScene)) yield return initialProductionScene.Trim();
            if (!string.IsNullOrWhiteSpace(loadingScene)) yield return loadingScene.Trim();
            foreach (string scene in developmentScenes)
                if (!string.IsNullOrWhiteSpace(scene)) yield return scene.Trim();
            foreach (string scene in testScenes)
                if (!string.IsNullOrWhiteSpace(scene)) yield return scene.Trim();
        }

        private void OnValidate()
        {
            bootstrapScene = Normalise(bootstrapScene);
            initialProductionScene = Normalise(initialProductionScene);
            loadingScene = Normalise(loadingScene);
            RemoveDuplicates(developmentScenes);
            RemoveDuplicates(testScenes);
        }

        private static string Normalise(string value) => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();

        private static void RemoveDuplicates(List<string> values)
        {
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int i = values.Count - 1; i >= 0; i--)
            {
                values[i] = Normalise(values[i]);
                if (string.IsNullOrEmpty(values[i]) || !seen.Add(values[i])) values.RemoveAt(i);
            }
        }
    }
}
