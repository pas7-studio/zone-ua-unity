using UnityEngine;

public class BallSpawner : MonoBehaviour
{
    [SerializeField] private GameObject ballPrefab; // The prefab of the ball GameObject to spawn
    [SerializeField] private GameObject spawnPosition; // The position to spawn the ball GameObject
    [SerializeField] private int spawnBallNumber = 10; // The number of balls to spawn before stopping
    [SerializeField] private float spawnRange = 1.0f;

    [SerializeField] private int leftCounter = 0; // The current count for the left side
    [SerializeField] private int rightCounter = 0; // The current count for the right side

    // Call this function to increment the left counter
    public void IncrementLeftCounter()
    {
        leftCounter++;

        // Check if the left counter has reached the spawn ball number
        if (leftCounter >= spawnBallNumber)
        {
            SpawnBall(); // Spawn a ball GameObject
            leftCounter = 0; // Reset the left counter
        }
    }

    // Call this function to increment the right counter
    public void IncrementRightCounter()
    {
        rightCounter++;

        // Check if the right counter has reached the spawn ball number
        if (rightCounter >= spawnBallNumber)
        {
            SpawnBall(); // Spawn a ball GameObject
            rightCounter = 0; // Reset the right counter
        }
    }

    // Spawn a ball GameObject at the preset position
    private void SpawnBall()
    {
        // Calculate a random X position offset based on the spawn range
        float xOffset = Random.Range(-spawnRange, spawnRange);

        // Calculate the spawn position with the X position offset
        Vector3 spawnPosition = this.spawnPosition.transform.position + new Vector3(xOffset, 0f, 0f);

        // Spawn the ball GameObject at the calculated spawn position
        Instantiate(ballPrefab, spawnPosition, Quaternion.identity);
    }

}