using UnityEngine;

[CreateAssetMenu(fileName = "food", menuName = "food/foodItem")]
public class Food : ScriptableObject
{
    public string foodName;       // Name of the food item
    public bool isDrink; // Add this to identify if the food is a drink
    public Sprite foodSprite;     // The sprite for the food item

    public enum FoodType          // Encapsulate the food type enum inside the Food class
    {
        Burger,
        Chicken,
        Eggs,
        Soup,
        Cake,
        Donuts,
        Water,
        Soda
    }

    public FoodType foodType;     // The type of food this item represents
}
