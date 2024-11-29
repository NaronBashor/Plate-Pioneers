using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class ShopManager : MonoBehaviour
{
    public TextMeshProUGUI moneyTotalText;

    public List<GameObject> pages = new List<GameObject>();
    private int currentPageIndex = 0;

    public TextMeshProUGUI recipeCostText;
    public TextMeshProUGUI tableCostText;

    public List<GameObject> lockIcons = new List<GameObject>(); // For recipes
    public List<Button> recipeButtons = new List<Button>(); // For recipes
    public List<GameObject> tableLockIcons = new List<GameObject>(); // For tables
    public List<Button> tableButtons = new List<Button>(); // For tables

    public GameObject shopPanel;
    public GameObject player;

    private bool canOpenPanel;
    PlayerControls controls;

    private void Awake()
    {
        controls = new PlayerControls();
    }

    private void OnEnable()
    {
        controls.Player.Enable();
        controls.Player.TakeOrderAction.performed += OnOpenShopPanelButtonPressed;
        controls.Player.MenuClose.performed += OnCloseShopPanelButtonPressed;
        controls.Player.PageNav.performed += OnPageTurnedPressed;
    }

    private void OnDisable()
    {
        controls.Player.Disable();
        controls.Player.TakeOrderAction.performed -= OnOpenShopPanelButtonPressed;
        controls.Player.MenuClose.performed -= OnCloseShopPanelButtonPressed;
        controls.Player.PageNav.performed -= OnPageTurnedPressed;
    }

    private void Start()
    {
        SoundManager.Instance.StopSound(6);
        SoundManager.Instance.StopSound(4);
        SoundManager.Instance.PlaySound(5, true);

        canOpenPanel = false;
        // Initialize the buttons and lock icons based on saved progress
        UpdateRecipeUnlocks();
        UpdateTableUnlocks();
        shopPanel.SetActive(false);
    }

    private void Update()
    {
        moneyTotalText.text = GameSettings.playerMoney.ToString();
        // Check if all recipes or tables are fully upgraded
        if (AllRecipesPurchased()) {
            recipeCostText.text = "Congratulations on fully upgrading recipes!";
        } else {
            recipeCostText.text = "$" + GetRecipeCost(GameSettings.foodUnlocked).ToString() + " to purchase next recipe.";
        }

        if (AllTablesPurchased()) {
            tableCostText.text = "Congratulations on fully upgrading tables!";
        } else {
            tableCostText.text = "$" + GetTableCost(GameSettings.tablesPurchased).ToString() + " to purchase next table.";
        }
    }

    // Update the recipe unlock and purchase status
    public void UpdateRecipeUnlocks()
    {
        for (int i = 0; i < lockIcons.Count; i++) {
            if (GameSettings.recipePurchased[i]) {
                lockIcons[i].SetActive(false);
                recipeButtons[i].interactable = false;
                recipeButtons[i].GetComponent<Image>().color = Color.green; // Purchased
            } else if (GameSettings.recipeUnlocked[i]) {
                lockIcons[i].SetActive(false);
                recipeButtons[i].interactable = true;
                recipeButtons[i].GetComponent<Image>().color = Color.white; // Unlocked, ready to purchase
            } else {
                lockIcons[i].SetActive(true);
                recipeButtons[i].interactable = false;
                recipeButtons[i].GetComponent<Image>().color = Color.white; // Locked
            }
        }
    }

    // Update the table unlock and purchase status
    public void UpdateTableUnlocks()
    {
        for (int i = 0; i < tableLockIcons.Count; i++) {
            if (GameSettings.tablePurchased[i]) {
                tableLockIcons[i].SetActive(false);
                tableButtons[i].interactable = false;
                tableButtons[i].GetComponent<Image>().color = Color.green; // Purchased
            } else if (GameSettings.tableUnlocked[i]) {
                tableLockIcons[i].SetActive(false);
                tableButtons[i].interactable = true;
                tableButtons[i].GetComponent<Image>().color = Color.white; // Unlocked, ready to purchase
            } else {
                tableLockIcons[i].SetActive(true);
                tableButtons[i].interactable = false;
                tableButtons[i].GetComponent<Image>().color = Color.white; // Locked
            }
        }
    }

    // Check if all recipes are purchased
    private bool AllRecipesPurchased()
    {
        foreach (bool purchased in GameSettings.recipePurchased) {
            if (!purchased) {
                return false;
            }
        }
        return true; // All purchased
    }

    // Check if all tables are purchased
    private bool AllTablesPurchased()
    {
        foreach (bool purchased in GameSettings.tablePurchased) {
            if (!purchased) {
                return false;
            }
        }
        return true; // All purchased
    }

    // Purchase a new recipe
    public void OnNewRecipePurchase(int recipeIndex)
    {
        float recipeCost = GetRecipeCost(recipeIndex);
        if (CanPurchase(recipeCost)) {
            SoundManager.Instance.PlaySound(0, false);
            GameSettings.recipeUnlocked[recipeIndex] = true; // Mark as unlocked
            GameSettings.recipePurchased[recipeIndex] = true; // Mark as purchased
            if (recipeIndex + 1 < GameSettings.recipeUnlocked.Count) {
                GameSettings.recipeUnlocked[recipeIndex + 1] = true; // Unlock next recipe
            }
            GameSettings.foodUnlocked++;
            GameSettings.SpendMoney(recipeCost); // Deduct money
            UpdateRecipeUnlocks(); // Update the UI
        } else {
            Debug.Log($"Not enough money to purchase recipe ({GetRecipeCost(recipeIndex)}) at index {recipeIndex}.");
        }
    }

    // Purchase a new table
    public void OnNewTablePurchase(int tableIndex)
    {
        float tableCost = GetTableCost(tableIndex);
        if (CanPurchase(tableCost)) {
            SoundManager.Instance.PlaySound(0, false);
            GameSettings.tableUnlocked[tableIndex] = true; // Mark as unlocked
            GameSettings.tablePurchased[tableIndex] = true; // Mark as purchased
            if (tableIndex + 1 < GameSettings.tableUnlocked.Count) {
                GameSettings.tableUnlocked[tableIndex + 1] = true; // Unlock next table
            }
            GameSettings.tablesPurchased++;
            GameSettings.SpendMoney(tableCost); // Deduct money
            UpdateTableUnlocks(); // Update the UI
        } else {
            Debug.Log($"Not enough money to purchase table ({GetTableCost(tableIndex)}) at index {tableIndex}.");
        }
    }

    // Example of determining the cost of a recipe (increases with each recipe)
    private float GetRecipeCost(int recipeIndex)
    {
        return 200 + (recipeIndex * 75f);
    }

    // Example of determining the cost of a table (increases with each table)
    private float GetTableCost(int tableIndex)
    {
        return 300 + (tableIndex * 100f);
    }

    // Check if the player can afford a purchase
    private bool CanPurchase(float cost)
    {
        return GameSettings.playerMoney >= cost;
    }

    // Handle input for page navigation
    private void OnPageTurnedPressed(InputAction.CallbackContext ctx)
    {
        SoundManager.Instance.PlaySound(0, false);
        Vector2 navigationInput = ctx.ReadValue<Vector2>();

        if (navigationInput.x > 0) // Right bumper or right arrow
        {
            TurnPageRight();
        } else if (navigationInput.x < 0) // Left bumper or left arrow
          {
            TurnPageLeft();
        }
    }

    // Show the current page based on the index
    private void ShowCurrentPage()
    {
        // Hide all pages
        foreach (GameObject page in pages) {
            page.SetActive(false);
        }

        // Show only the current page
        if (currentPageIndex >= 0 && currentPageIndex < pages.Count) {
            pages[currentPageIndex].SetActive(true);
        }
    }

    // Navigate to the next page
    private void TurnPageRight()
    {
        if (currentPageIndex < pages.Count - 1) {
            currentPageIndex++;
            ShowCurrentPage();
        }
    }

    // Navigate to the previous page
    private void TurnPageLeft()
    {
        if (currentPageIndex > 0) {
            currentPageIndex--;
            ShowCurrentPage();
        }
    }

    public void OnOpenShopPanelButtonPressed(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && shopPanel != null)
            if (canOpenPanel) {
                shopPanel.SetActive(true);
                ShowCurrentPage();
                player.GetComponent<PlayerController>().CanMove = false;
            }
    }

    public void OnCloseShopPanelButtonPressed(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && shopPanel != null)
            shopPanel.SetActive(false);
        player.GetComponent<PlayerController>().CanMove = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision != null && collision.CompareTag("Player")) {
            canOpenPanel = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision != null && collision.CompareTag("Player")) {
            canOpenPanel = false;
        }
    }
}
