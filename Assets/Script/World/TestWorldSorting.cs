using System.Collections.Generic;
using UnityEngine;

public sealed class TestWorldSorting : MonoBehaviour
{
    [SerializeField] private List<string> tagsToSort = new List<string>();

    private readonly Dictionary<string, int> tagSortingOrder =
        new Dictionary<string, int>();

    [ContextMenu("Sort All")]
    public void SortAll()
    {
        tagSortingOrder.Clear();

        for (int i = 0; i < tagsToSort.Count; i++)
        {
            tagSortingOrder[tagsToSort[i]] = i;
        }

        for (int tagIndex = 0; tagIndex < tagsToSort.Count; tagIndex++)
        {
            string tag = tagsToSort[tagIndex];
            GameObject[] objects = GameObject.FindGameObjectsWithTag(tag);

            System.Array.Sort(
                objects,
                (left, right) =>
                    right.transform.position.y.CompareTo(left.transform.position.y));

            int baseOrder = tagSortingOrder[tag] * 1000;
            for (int i = 0; i < objects.Length; i++)
            {
                if (objects[i].TryGetComponent(out SpriteRenderer renderer))
                {
                    renderer.sortingOrder = baseOrder + i;
                }

                if (objects[i].TryGetComponent(out Grass grass))
                {
                    grass.SetSortingFromSource();
                }
            }
        }
    }
}
