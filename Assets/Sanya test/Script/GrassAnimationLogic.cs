using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrassAnimationLogic : MonoBehaviour
{
    public bool isCharacterOn = false; // Variable to indicate if a character is on the sprite
    private Animator anim;

    public void Start()
    {
        anim = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the collider belongs to a character
        CharacterCustomController character = other.GetComponent<CharacterCustomController>();
        if (character != null)
        {
            // Set the isCharacterOn variable to true
            isCharacterOn = true;
            anim.SetBool("isCharacterOn", isCharacterOn);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Check if the collider belongs to a character
        CharacterCustomController character = other.GetComponent<CharacterCustomController>();
        if (character != null)
        {
            // Set the isCharacterOn variable to false
            isCharacterOn = false;
            anim.SetBool("isCharacterOn", isCharacterOn);
        }
    }

}