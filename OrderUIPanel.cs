using UnityEngine;

public class OrderUIPanel : MonoBehaviour
{
    public SpriteRenderer[] orderBoxes; // Array of order box UI slots

    // Clear the order boxes when starting
    private void Start()
    {
        ClearOrderBoxes();
    }

    public void UpdateOrderUI(Food food, int customerIndex)
    {
        if (customerIndex >= 0 && customerIndex < orderBoxes.Length && food != null) {
            // Assign the sprite from the Food ScriptableObject
            //Debug.Log($"Updating UI for customer {customerIndex} with food: {food.foodName}");
            orderBoxes[customerIndex].sprite = food.foodSprite;
        }
    }


    // Clear all the order boxes by setting them to null or a default sprite
    public void ClearOrderBoxes()
    {
        foreach (SpriteRenderer box in orderBoxes) {
            box.sprite = null; // Or assign a default empty sprite if needed
        }
    }
}
