using UnityEngine;

namespace ZoneUA.Testing
{
    public sealed class ZoneUATestScenarioMarker : MonoBehaviour
    {
        [SerializeField] private string scenarioId;

        public string ScenarioId => scenarioId;

        public void Configure(string id)
        {
            scenarioId = id;
        }
    }
}
