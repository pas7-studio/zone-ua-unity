using UnityEngine;

public sealed class BallSpawner : MonoBehaviour
{
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private GameObject spawnPosition;
    [SerializeField, Min(1)] private int spawnBallNumber = 10;
    [SerializeField, Min(0f)] private float spawnRange = 1f;

    [SerializeField] private int leftCounter;
    [SerializeField] private int rightCounter;

    public void IncrementLeftCounter()
    {
        leftCounter = IncrementCounter(leftCounter);
    }

    public void IncrementRightCounter()
    {
        rightCounter = IncrementCounter(rightCounter);
    }

    private int IncrementCounter(int counter)
    {
        counter++;

        if (counter < spawnBallNumber)
        {
            return counter;
        }

        SpawnBall();
        return 0;
    }

    private void SpawnBall()
    {
        if (ballPrefab == null || spawnPosition == null)
        {
            return;
        }

        float xOffset = Random.Range(-spawnRange, spawnRange);
        Vector3 position = spawnPosition.transform.position + Vector3.right * xOffset;
        Instantiate(ballPrefab, position, Quaternion.identity);
    }
}
