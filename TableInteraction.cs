using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class TableInteraction : MonoBehaviour
{
    public OrderUIPanel orderUIPanel; // Reference to UI for showing orders
    public List<Food> orderedFoodItems = new List<Food>(); // List of ordered food items

    public Table table; // Reference to the table component

    public float money;

    private Dictionary<Transform, CustomerController> seatAssignments = new Dictionary<Transform, CustomerController>();

    private bool isTableReset = false;
    private bool isResetting = false;

    private void Start()
    {
        orderUIPanel = GetComponent<OrderUIPanel>();
    }

    // Assign a customer to a seat
    public void AssignCustomerToSeat(CustomerController customer, Transform seat)
    {
        if (!seatAssignments.ContainsKey(seat)) {
            seatAssignments.Add(seat, customer);
            customer.currentTable = this;
        }
    }

    // Handle clicking on the table to take an order
    public void HandleTableClick()
    {
        TakeOrder();
    }

    // Check if any customers are ready to order
    public bool CheckIfReadyToOrder()
    {
        foreach (var seat in seatAssignments.Keys) {
            CustomerController customer = seatAssignments[seat];
            if (customer != null && customer.currentPhase == CustomerController.CustomerPhase.CallingWaiter) {
                return true;
            }
        }
        return false;
    }

    // Take orders from all seated customers
    private void TakeOrder()
    {
        int customerIndex = 0;
        foreach (var seat in seatAssignments.Keys) {
            CustomerController customer = seatAssignments[seat];
            if (customer != null && customer.currentPhase == CustomerController.CustomerPhase.CallingWaiter) {
                customer.AssignRandomOrder();
                orderedFoodItems.Add(customer.orderedFood);
                table.orderedFoodItems.Add(customer.orderedFood);

                customer.SetWaitingForFood();
                orderUIPanel.UpdateOrderUI(customer.orderedFood, customerIndex);
                customerIndex++;

                RouteOrderToCookOrStand(customer.orderedFood);
            }
        }
    }

    // Route the order to the appropriate cook or stand
    private void RouteOrderToCookOrStand(Food orderedFood)
    {
        Cook[] cooks = FindObjectsOfType<Cook>();
        Stand[] stands = FindObjectsOfType<Stand>();

        foreach (Cook cook in cooks) {
            if (cook.foodHandled.Contains(orderedFood)) {
                cook.AddOrder(orderedFood);
                return;
            }
        }

        foreach (Stand stand in stands) {
            if (stand.foodHandled.Contains(orderedFood)) {
                stand.AddOrder(orderedFood);
                return;
            }
        }
    }

    // Set customers to the eating phase
    public void SetCustomersToEatingPhase()
    {
        foreach (var seat in seatAssignments.Keys) {
            CustomerController customer = seatAssignments[seat];
            if (customer != null && customer.currentPhase == CustomerController.CustomerPhase.WaitingForFood) {
                customer.SetEating(); // This sets each customer to the Eating phase
            }
        }
    }

    private void RemoveAllCustomersFromTable()
    {
        foreach (var seat in seatAssignments.Keys) {
            CustomerController customer = seatAssignments[seat];
            if (customer != null) {
                Destroy(customer.gameObject);
            }
        }

        seatAssignments.Clear();
    }

    public void BusTable()
    {
        table.ClearFoodAndPlaceDirtyDishes();
        StartCoroutine(DelayedResetTable(2f));
    }

    private IEnumerator DelayedResetTable(float delay)
    {
        if (isResetting) yield break; // Exit if already resetting
        isResetting = true;

        yield return new WaitForSeconds(delay);

        ResetTable();

        isResetting = false; // Reset the flag after the table is reset
    }

    public void ResetTable()
    {
        if (isTableReset) return;  // Exit if already reset
        isTableReset = true;

        foreach (var seat in seatAssignments.Keys) {
            CustomerController customer = seatAssignments[seat];
            float moodMultiplier = customer.GetComponent<CustomerController>().FinalSatisfactionScore();
            float reward = GameSettings.baseRewardAmount * moodMultiplier * GameSettings.currentRewardMultiplier;

            // Add the reward for this customer directly to GameSettings
            SoundManager.Instance.PlaySound(10, false);
            GameSettings.AddMoney(reward);
        }

        // Notify the WaveManager that a customer has finished
        WaveManager waveManager = FindObjectOfType<WaveManager>();
        if (waveManager != null) {
            waveManager.OnCustomerServed();
        }

        // Proceed with table reset logic
        RemoveAllCustomersFromTable();
        ClearFoodAndDirtyDishes();

        orderedFoodItems.Clear();
        table.orderedFoodItems.Clear();

        orderUIPanel.ClearOrderBoxes();
        seatAssignments.Clear();

        CustomerManager customerManager = FindObjectOfType<CustomerManager>();
        if (customerManager != null) {
            customerManager.MarkTableAsAvailable(table);
            for (int i = 0; i < table.snapPointOccupied.Length; i++) {
                table.snapPointOccupied[i] = false;
            }
        }

        isTableReset = false;
    }


    private void ClearFoodAndDirtyDishes()
    {
        if (table.centerPoint.childCount > 0) {
            foreach (Transform child in table.centerPoint) {
                Destroy(child.gameObject);
            }
        }
    }
}
