using UnityEngine;

public sealed class Weapon : MonoBehaviour
{
    [SerializeField] private string weaponName;
    [TextArea]
    [SerializeField] private string weaponDescription;
    [SerializeField, Min(1)] private int weaponAmmoMax = 1;

    public string Name => weaponName;
    public string Description => weaponDescription;
    public int MaximumAmmo => weaponAmmoMax;

    private void OnValidate()
    {
        weaponAmmoMax = Mathf.Max(1, weaponAmmoMax);
    }
}
