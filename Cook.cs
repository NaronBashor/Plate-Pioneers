using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using static CustomerController;

public class Cook : MonoBehaviour
{
    Animator anim;

    public float cookTime = 5f; // Cooking time for each item
    public string cookName; // Name of the cook (e.g., Burger Cook, Chicken Cook)
    public List<Food> foodHandled; // List of food items this cook is responsible for
    private Queue<Food> orderQueue = new Queue<Food>(); // Queue to store incoming orders

    public Transform counterPosition; // The position where finished food will be placed
    public GameObject foodItemPrefab; // Prefab for the soup

    private bool isProcessingOrder = false; // Flag to indicate if the cook is currently processing an order

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (!isProcessingOrder && orderQueue.Count > 0) {
            StartCooking(); // Only process if not already processing
        }
    }

    public void AddOrder(Food food)
    {
        orderQueue.Enqueue(food);
        // You can start processing this order here, or queue them for later processing
        StartCooking();
    }

    private void StartCooking()
    {
        if (orderQueue.Count > 0) {
            Food currentOrder = orderQueue.Dequeue();
            //Debug.Log($"Cooking {currentOrder.foodName}");
            // Process the cooking (start coroutine, timer, etc.)
            StartCoroutine(CookFood(currentOrder));
        }
    }

    private IEnumerator CookFood(Food food)
    {
        SoundManager.Instance.PlaySound(2, false);
        anim.SetBool("isCooking", true);
        // Simulate cooking time (for example, 5 seconds)
        yield return new WaitForSeconds(5f);

        anim.SetBool("isCooking", false);
        // Instantiate the food prefab after cooking is done
        GameObject preparedFoodPrefab = Instantiate(foodItemPrefab, counterPosition.position, Quaternion.identity);
        preparedFoodPrefab.GetComponent<SpriteRenderer>().sortingOrder = -1;
        preparedFoodPrefab.GetComponent<FoodItemComponent>().food = food; // Assign the correct food to the prefab

        //Debug.Log($"Finished cooking {food.foodName}. Placing it on the counter.");
    }
}
