using System;
using UnityEngine;
using ZoneUA.Combat;

[DisallowMultipleComponent]
public sealed class Death : MonoBehaviour
{
    private static readonly int DeadByBulletHash = Animator.StringToHash("DeadByBullet");

    [Header("Death Behaviour")]
    [SerializeField] private bool disableBodySimulation = true;
    [SerializeField] private bool disableColliders = true;
    [SerializeField, Tooltip("Additional behaviours disabled when death is entered.")]
    private MonoBehaviour[] behavioursToDisable;

    private readonly DeathState state = new DeathState();

    private Animator animator;
    private Rigidbody2D body;
    private CharacterCustomController characterController;
    private WeaponSwitcher weaponSwitcher;
    private NPCController npcController;
    private WeaponController[] weaponControllers;
    private Collider2D[] colliders;

    public event Action DeathEntered;

    public bool IsDead => state.IsDead;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        body = GetComponent<Rigidbody2D>();
        characterController = GetComponent<CharacterCustomController>();
        weaponSwitcher = GetComponent<WeaponSwitcher>();
        npcController = GetComponent<NPCController>();
        weaponControllers = GetComponentsInChildren<WeaponController>(true);
        colliders = GetComponentsInChildren<Collider2D>(true);
    }

    public void Dead()
    {
        if (!state.TryEnter())
        {
            return;
        }

        DisableGameplay();
        PlayDeathAnimation();
        DeathEntered?.Invoke();
    }

    public bool TryEnterDeath()
    {
        return state.TryEnter();
    }

    private void DisableGameplay()
    {
        if (body != null)
        {
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
            if (disableBodySimulation)
            {
                body.simulated = false;
            }
        }

        if (disableColliders && colliders != null)
        {
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] != null)
                {
                    colliders[i].enabled = false;
                }
            }
        }

        if (characterController != null)
        {
            characterController.enabled = false;
        }

        if (weaponControllers != null)
        {
            for (int i = 0; i < weaponControllers.Length; i++)
            {
                WeaponController controller = weaponControllers[i];
                if (controller == null)
                {
                    continue;
                }

                controller.StopFire();
                controller.enabled = false;
            }
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

        if (behavioursToDisable != null)
        {
            for (int i = 0; i < behavioursToDisable.Length; i++)
            {
                MonoBehaviour behaviour = behavioursToDisable[i];
                if (behaviour != null && behaviour != this)
                {
                    behaviour.enabled = false;
                }
            }
        }
    }

    private void PlayDeathAnimation()
    {
        if (animator == null)
        {
            return;
        }

        animator.applyRootMotion = false;
        animator.SetTrigger(DeadByBulletHash);
    }
}
