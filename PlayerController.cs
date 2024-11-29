using UnityEngine;
using UnityEngine.InputSystem; // For the new Input System

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;

    SpriteRenderer spriteRenderer;
    private Animator animator;
    private Rigidbody2D rb;

    private Vector2 moveInput; // Will store x and y input values

    // Hold points for Plate 1
    public Transform holdPoint1Up;
    public Transform holdPoint1Down;
    public Transform holdPoint1Left;
    public Transform holdPoint1Right;

    // Hold points for Plate 2
    public Transform holdPoint2Up;
    public Transform holdPoint2Down;
    public Transform holdPoint2Left;
    public Transform holdPoint2Right;

    // References to currently active hold points
    private Transform currentHoldPoint1; // The currently active hold point for Plate 1
    private Transform currentHoldPoint2; // The currently active hold point for Plate 2

    private GameObject heldItem1 = null; // The first plate the player is currently holding
    private GameObject heldItem2 = null; // The second plate the player is currently holding
    public float interactionRange = 2f; // How close the player needs to be to pick up the item

    public int baseSortingOrder = 100; // Base sorting order value for held items
    public float interactionDistance = 3f; // Distance to interact with the table

    private Table nearbyTable;
    private Stand nearbyStand;
    private TableInteraction nearbyTableInteraction;

    private int carryingPlates = 0; // 0 = no plates, 1 = one plate, 2 = two plates

    private bool canMove = true;

    public bool CanMove
    {
        get {
            return canMove;
        }
        set {
            canMove = value;
        }
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Initialize the current hold points to default to "down"
        currentHoldPoint1 = holdPoint1Down;
        currentHoldPoint2 = holdPoint2Down;
    }

    // This method will be called by the new Input System when the Move action is triggered
    public void OnMove(InputAction.CallbackContext context)
    {
        if (Time.timeScale == 0 || !canMove) { return; }
        moveInput = context.ReadValue<Vector2>(); // Read the movement input from the new input system
    }

    public void OnPickupDropItem(InputAction.CallbackContext context)
    {
        if (context.performed) // Only trigger on the performed phase
        {
            if (context.performed) {
                if (nearbyStand != null && nearbyStand.HasFoodReady()) {
                    TryPickupFromStand(); // Try to pick up food from the stand
                } else {
                    // If no items are being held, try to pick up an item
                    if (heldItem1 == null && heldItem2 == null) {
                        TryPickup(); // Try to pick up an item if there's an available hold point
                    }
                    // If one item is being held, allow the player to either pick up another or place/drop the current item
                    else if (heldItem1 != null && heldItem2 == null) {
                        TryPickup(); // Try to pick up the second item
                        if (heldItem2 == null) { // If no item was picked up, place or drop the first item
                            TryPlaceFood();
                            if (heldItem1 != null) {
                                //DropSingleItem(heldItem1);
                                //heldItem1 = null;
                                //carryingPlates = 0; // No plates held anymore
                            }
                        }
                    } else if (heldItem2 != null && heldItem1 == null) {
                        TryPickup(); // Try to pick up the first item
                        if (heldItem1 == null) { // If no item was picked up, place or drop the second item
                            TryPlaceFood();
                            if (heldItem2 != null) {
                                //DropSingleItem(heldItem2);
                                //heldItem2 = null;
                                //carryingPlates = 0; // No plates held anymore
                            }
                        }
                    }
                      // If both items are being held, try to place one or drop one
                      else if (heldItem1 != null && heldItem2 != null) {
                        // Try to place or drop the first item
                        TryPlaceFood();
                        if (heldItem1 != null) {
                            //DropSingleItem(heldItem1);
                            //heldItem1 = null;
                            //carryingPlates = 1; // Still holding the second item
                        } else if (heldItem2 != null) {
                            TryPlaceFood();
                        }
                    }
                }
            }
        }
    }

    private void TryPickupFromStand()
    {
        // Pickup food if possible
        if (heldItem1 == null) {
            SoundManager.Instance.PlaySound(9, false);
            GameObject foodItem = nearbyStand.PickupFood();
            if (foodItem != null) {
                SoundManager.Instance.PlaySound(9, false);
                heldItem1 = foodItem;
                heldItem1.transform.position = currentHoldPoint1.position;
                heldItem1.transform.SetParent(currentHoldPoint1);
                carryingPlates = 1;
                // Set up sorting or any other components here
            }
        } else if (heldItem2 == null) {
            GameObject foodItem = nearbyStand.PickupFood();
            if (foodItem != null) {
                SoundManager.Instance.PlaySound(9, false);
                heldItem2 = foodItem;
                heldItem2.transform.position = currentHoldPoint2.position;
                heldItem2.transform.SetParent(currentHoldPoint2);
                carryingPlates = 2;
                // Set up sorting or any other components here
            }
        }
    }

    public void OnTableTakeOrder(InputAction.CallbackContext context)
    {
        if (context.performed) {
            // Ensure nearbyTable is not null
            if (nearbyTable != null) {
                TableInteraction tableInteraction = nearbyTable.GetComponent<TableInteraction>();

                // Ensure TableInteraction component is found
                if (tableInteraction != null) {
                    if (IsPlayerCloseToTable()) {
                        if (tableInteraction.CheckIfReadyToOrder()) {
                            tableInteraction.HandleTableClick();
                        }
                    }
                } else {
                    //Debug.LogError("TableInteraction component not found on the nearby table.");
                }
            } else {
                //Debug.LogError("No nearby table detected.");
            }
        }
    }


    // Check if the player is within range of the table
    private bool IsPlayerCloseToTable()
    {
        return Vector3.Distance(transform.position, transform.position) <= interactionDistance;
    }

    private void Update()
    {
        if (!canMove) { return; }
        UpdatePlayerMovementAndAnimations();
    }

    private void FixedUpdate()
    {
        if (!canMove) { rb.velocity = Vector2.zero; animator.SetBool("isMoving", false); return; }
        // Move the player using Rigidbody2D to handle physics-based movement
        Vector2 moveVelocity = moveInput * moveSpeed * GameSettings.miniGameBoost;
        rb.velocity = moveVelocity;

        // If holding items, update their positions
        if (heldItem1 != null) heldItem1.transform.position = currentHoldPoint1.position;
        if (heldItem2 != null) heldItem2.transform.position = currentHoldPoint2.position;
    }

    private void TryPickup()
    {
        // Use OverlapCircle to detect nearby food items
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, interactionRange);

        foreach (Collider2D hit in hitColliders) {
            if (hit.CompareTag("Food")) {
                GameObject foodItem = hit.gameObject;

                // Check if the player is holding no items
                if (heldItem1 == null)  // First plate is empty, pick it up
                {
                    SoundManager.Instance.PlaySound(9, false);
                    PickUpItem(foodItem, 1); // Pick up the first item
                    carryingPlates = 1;      // Now holding 1 plate
                    return;                  // Exit after picking up the first item
                }
                // Check if the player is holding only one item (heldItem1 is not null, but heldItem2 is)
                else if (heldItem2 == null && heldItem1 != foodItem) // Make sure we're not picking the same item again
                {
                    SoundManager.Instance.PlaySound(9, false);
                    PickUpItem(foodItem, 2); // Pick up the second item
                    carryingPlates = 2;      // Now holding 2 plates
                    return;                  // Exit after picking up the second item
                }
            }
        }
    }

    private void TryPlaceFood()
    {
        if (nearbyTable == null) {
            //Debug.Log("No table nearby to place food.");
            return;
        }

        // Place heldItem1 if it's not null
        if (heldItem1 != null) {
            Food foodItem = heldItem1.GetComponent<FoodItemComponent>().food;
            if (foodItem != null && nearbyTable.CheckIfOrdered(foodItem)) {
                SoundManager.Instance.PlaySound(9, false);
                PlaceFoodAtTable(heldItem1, foodItem);
                heldItem1 = null;
                carryingPlates = (heldItem2 == null) ? 0 : 1; // Update the plates count
                CheckIfAllFoodServed(); // After placing, check if all food has been served
            }
        }

        // Place heldItem2 if it's not null
        else if (heldItem2 != null) {
            Food foodItem = heldItem2.GetComponent<FoodItemComponent>().food;
            if (foodItem != null && nearbyTable.CheckIfOrdered(foodItem)) {
                SoundManager.Instance.PlaySound(9, false);
                PlaceFoodAtTable(heldItem2, foodItem);
                heldItem2 = null;
                carryingPlates = (heldItem1 == null) ? 0 : 1; // Update the plates count
                CheckIfAllFoodServed(); // After placing, check if all food has been served
            }
        }
    }

    private void CheckIfAllFoodServed()
    {
        if (nearbyTable != null && nearbyTable.orderedFoodItems.Count == 0) // All ordered food items have been removed (served)
        {
            nearbyTableInteraction.SetCustomersToEatingPhase();
        }
    }

    private void PlaceFoodAtTable(GameObject foodItem, Food foodItemType)
    {
        // Get the nearest available snap point
        Transform snapPoint = nearbyTable.GetAvailableSnapPoint();

        if (snapPoint != null) {
            //Debug.Log("Attempting to place food at table...");

            // Snap the food item to the snap point
            foodItem.transform.position = snapPoint.position;
            foodItem.transform.SetParent(snapPoint); // Parent it to the table for better organization

            foodItem.tag = "Untagged";

            // Set sorting layer and order
            SpriteRenderer sr = foodItem.GetComponent<SpriteRenderer>();
            if (sr != null) {
                sr.sortingLayerName = "Walk In Front";  // Set the sorting layer
                sr.sortingOrder = 3;  // Set sorting order to 3
                //Debug.Log("Food item set to sorting layer: Walk In Front with sorting order 3.");
            } else {
                //Debug.LogError("No SpriteRenderer found on the food item.");
            }

            // Optionally, disable any physics so the item doesn't move
            Rigidbody2D rb = foodItem.GetComponent<Rigidbody2D>();
            if (rb != null) {
                rb.isKinematic = true; // Make it static
            }

            // Remove the food from the table's ordered list
            //Debug.Log("Removing food item from ordered list...");
            nearbyTable.RemoveFoodItem(foodItemType);
        } else {
            //Debug.Log("No available snap points on the table.");
        }
    }



    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Table")) {
            nearbyTable = other.GetComponent<Table>();
            nearbyTableInteraction = other.GetComponent<TableInteraction>();

            if (nearbyTable != null) {
                //Debug.Log("Entered table trigger zone, table assigned.");
            } else {
                //Debug.LogWarning("Table component not found on the object.");
            }
        }
        if (other.CompareTag("Stand")) {
            nearbyStand = other.GetComponent<Stand>();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Table")) {
            //Debug.Log("Exited table trigger zone, table unassigned.");
            nearbyTable = null;
            nearbyTableInteraction = null;
        }
        if (other.CompareTag("Stand")) {
            nearbyStand = null;
        }
    }
    private void PickUpItem(GameObject item, int holdPointNumber)
    {
        if (holdPointNumber == 1 && heldItem1 == null) {
            heldItem1 = item;
            item.transform.position = currentHoldPoint1.position;
            item.transform.SetParent(currentHoldPoint1);
        } else if (holdPointNumber == 2 && heldItem2 == null) {
            heldItem2 = item;
            item.transform.position = currentHoldPoint2.position;
            item.transform.SetParent(currentHoldPoint2);
        }
    }


    private void DropSingleItem(GameObject item)
    {
        // Release the snap point when the food is picked up
        if (item.transform.parent != null && item.transform.parent.CompareTag("Table")) {
            Table table = item.transform.parent.GetComponent<Table>();
            if (table != null) {
                table.ReleaseSnapPoint(item.transform); // Release the snap point
            }
        }

        item.transform.SetParent(null);
    }

    private void UpdatePlayerMovementAndAnimations()
    {
        // Set the input values in the Animator to control the animations
        animator.SetFloat("xInput", moveInput.x);
        animator.SetFloat("yInput", moveInput.y);
        animator.SetInteger("Plates", carryingPlates);

        // Check which input to prioritize (horizontal has priority if both are pressed)
        if (Mathf.Abs(moveInput.x) >= Mathf.Abs(moveInput.y)) {
            // Prioritize horizontal movement
            animator.SetFloat("xInput", moveInput.x);
            animator.SetFloat("yInput", 0);  // Reset y movement in Animator to ensure horizontal animation plays

            // Handle sprite flipping for horizontal movement
            if (moveInput.x < 0) {
                spriteRenderer.flipX = true; // Face left
                currentHoldPoint1 = holdPoint1Left;
                currentHoldPoint2 = holdPoint2Left;
                if (heldItem1 != null) {
                    SpriteRenderer sr1 = heldItem1.GetComponent<SpriteRenderer>();
                    if (sr1 != null) {
                        // Update sorting order based on player's Y position
                        sr1.sortingOrder = -100;
                    }
                }

                if (heldItem2 != null) {
                    SpriteRenderer sr2 = heldItem2.GetComponent<SpriteRenderer>();
                    if (sr2 != null) {
                        // Update sorting order based on player's Y position
                        sr2.sortingOrder = 100;
                    }
                }
            } else if (moveInput.x > 0) {
                spriteRenderer.flipX = false; // Face right
                currentHoldPoint1 = holdPoint1Right;
                currentHoldPoint2 = holdPoint2Right;
                if (heldItem1 != null) {
                    SpriteRenderer sr1 = heldItem1.GetComponent<SpriteRenderer>();
                    if (sr1 != null) {
                        // Update sorting order based on player's Y position
                        sr1.sortingOrder = 100;
                    }
                }

                if (heldItem2 != null) {
                    SpriteRenderer sr2 = heldItem2.GetComponent<SpriteRenderer>();
                    if (sr2 != null) {
                        // Update sorting order based on player's Y position
                        sr2.sortingOrder = -100;
                    }
                }
            } else {
                currentHoldPoint1 = holdPoint1Down;
                currentHoldPoint2 = holdPoint2Down;
                if (heldItem1 != null) {
                    SpriteRenderer sr1 = heldItem1.GetComponent<SpriteRenderer>();
                    if (sr1 != null) {
                        // Update sorting order based on player's Y position
                        sr1.sortingOrder = 100;
                    }
                }

                if (heldItem2 != null) {
                    SpriteRenderer sr2 = heldItem2.GetComponent<SpriteRenderer>();
                    if (sr2 != null) {
                        // Update sorting order based on player's Y position
                        sr2.sortingOrder = 100;
                    }
                }
            }
        } else {
            // Prioritize vertical movement
            animator.SetFloat("xInput", 0);  // Reset x movement in Animator to ensure vertical animation plays
            animator.SetFloat("yInput", moveInput.y);

            // Do not flip for vertical movement
            spriteRenderer.flipX = false;

            if (moveInput.y > 0) // Moving up
            {
                currentHoldPoint1 = holdPoint1Up;
                currentHoldPoint2 = holdPoint2Up;
                if (heldItem1 != null) {
                    SpriteRenderer sr1 = heldItem1.GetComponent<SpriteRenderer>();
                    if (sr1 != null) {
                        // Update sorting order based on player's Y position
                        sr1.sortingOrder = -100;
                    }
                }

                if (heldItem2 != null) {
                    SpriteRenderer sr2 = heldItem2.GetComponent<SpriteRenderer>();
                    if (sr2 != null) {
                        // Update sorting order based on player's Y position
                        sr2.sortingOrder = -100;
                    }
                }
            } else if (moveInput.y < 0) // Moving down
              {
                currentHoldPoint1 = holdPoint1Down;
                currentHoldPoint2 = holdPoint2Down;
                if (heldItem1 != null) {
                    SpriteRenderer sr1 = heldItem1.GetComponent<SpriteRenderer>();
                    if (sr1 != null) {
                        // Update sorting order based on player's Y position
                        sr1.sortingOrder = 100;
                    }
                }

                if (heldItem2 != null) {
                    SpriteRenderer sr2 = heldItem2.GetComponent<SpriteRenderer>();
                    if (sr2 != null) {
                        // Update sorting order based on player's Y position
                        sr2.sortingOrder = 100;
                    }
                }
            } else {
                currentHoldPoint1 = holdPoint1Down;
                currentHoldPoint2 = holdPoint2Down;
                if (heldItem1 != null) {
                    SpriteRenderer sr1 = heldItem1.GetComponent<SpriteRenderer>();
                    if (sr1 != null) {
                        // Update sorting order based on player's Y position
                        sr1.sortingOrder = 100;
                    }
                }

                if (heldItem2 != null) {
                    SpriteRenderer sr2 = heldItem2.GetComponent<SpriteRenderer>();
                    if (sr2 != null) {
                        // Update sorting order based on player's Y position
                        sr2.sortingOrder = 100;
                    }
                }
            }
        }

        // Update the animation state based on movement
        animator.SetBool("isMoving", moveInput.magnitude > 0);
    }
}