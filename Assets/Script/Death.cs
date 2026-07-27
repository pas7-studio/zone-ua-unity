using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class Death : MonoBehaviour
{
    private Animator animator;
    private Rigidbody2D physic;

    //If player
    private CharacterCustomController characterController;
    private WeaponSwitcher weaponSwitcher;

    //If NPC
    private NPCController npcController;

    private GlobalSystem system;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        physic = GetComponent<Rigidbody2D>();
        characterController = GetComponent<CharacterCustomController>();
        weaponSwitcher = GetComponent<WeaponSwitcher>();
        npcController = GetComponent<NPCController>();
        system = GameObject.FindGameObjectWithTag("System").GetComponent<GlobalSystem>();
    }

    public void Dead()
    {
        physic.simulated = false;
        animator.applyRootMotion = false;
        animator.SetTrigger("DeadByBullet");

        if (characterController)
        {
            characterController.enabled = false;
        }
        if (weaponSwitcher)
        {
            weaponSwitcher.HideAllWeapons();
            weaponSwitcher.StopAllCoroutines();
            weaponSwitcher.enabled = false;
        }
        if (npcController)
        {
            npcController.SetTarget(null, NPCController.TargetType.BOTH);
            npcController.SetWeaponShow(false);
            npcController.StopAllWeaponCoroutines();
            npcController.enabled = false;
        }
        if (system && !npcController)
        {
            if (system.UIAmmoSystem)
            {
                system.UIAmmoSystem.ShowHideUI(false);
            }
        }
    }
}
