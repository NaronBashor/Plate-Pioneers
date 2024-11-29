using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;

public static class GameSettings
{
    public static bool gameControlsPanelShown = false;
    public static int foodUnlocked = 1; // Tracks how many recipes are unlocked.

    // New: List to track if each recipe is unlocked.
    public static List<bool> recipeUnlocked = new List<bool> { true, false, false, false, false, false };
    public static List<bool> recipePurchased = new List<bool> { false, false, false, false, false, false };

    public static List<bool> tableUnlocked = new List<bool> { true, false, false, false, false, false, false };
    public static List<bool> tablePurchased = new List<bool> { false, false, false, false, false, false, false };

    public static float miniGameBoost = 1;
    public static bool ranMiniGameToday = false;
    public static bool firstTime = true;

    // Existing settings
    public static int tablesPurchased = 1;
    public static float customerSpawnTimer = 5f;
    public static float baseRewardAmount = 10f;
    public static float currentRewardMultiplier = 1f;

    // Customer-related settings
    public static float standingInLineBoredTime = 10f;
    public static float standingInLineAngryTime = 20f;
    public static float waitingForFoodBoredTime = 10f;
    public static float waitingForFoodAngryTime = 20f;
    public static float customerMoveSpeed = 2f;
    public static float customerStopDistance = 1.5f;
    public static float lookAtMenuTime = 10f;
    public static float eatingTime = 10f;

    public static float playerMoney = 0f; // Starting money

    // Check if save data exists
    public static bool IsSaveDataAvailable()
    {
        string path = Application.persistentDataPath + "/gamesettings.save";
        return File.Exists(path);
    }

    // Method to add money
    public static void AddMoney(float amount)
    {
        playerMoney += amount;
        SaveSettings();
    }

    // Method to subtract money
    public static bool SpendMoney(float amount)
    {
        if (playerMoney >= amount) {
            playerMoney -= amount;
            SaveSettings();
            return true;
        }
        return false;
    }

    // Method to save the settings to a file
    public static void SaveSettings()
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/gamesettings.save";
        FileStream stream = new FileStream(path, FileMode.Create);

        GameSettingsData data = new GameSettingsData();
        formatter.Serialize(stream, data);
        stream.Close();
    }

    // Method to load the settings from a file
    public static void LoadSettings()
    {
        string path = Application.persistentDataPath + "/gamesettings.save";
        if (File.Exists(path)) {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            GameSettingsData data = formatter.Deserialize(stream) as GameSettingsData;
            stream.Close();

            // Load the settings into the static class
            gameControlsPanelShown = data.gameControlsPanelShown;
            foodUnlocked = data.foodUnlocked;
            recipeUnlocked = data.recipeUnlocked; // Load recipe unlocks
            recipePurchased = data.recipePurchased;

            tableUnlocked = data.tableUnlocked;
            tablePurchased = data.tablePurchased;

            miniGameBoost = data.miniGameBoost;
            ranMiniGameToday = data.ranMiniGameToday;
            firstTime = data.firstTime;

            tablesPurchased = data.tablesPurchased;
            customerSpawnTimer = data.customerSpawnTimer;
            baseRewardAmount = data.baseRewardAmount;
            currentRewardMultiplier = data.currentRewardMultiplier;

            // Load customer-related settings
            standingInLineBoredTime = data.standingInLineBoredTime;
            standingInLineAngryTime = data.standingInLineAngryTime;
            waitingForFoodBoredTime = data.waitingForFoodBoredTime;
            waitingForFoodAngryTime = data.waitingForFoodAngryTime;
            customerMoveSpeed = data.customerMoveSpeed;
            customerStopDistance = data.customerStopDistance;
        } else {
            Debug.LogError("Save file not found in " + path);
        }
    }

    // Method to reset to default settings
    public static void ResetToDefault()
    {
        gameControlsPanelShown = false;
        foodUnlocked = 1;
        recipeUnlocked = new List<bool> { true, false, false, false, false, false }; // Reset all recipes
        recipePurchased = new List<bool> { false, false, false, false, false, false };

        tableUnlocked = new List<bool> { true, false, false, false, false, false, false };
        tablePurchased = new List<bool> { false, false, false, false, false, false, false };

        miniGameBoost = 1;
        ranMiniGameToday = false;
        firstTime = true;

        tablesPurchased = 1;
        customerSpawnTimer = 5f;
        baseRewardAmount = 10f;
        currentRewardMultiplier = 1f;

        standingInLineBoredTime = 10f;
        standingInLineAngryTime = 20f;
        waitingForFoodBoredTime = 10f;
        waitingForFoodAngryTime = 20f;
        customerMoveSpeed = 2f;
        customerStopDistance = 1.5f;
        lookAtMenuTime = 10f;
        eatingTime = 10f;

        playerMoney = 0f;
        SaveSettings();
    }
}

// Data class for serialization
[System.Serializable]
public class GameSettingsData
{
    public bool gameControlsPanelShown;
    public int foodUnlocked;

    public List<bool> recipeUnlocked;  // List to store unlocked recipes
    public List<bool> recipePurchased;

    public List<bool> tableUnlocked;
    public List<bool> tablePurchased;

    public float miniGameBoost = 1;
    public bool ranMiniGameToday = false;
    public bool firstTime = true;

    public int tablesPurchased;
    public float customerSpawnTimer;
    public float baseRewardAmount;
    public float currentRewardMultiplier;

    // Customer-related settings
    public float standingInLineBoredTime;
    public float standingInLineAngryTime;
    public float waitingForFoodBoredTime;
    public float waitingForFoodAngryTime;
    public float customerMoveSpeed;
    public float customerStopDistance;

    public GameSettingsData()
    {
        gameControlsPanelShown = GameSettings.gameControlsPanelShown;
        foodUnlocked = GameSettings.foodUnlocked;

        // Copy recipe unlocked status
        recipeUnlocked = new List<bool>(GameSettings.recipeUnlocked);
        recipePurchased = new List<bool>(GameSettings.recipePurchased);

        tableUnlocked = new List<bool>(GameSettings.tableUnlocked);
        tablePurchased = new List<bool>(GameSettings.tablePurchased);

        miniGameBoost = GameSettings.miniGameBoost;
        ranMiniGameToday = GameSettings.ranMiniGameToday;
        firstTime = GameSettings.firstTime;

        tablesPurchased = GameSettings.tablesPurchased;
        customerSpawnTimer = GameSettings.customerSpawnTimer;
        baseRewardAmount = GameSettings.baseRewardAmount;
        currentRewardMultiplier = GameSettings.currentRewardMultiplier;

        // Copy customer-related settings
        standingInLineBoredTime = GameSettings.standingInLineBoredTime;
        standingInLineAngryTime = GameSettings.standingInLineAngryTime;
        waitingForFoodBoredTime = GameSettings.waitingForFoodBoredTime;
        waitingForFoodAngryTime = GameSettings.waitingForFoodAngryTime;
        customerMoveSpeed = GameSettings.customerMoveSpeed;
        customerStopDistance = GameSettings.customerStopDistance;
    }
}
