using Assets.Script;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Death))]
public class Health : MonoBehaviour
{
    [SerializeField]
    private int currentHeals = 100;
    [SerializeField]
    private int defaultHeals = 100;
    [SerializeField]
    private bool isImunable = false;
    [SerializeField]
    private bool isAlive = true;

    private Animator animator;
    private Death death;
    private GlobalSystem globalSystem;

    private void Start()
    {
        animator = GetComponent<Animator>();
        death = GetComponent<Death>();
        globalSystem = GameObject.FindGameObjectWithTag("System").GetComponent<GlobalSystem>();
    }

    public void HealthLogic()
    {
        if(currentHeals == 0)
        {
            if (animator != null)
            {
                isAlive = false;
                death.Dead();
            }
        }
    }

    public void setHeals(int heals)
    {
        currentHeals = Mathf.Clamp(heals, 0, defaultHeals);
    }

    public void restoreSomeHeals(int amount)
    {
        currentHeals = Mathf.Clamp(currentHeals + amount, 0, defaultHeals);
    }

    public void restoreDefaultHeals()
    {
        currentHeals = defaultHeals;
    }

    public int getHeals()
    {
        return currentHeals;
    }

    public void receiveDamage(int damageAmount)
    {
        if (!isImunable)
        {
            currentHeals -= damageAmount;
            currentHeals = Mathf.Clamp(currentHeals, 0, defaultHeals);
            SpawnBlood();
            HealthLogic();
        }
    }

    public bool getIsAlive()
    {
        return isAlive;
    }

    private void SpawnBlood()
    {
        for (int i = 0; i < globalSystem.bloodAmount; i++)
        {
            Vector2 spawnPosition = (Vector2)transform.position + Random.insideUnitCircle * globalSystem.spawnRadius;

            Quaternion bloodDropRotation = Quaternion.Euler(0.0f, 0.0f, Random.Range(-360,360));
            Vector2 forceDirection = -transform.up * globalSystem.bloodImpulsSpeed;

            SpawnBloodEffect();
            GameObject bloodDrop = Instantiate(globalSystem.getRandomBlood(), spawnPosition, bloodDropRotation, globalSystem.garbadge);
            bloodDrop.GetComponent<Rigidbody2D>().AddForce(forceDirection, ForceMode2D.Impulse);
            StartCoroutine(Tools.AttenuateAmmoImpulse(bloodDrop.GetComponent<Rigidbody2D>(), globalSystem.bloodImpulseDuration));
        }
    }

    public void SpawnBloodEffect()
    {
        var particleSystemInstance = Instantiate(globalSystem.bloodParticleSystem, transform.position, globalSystem.bloodParticleSystem.transform.rotation);
        particleSystemInstance.Play();
        Destroy(particleSystemInstance.gameObject, globalSystem.bloodParticleSystem.main.startLifetime.constant);
    }
}