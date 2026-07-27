using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public sealed class SpriteMaskController : MonoBehaviour
{
    [SerializeField] private SpriteRenderer playerSpriteRenderer;
    [SerializeField] private SpriteMask spriteMask;

    private readonly HashSet<SpriteRenderer> overlappingRenderers =
        new HashSet<SpriteRenderer>();

    private Collider2D maskCollider;

    private void Awake()
    {
        maskCollider = GetComponent<Collider2D>();
        maskCollider.isTrigger = true;
        SetMaskState(false);
    }

    private void LateUpdate()
    {
        if (overlappingRenderers.Count == 0 ||
            playerSpriteRenderer == null ||
            spriteMask == null)
        {
            SetMaskState(false);
            return;
        }

        bool shouldMask = false;
        overlappingRenderers.RemoveWhere(renderer => renderer == null);

        foreach (SpriteRenderer renderer in overlappingRenderers)
        {
            if (playerSpriteRenderer.sortingLayerID == renderer.sortingLayerID &&
                playerSpriteRenderer.sortingOrder <= renderer.sortingOrder &&
                playerSpriteRenderer.transform.position.y > renderer.transform.position.y)
            {
                shouldMask = true;
                break;
            }
        }

        SetMaskState(shouldMask);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.isTrigger)
        {
            return;
        }

        SpriteRenderer renderer = collision.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            overlappingRenderers.Add(renderer);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.isTrigger)
        {
            return;
        }

        SpriteRenderer renderer = collision.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            overlappingRenderers.Remove(renderer);
        }

        if (overlappingRenderers.Count == 0)
        {
            SetMaskState(false);
        }
    }

    private void SetMaskState(bool state)
    {
        if (spriteMask == null || playerSpriteRenderer == null)
        {
            return;
        }

        spriteMask.enabled = state;
        playerSpriteRenderer.maskInteraction = state
            ? SpriteMaskInteraction.VisibleInsideMask
            : SpriteMaskInteraction.None;
    }
}
