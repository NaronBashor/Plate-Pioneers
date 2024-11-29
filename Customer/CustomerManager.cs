using System.Collections.Generic;
using UnityEngine;

public class CustomerManager : MonoBehaviour
{
    public GameObject playerPrefab;
    public Transform playerOneSpawnPosition;

    public GameObject customerPrefab;  // Prefab for the customer object
    public Transform spawnPosition1;   // Spawn position for line 1 (left)
    public Transform spawnPosition2;   // Spawn position for line 2 (right)
    public Vector3 leftDoorPosition; // The door position for the left line
    public Vector3 rightDoorPosition;// The door position for the right line

    public List<Table> inactiveTwoTops = new List<Table>();
    public List<Table> inactiveFourTops = new List<Table>();

    public List<Table> activeTwoTopTables; // List of 2-top tables (each with 2 seats)
    public List<Table> activeFourTopTables; // List of 4-top tables (each with 4 seats)

    private List<bool> twoTopTableAvailability = new List<bool>();
    private List<bool> fourTopTableAvailability = new List<bool>();

    public List<GameObject> tableParents;

    public int maxLineLength = 5;      // Max number of customers in each line
    private List<GameObject> line1 = new List<GameObject>(); // List for left line customers
    private List<GameObject> line2 = new List<GameObject>(); // List for right line customers

    public float lineSpacing = 2f;     // Spacing between customers in the line
    int twoTopIndex = 0; // Index to track the two-top tables
    int fourTopIndex = 0; // Index to track the four-top tables

    private void Awake()
    {
        if (GameObject.Find("Player") == null) {
            Instantiate(playerPrefab, playerOneSpawnPosition.position, Quaternion.identity);
        }
    }

    private void Start()
    {
        //GameSettings.LoadSettings();  // Load game settings at the start

        activeFourTopTables.Clear();
        activeTwoTopTables.Clear();

        foreach (GameObject table in tableParents) {
            table.SetActive(false);
        }
        for (int i = 0; i < GameSettings.tablesPurchased; i++) {
            tableParents[i].SetActive(true);
        }

        SetTableAvailabilityOnStart();
    }

    public void SetTableAvailabilityOnStart()
    {
        for (int i = 0; i < GameSettings.tablesPurchased; i++) {
            if (i % 2 == 0) { // Even index, add a two-top table
                if (twoTopIndex < inactiveTwoTops.Count) {
                    activeTwoTopTables.Add(inactiveTwoTops[twoTopIndex]);
                    twoTopTableAvailability.Add(false); // Mark this table as available
                    twoTopIndex++; // Increment the two-top index
                }
            } else { // Odd index, add a four-top table
                if (fourTopIndex < inactiveFourTops.Count) {
                    activeFourTopTables.Add(inactiveFourTops[fourTopIndex]);
                    fourTopTableAvailability.Add(false); // Mark this table as available
                    fourTopIndex++; // Increment the four-top index
                }
            }
        }
    }

    public void TrySeatCustomer(CustomerController customerController)
    {
        if (customerController.currentPhase == CustomerController.CustomerPhase.StandingInLine) {
            if (TwoTopTableAvailable() && line1.Count > 0 && line2.Count > 0) {
                SendCustomersToTwoTopTable();
            }

            if (FourTopTableAvailable() && line1.Count >= 2 && line2.Count >= 2) {
                SendCustomersToFourTopTable();
            }
        }
    }

    // Called by the WaveManager to start customer spawning during each wave
    public void SpawnCustomersInWave()
    {
        if (line1.Count < maxLineLength) {
            SpawnCustomerInLine(spawnPosition1, line1, leftDoorPosition); // Spawn to left door
        }

        if (line2.Count < maxLineLength) {
            SpawnCustomerInLine(spawnPosition2, line2, rightDoorPosition); // Spawn to right door
        }
    }

    void SpawnCustomerInLine(Transform spawnPosition, List<GameObject> line, Vector3 doorPosition)
    {
        // Instantiate a customer at the spawn position
        GameObject newCustomer = Instantiate(customerPrefab, spawnPosition.position, Quaternion.identity);

        // Set the target door position for the customer (either left or right)
        CustomerController customerController = newCustomer.GetComponent<CustomerController>();
        if (customerController != null) {
            // Ensure the target position is correctly set to the door position (left or right)
            customerController.targetPosition = doorPosition;
        }

        // Add the customer to the appropriate line
        line.Add(newCustomer);

        // Position the customer in the correct spot in the line
        PositionCustomerInLine(line, doorPosition);
    }

    void PositionCustomerInLine(List<GameObject> line, Vector3 doorPosition)
    {
        for (int i = 0; i < line.Count; i++) {
            CustomerController customerController = line[i].GetComponent<CustomerController>();

            if (customerController != null) {
                if (i == 0) {
                    // First customer moves directly to the line's start position (door)
                    customerController.frontCustomer = null; // No one is in front
                } else {
                    // Assign the customer ahead in the line
                    customerController.frontCustomer = line[i - 1];
                }

                // Dynamically calculate the target position based on the index in the line
                Vector3 targetPosition = new Vector3(
                    doorPosition.x,               // Use the door's X position
                    doorPosition.y - i * lineSpacing, // Adjust Y position based on spacing
                    doorPosition.z               // Use the door's Z position
                );

                // Assign the calculated position to the customer's target (without changing the door's position)
                customerController.targetPosition = targetPosition; // Assign as a Vector3, not Transform
            }
        }
    }

    public void SendCustomersToTwoTopTable()
    {
        int tableIndex = twoTopTableAvailability.IndexOf(false);
        if (tableIndex >= 0 && line1.Count > 0 && line2.Count > 0) {
            twoTopTableAvailability[tableIndex] = true;

            GameObject leftCustomer = line1[0];
            GameObject rightCustomer = line2[0];

            CustomerController leftController = leftCustomer.GetComponent<CustomerController>();
            CustomerController rightController = rightCustomer.GetComponent<CustomerController>();

            // Teleport customers to their seats
            leftController.SetSeated(activeTwoTopTables[tableIndex].seats[0].position);
            rightController.SetSeated(activeTwoTopTables[tableIndex].seats[1].position);

            // Get the TableInteraction component from the table
            TableInteraction tableInteraction = activeTwoTopTables[tableIndex].seats[0].GetComponentInParent<TableInteraction>();
            if (tableInteraction != null) {
                // Notify TableInteraction that customers are seated
                tableInteraction.AssignCustomerToSeat(leftController, activeTwoTopTables[tableIndex].seats[0]);
                tableInteraction.AssignCustomerToSeat(rightController, activeTwoTopTables[tableIndex].seats[1]);
            }

            // Check if left seat faces away and call the function if true
            if (activeTwoTopTables[tableIndex].facesAway[0]) {
                leftController.FaceAway(leftController);
            } else {
                leftController.GetComponent<SpriteRenderer>().sortingLayerName = "Walk Behind";
            }

            // Check if right seat faces away and call the function if true
            if (activeTwoTopTables[tableIndex].facesAway[1]) {
                rightController.FaceAway(rightController);
            } else {
                rightController.GetComponent<SpriteRenderer>().sortingLayerName = "Walk Behind";
            }

            // Remove customers from the lines
            line1.RemoveAt(0);
            line2.RemoveAt(0);
            PositionCustomerInLine(line1, leftDoorPosition);
            PositionCustomerInLine(line2, rightDoorPosition);

            // Move up the remaining customers in the line
            MoveUpLine(line1, leftDoorPosition);
            MoveUpLine(line2, rightDoorPosition);
        }
    }

    void MoveUpLine(List<GameObject> line, Vector3 doorPosition)
    {
        for (int i = 0; i < line.Count; i++) {
            CustomerController customerController = line[i].GetComponent<CustomerController>();

            if (i == 0) {
                customerController.frontCustomer = null;  // No one in front for the first customer
            } else {
                customerController.frontCustomer = line[i - 1];  // Set front customer
            }

            Vector3 targetPosition = new Vector3(
                doorPosition.x,
                doorPosition.y - i * lineSpacing,  // Adjust Y position based on the new index
                doorPosition.z
            );

            customerController.targetPosition = targetPosition;
        }
    }

    public void SendCustomersToFourTopTable()
    {
        int tableIndex = fourTopTableAvailability.IndexOf(false);
        if (tableIndex >= 0 && line1.Count >= 2 && line2.Count >= 2) {
            fourTopTableAvailability[tableIndex] = true;

            GameObject leftCustomer1 = line1[0];
            GameObject leftCustomer2 = line1[1];
            GameObject rightCustomer1 = line2[0];
            GameObject rightCustomer2 = line2[1];

            CustomerController leftController1 = leftCustomer1.GetComponent<CustomerController>();
            CustomerController leftController2 = leftCustomer2.GetComponent<CustomerController>();
            CustomerController rightController1 = rightCustomer1.GetComponent<CustomerController>();
            CustomerController rightController2 = rightCustomer2.GetComponent<CustomerController>();

            // Teleport customers to their seats
            leftController1.SetSeated(activeFourTopTables[tableIndex].seats[0].position);
            leftController2.SetSeated(activeFourTopTables[tableIndex].seats[1].position);
            rightController1.SetSeated(activeFourTopTables[tableIndex].seats[2].position);
            rightController2.SetSeated(activeFourTopTables[tableIndex].seats[3].position);

            // Get the TableInteraction component from the table
            TableInteraction tableInteraction = activeFourTopTables[tableIndex].seats[0].GetComponentInParent<TableInteraction>();
            if (tableInteraction != null) {
                // Notify TableInteraction that customers are seated
                tableInteraction.AssignCustomerToSeat(leftController1, activeFourTopTables[tableIndex].seats[0]);
                tableInteraction.AssignCustomerToSeat(leftController2, activeFourTopTables[tableIndex].seats[1]);
                tableInteraction.AssignCustomerToSeat(rightController1, activeFourTopTables[tableIndex].seats[2]);
                tableInteraction.AssignCustomerToSeat(rightController2, activeFourTopTables[tableIndex].seats[3]);
            }

            // Check if left seat faces away and call the function if true
            if (activeFourTopTables[tableIndex].facesAway[0]) {
                leftController1.FaceAway(leftController1);
            } else {
                leftController1.GetComponent<SpriteRenderer>().sortingLayerName = "Walk Behind";
                leftController1.GetComponent<SpriteRenderer>().sortingOrder = 1;
            }

            // Check if right seat faces away and call the function if true
            if (activeFourTopTables[tableIndex].facesAway[1]) {
                leftController2.FaceAway(leftController2);
            } else {
                leftController2.GetComponent<SpriteRenderer>().sortingLayerName = "Walk Behind";
                leftController2.GetComponent<SpriteRenderer>().sortingOrder = 2;
            }

            if (activeFourTopTables[tableIndex].facesAway[2]) {
                rightController1.FaceAway(rightController1);
            } else {
                rightController1.GetComponent<SpriteRenderer>().sortingLayerName = "Walk Behind";
                rightController1.GetComponent<SpriteRenderer>().sortingOrder = 4;
            }

            // Check if right seat faces away and call the function if true
            if (activeFourTopTables[tableIndex].facesAway[3]) {
                rightController2.FaceAway(rightController2);
            } else {
                rightController2.GetComponent<SpriteRenderer>().sortingLayerName = "Walk Behind";
                rightController2.GetComponent<SpriteRenderer>().sortingOrder = 5;
            }

            // Remove customers from the lines
            line1.RemoveAt(0); line1.RemoveAt(0);
            line2.RemoveAt(0); line2.RemoveAt(0);
            PositionCustomerInLine(line1, leftDoorPosition);
            PositionCustomerInLine(line2, rightDoorPosition);
        }
    }

    public void MarkTableAsAvailable(Table table)
    {
        // Check if it's a 2-top table
        int twoTopIndex = activeTwoTopTables.IndexOf(table);
        if (twoTopIndex != -1) {
            twoTopTableAvailability[twoTopIndex] = false; // Mark the table as available
        }

        // Check if it's a 4-top table
        int fourTopIndex = activeFourTopTables.IndexOf(table);
        if (fourTopIndex != -1) {
            fourTopTableAvailability[fourTopIndex] = false; // Mark the table as available
        }
    }

    public bool TwoTopTableAvailable()
    {
        return twoTopTableAvailability.Contains(false);
    }

    public bool FourTopTableAvailable()
    {
        return fourTopTableAvailability.Contains(false);
    }
}
