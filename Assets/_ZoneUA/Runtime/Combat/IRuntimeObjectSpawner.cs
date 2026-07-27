using UnityEngine;

namespace ZoneUA.Combat
{
    public interface IRuntimeObjectSpawner
    {
        GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null);
        void Release(GameObject instance);
        void ReleaseAfter(GameObject instance, float delay);
    }
}
