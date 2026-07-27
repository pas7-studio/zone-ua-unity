using System.Collections.Generic;
using UnityEngine;

public sealed class GrassSorting : MonoBehaviour
{
    [SerializeField] private List<SpriteRenderer> grassSprites =
        new List<SpriteRenderer>();

    [SerializeField] private float offset = 0.1f;

    private Grass[] grassBehaviours;
    private Collider2D playerCollider;
    private SpriteRenderer playerRenderer;

    private void Awake()
    {
        BuildCache();
    }

    private void OnEnable()
    {
        if (grassBehaviours == null || grassBehaviours.Length != grassSprites.Count)
        {
            BuildCache();
        }
    }

    private void FixedUpdate()
    {
        if (playerCollider == null || playerRenderer == null)
        {
            return;
        }

        float playerPositionY = playerCollider.bounds.center.y;
        float playerHeight = playerCollider.bounds.size.y;
        int behindPlayerOrder = playerRenderer.sortingOrder - 1;

        for (int i = 0; i < grassSprites.Count; i++)
        {
            SpriteRenderer grassSprite = grassSprites[i];
            Grass grass = grassBehaviours[i];

            if (grassSprite == null || grass == null)
            {
                continue;
            }

            float grassBottom = grassSprite.bounds.min.y;
            float distanceToBottom = playerPositionY - (grassBottom + offset);

            if (distanceToBottom > playerHeight)
            {
                grass.SetDefaultOrder();
            }
            else
            {
                grass.SetSortOrder(behindPlayerOrder);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        CachePlayer(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (playerCollider == null)
        {
            CachePlayer(other);
        }
    }

    private void CachePlayer(Collider2D other)
    {
        if (other.GetComponentInParent<PlayerInputRouter>() == null)
        {
            return;
        }

        playerCollider = other;
        playerRenderer = other.GetComponentInChildren<SpriteRenderer>();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other != playerCollider)
        {
            return;
        }

        RestoreDefaultOrders();
        playerCollider = null;
        playerRenderer = null;
    }

    private void BuildCache()
    {
        grassBehaviours = new Grass[grassSprites.Count];

        for (int i = 0; i < grassSprites.Count; i++)
        {
            if (grassSprites[i] != null)
            {
                grassBehaviours[i] = grassSprites[i].GetComponent<Grass>();
            }
        }
    }

    private void RestoreDefaultOrders()
    {
        if (grassBehaviours == null)
        {
            return;
        }

        for (int i = 0; i < grassBehaviours.Length; i++)
        {
            grassBehaviours[i]?.SetDefaultOrder();
        }
    }
}
