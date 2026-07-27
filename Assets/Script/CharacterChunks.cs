using UnityEngine;

public sealed class CharacterChunks : MonoBehaviour
{
    private GrassSorting activeSorting;

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryActivateChunk(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (activeSorting == null)
        {
            TryActivateChunk(other);
        }
    }

    private void TryActivateChunk(Collider2D other)
    {
        if (!other.CompareTag("Chunk") ||
            !other.TryGetComponent(out GrassSorting newSorting) ||
            newSorting == activeSorting)
        {
            return;
        }

        if (activeSorting != null)
        {
            activeSorting.enabled = false;
        }

        activeSorting = newSorting;
        activeSorting.enabled = true;
    }
}
