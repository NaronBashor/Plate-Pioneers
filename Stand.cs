using System.Collections.Generic;
using UnityEngine;

public class Stand : MonoBehaviour
{
    public string standName; // Name of the stand (e.g., Cake Stand, Donut Stand)
    public List<Food> foodHandled; // List of food items this stand is responsible for
    private Queue<Food> readyFoodQueue = new Queue<Food>(); // Queue to store ready food items

    public GameObject foodPrefab; // Food prefab to instantiate when picked up by the player

    // Add an order to the stand's queue and simulate cooking
    public void AddOrder(Food food)
    {
        readyFoodQueue.Enqueue(food);
        //Debug.Log($"{standName} has prepared {food.foodName}");
    }

    // Called by PlayerController when the player presses 'E' near the stand
    public GameObject PickupFood()
    {
        if (readyFoodQueue.Count > 0) {
            Food foodItem = readyFoodQueue.Dequeue();
            //Debug.Log($"{standName}: Player picked up {foodItem.foodName}");

            // Instantiate the food prefab at the stand's position (or a counter position)
            GameObject preparedFood = Instantiate(foodPrefab, transform.position, Quaternion.identity);
            preparedFood.GetComponent<FoodItemComponent>().food = foodItem;
            return preparedFood; // Return the instantiated food item for the player to pick up
        }

        return null; // No food available
    }

    // Check if there's food available for the player to pick up
    public bool HasFoodReady()
    {
        return readyFoodQueue.Count > 0;
    }
}
