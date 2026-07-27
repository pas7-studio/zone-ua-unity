using UnityEngine;

public class CharacterChunks : MonoBehaviour
{
    private int currentChunk = -1;

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Chunk"))
        {
            int newChunk = other.gameObject.GetInstanceID();

            if (newChunk != currentChunk)
            {
                currentChunk = newChunk;

                GrassSorting[] grassSortings = FindObjectsOfType<GrassSorting>();

                foreach (GrassSorting grassSorting in grassSortings)
                {
                    grassSorting.enabled = false;
                }

                other.GetComponent<GrassSorting>().enabled = true;
            }
        }
    }
}