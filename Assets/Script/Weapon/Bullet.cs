using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField]
    private int minDamage = 1;

    [SerializeField]
    private int maxDamage = 1;

    [SerializeField]
    private float criticalScale = 1.5f;

    [SerializeField]
    private int criticalChanse = 20;

    [SerializeField] // Should be global param
    private int criticalMaximum = 100;

    [SerializeField]
    private Transform damageDealPrefab;

    [SerializeField]
    private string[] whoRecieveDamage = {"Player", "Enemy"};


    void OnTriggerEnter2D(Collider2D other)
    {
        if (whoRecieveDamage.Any(other.tag.Contains))
        {
            // if the bullet collides with an object that has a health component
            Health health = other.GetComponent<Health>();
            if (health != null)
            {
                float damage = Random.Range(minDamage, maxDamage);
                bool isCritical = criticalChanse > Random.Range(0, criticalMaximum);

                damage = isCritical ? damage * criticalScale : damage;

                // inflict damage on the object
                health.receiveDamage((int)damage);

                DamageDealPopup.Crate(damageDealPrefab, other.transform.position, (int)damage, isCritical);
            }

            // destroy the bullet on impact
            Destroy(gameObject);
        }
    }
}