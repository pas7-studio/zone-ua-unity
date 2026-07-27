using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrassSorting : MonoBehaviour
{
    public List<SpriteRenderer> grassSprites;
    public float offset = 0.1f;

    private float playerHeight;

    private void OnTriggerStay2D(Collider2D other)
    {
        // Only run the sorting logic if the collider that entered the grass collider belongs to the player
        if (other.CompareTag("Player"))
        {
            // Get the position of the player's collider
            float playerPosY = other.bounds.center.y;

            // Get the height of the player's collider
            playerHeight = other.bounds.size.y;

            // Loop through each grass sprite in the list
            foreach (SpriteRenderer grassSprite in grassSprites)
            {
                var grassLogic = grassSprite.GetComponent<Grass>();

                // Get the bottom position of the grass sprite
                float grassBottom = grassSprite.bounds.min.y;

                // Calculate the distance from the player's bottom to the bottom of the grass
                float distToBottom = playerPosY - (grassBottom + offset);

                if (distToBottom > playerHeight)
                {
                    grassLogic.setDefaultOrder();
                }
                else
                {
                    grassLogic.setSortOrder(other.GetComponentInChildren<SpriteRenderer>().sortingOrder - 1);
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {

        }
    }
}