using UnityEngine;
using ZoneUA.Combat;

public sealed class Weapon : MonoBehaviour
{
    [Header("Definition")]
    [SerializeField, Tooltip("Shared immutable weapon configuration. Legacy fields remain as fallback during migration.")]
    private WeaponDefinition definition;

    [Header("Legacy Metadata Fallback")]
    [SerializeField] private string weaponName;

    [TextArea]
    [SerializeField] private string weaponDescription;

    [SerializeField, Min(1)] private int weaponAmmoMax = 1;

    public WeaponDefinition Definition => definition;
    public string Name => definition != null ? definition.DisplayName : weaponName;
    public string Description => weaponDescription;
    public int MaximumAmmo => definition != null
        ? definition.MagazineCapacity
        : weaponAmmoMax;

    public ProjectileDefinition Projectile => definition != null
        ? definition.Projectile
        : null;

    private void OnValidate()
    {
        weaponAmmoMax = Mathf.Max(1, weaponAmmoMax);

        if (definition == null && string.IsNullOrWhiteSpace(weaponName))
        {
            weaponName = name;
        }
    }
}
