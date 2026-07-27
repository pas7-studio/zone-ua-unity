using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public sealed class Grass : MonoBehaviour
{
    private int defaultSortingOrder;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        CacheRenderer();
        defaultSortingOrder = spriteRenderer.sortingOrder;
    }

    public void SetSortingFromSource()
    {
        CacheRenderer();
        defaultSortingOrder = spriteRenderer.sortingOrder;
    }

    public void SetSortOrder(int sortingOrder)
    {
        CacheRenderer();
        spriteRenderer.sortingOrder = sortingOrder;
    }

    public void SetDefaultOrder()
    {
        CacheRenderer();
        spriteRenderer.sortingOrder = defaultSortingOrder;
    }

    private void CacheRenderer()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }

    // Backwards-compatible method names.
    public void setSortingFromSource() => SetSortingFromSource();
    public void setSortOrder(int sortingOrder) => SetSortOrder(sortingOrder);
    public void setDefaultOrder() => SetDefaultOrder();
}
