using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public sealed class Death : MonoBehaviour
{
    private static readonly int DeadByBulletHash = Animator.StringToHash("DeadByBullet");

    private Animator animator;
    private Rigidbody2D body;
    private CharacterCustomController characterController;
    private WeaponSwitcher weaponSwitcher;
    private NPCController npcController;

    public bool IsDead { get; private set; }

    private void Awake()
    {
        animator = GetComponent<Animator>();
        body = GetComponent<Rigidbody2D>();
        characterController = GetComponent<CharacterCustomController>();
        weaponSwitcher = GetComponent<WeaponSwitcher>();
        npcController = GetComponent<NPCController>();
    }

    public void Dead()
    {
        if (IsDead)
        {
            return;
        }

        IsDead = true;
        body.simulated = false;
        animator.applyRootMotion = false;
        animator.SetTrigger(DeadByBulletHash);

        if (characterController != null)
        {
            characterController.enabled = false;
        }

        if (weaponSwitcher != null)
        {
            weaponSwitcher.HideAllWeapons();
            weaponSwitcher.enabled = false;
        }

        if (npcController != null)
        {
            npcController.PrepareForDeath();
            npcController.enabled = false;
        }
        else
        {
            GlobalSystem.Instance?.AmmoUI?.ShowHideUI(false);
        }
    }
}
