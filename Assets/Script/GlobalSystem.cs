using UnityEngine;

public class GlobalSystem : MonoBehaviour
{
    //Weapon
    public float weaponXOffset = 0.005f;
    public float weaponYOffset = 0.035f;
    
    //Health
    public int bloodAmount = 10;
    public float spawnRadius = 1f;
    public float bloodImpulsSpeed = 1f;
    public float bloodImpulseDuration = 1.0f;
    public GameObject[] bloodPrefabs;
    public ParticleSystem bloodParticleSystem;

    public Transform garbadge;

    public UIAmmoSystem UIAmmoSystem;

    public GameObject getRandomBlood()
    {
        return bloodPrefabs.Length > 0 ? bloodPrefabs[Random.Range(0, bloodPrefabs.Length)] : null;
    }
}
