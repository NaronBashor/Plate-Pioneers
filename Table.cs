using UnityEngine;
using System.Collections.Generic;

public class Table : MonoBehaviour
{
    public Transform[] seats; // Array of seat positions (2 or 4 seats depending on the table)
    public bool[] facesAway;

    public List<Food> orderedFoodItems = new List<Food>(); // List of food ordered at this table
    public Transform[] foodSnapPoints; // Array of snap points for food placement
    public bool[] snapPointOccupied; // To track if a snap point is occupied

    public GameObject dirtyDishesPrefab; // The prefab for the dirty dishes sprite
    public Transform centerPoint; // A transform for the center of the table

    private void Start()
    {
        // Initialize the snap point occupation status
        snapPointOccupied = new bool[foodSnapPoints.Length];
    }

    // Example method to check if the food item matches the order
    public bool CheckIfOrdered(Food foodItem)
    {
        return orderedFoodItems.Contains(foodItem); // Check if the ordered food contains the food item
    }

    // Remove the food item once it's been placed
    public void RemoveFoodItem(Food foodItem)
    {
        if (orderedFoodItems != null && orderedFoodItems.Contains(foodItem)) {
            orderedFoodItems.Remove(foodItem);  // Remove the food item from the list
        } else {
            //Debug.LogError("Food item not found in orderedFoodItems list!");
        }
    }

    // Get the nearest available snap point
    public Transform GetAvailableSnapPoint()
    {
        for (int i = 0; i < foodSnapPoints.Length; i++)
        {
            if (!snapPointOccupied[i]) // If the snap point is not occupied
            {
                snapPointOccupied[i] = true; // Mark it as occupied
                return foodSnapPoints[i]; // Return this snap point
            }
        }
        return null; // No available snap points
    }

    // Release a snap point when food is removed
    public void ReleaseSnapPoint(Transform snapPoint)
    {
        for (int i = 0; i < foodSnapPoints.Length; i++)
        {
            if (foodSnapPoints[i] == snapPoint)
            {
                snapPointOccupied[i] = false; // Mark this snap point as available again
                break;
            }
        }
    }

    public void ClearFoodAndPlaceDirtyDishes()
    {
        foreach (Transform snapPoint in foodSnapPoints) {
            foreach (Transform child in snapPoint) {
                Destroy(child.gameObject); // Remove all food
            }
        }

        GameObject dirtyDishes = Instantiate(dirtyDishesPrefab, centerPoint.position, Quaternion.identity);
        dirtyDishes.transform.SetParent(centerPoint);
    }
}
