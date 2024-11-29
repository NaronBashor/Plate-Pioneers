using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodFireworks : MonoBehaviour
{
    public GameObject[] foodIcons; // Array of food icons to be used as fireworks
    public Transform launchPosition; // The position where the food icons will be launched from
    public float launchForceMin = 300f; // Minimum launch force
    public float launchForceMax = 600f; // Maximum launch force
    public float explosionTime = 2f; // Time after which the food icons disappear
    public int numberOfIcons = 5; // Number of food icons to launch

    public float minLaunchAngle = 75f; // Minimum angle for launch (upwards cone, in degrees)
    public float maxLaunchAngle = 105f; // Maximum angle for launch (upwards cone, in degrees)

    // Call this function when the player successfully completes the mini-game

    public void LaunchFoodFireworks()
    {
        for (int i = 0; i < numberOfIcons; i++) {
            // Pick a random food icon
            GameObject randomFoodIcon = foodIcons[Random.Range(0, foodIcons.Length)];

            // Instantiate the food icon at the launch position
            GameObject foodInstance = Instantiate(randomFoodIcon, launchPosition.position, Quaternion.identity);

            // Add random force to the food icon for the firework effect
            Rigidbody2D rb = foodInstance.AddComponent<Rigidbody2D>();

            // Generate a random angle between the minLaunchAngle and maxLaunchAngle
            float randomAngle = Random.Range(minLaunchAngle, maxLaunchAngle);

            // Convert the angle to a direction vector
            Vector2 launchDirection = AngleToVector2(randomAngle);

            // Generate a random launch force
            float randomForce = Random.Range(launchForceMin, launchForceMax);

            // Apply the random force in the calculated direction
            rb.AddForce(launchDirection * randomForce);

            // Optionally, add some random rotation
            float randomTorque = Random.Range(-100f, 100f); // Random rotation force
            rb.AddTorque(randomTorque);

            // Start coroutine to destroy the food icon after a short delay
            StartCoroutine(DestroyFoodIconAfterTime(foodInstance, explosionTime));
        }
    }

    // Convert an angle (in degrees) to a Vector2 direction
    private Vector2 AngleToVector2(float angle)
    {
        float radians = angle * Mathf.Deg2Rad; // Convert angle to radians
        return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)); // Return a direction based on angle
    }

    // Coroutine to destroy the food icon after a set amount of time
    IEnumerator DestroyFoodIconAfterTime(GameObject foodIcon, float delay)
    {
        yield return new WaitForSeconds(delay);

        // Optionally, fade out the food icon before destroying
        SpriteRenderer sr = foodIcon.GetComponent<SpriteRenderer>();
        if (sr != null) {
            for (float t = 0; t < 1f; t += Time.deltaTime) {
                Color color = sr.color;
                color.a = Mathf.Lerp(1, 0, t); // Fade out
                sr.color = color;
                yield return null;
            }
        }

        Destroy(foodIcon); // Destroy the food icon after the delay
    }
}
