using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grass : MonoBehaviour
{
    private int defaultSortingOrder = 0;
    private SpriteRenderer spriteRenderer;

    public void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void setSortingFromSource()
    {
        defaultSortingOrder = GetComponent<SpriteRenderer>().sortingOrder;
    }

    public void setSortOrder(int sortSet)
    {
        spriteRenderer.sortingOrder = sortSet;
    }

    public void setDefaultOrder()
    {
        spriteRenderer.sortingOrder = defaultSortingOrder;
    }
}
