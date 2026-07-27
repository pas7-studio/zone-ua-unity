using System.Collections.Generic;
using UnityEngine;

public class TestWorldSorting : MonoBehaviour
{
    // A list of all tags that need to be sorted
    public List<string> tagsToSort;

    // A dictionary to store the sorting order for each tag
    private Dictionary<string, int> tagSortingOrder = new Dictionary<string, int>();

    public void SortAll()
    {
        // Initialize the tagSortingOrder dictionary
        for (int i = 0; i < tagsToSort.Count; i++)
        {
            tagSortingOrder[tagsToSort[i]] = i;
        }

        // Sort the sprites by tag and then by y-axis position
        foreach (string tag in tagsToSort)
        {
            // Find all sprites with the current tag
            GameObject[] spritesWithTag = GameObject.FindGameObjectsWithTag(tag);

            // Sort the sprites with the current tag by y-axis position
            System.Array.Sort(spritesWithTag, (a, b) => b.transform.position.y.CompareTo(a.transform.position.y));

            // Assign sorting order levels to the sorted sprites
            for (int i = 0; i < spritesWithTag.Length; i++)
            {
                spritesWithTag[i].GetComponent<SpriteRenderer>().sortingOrder = i + (tagSortingOrder[tag] * 1000);
                var grass = spritesWithTag[i].GetComponent<Grass>();
                if(grass != null ) {
                    grass.setSortingFromSource();
                }
            }
        }
    }
}