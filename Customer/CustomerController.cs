using System.Collections.Generic;
using UnityEngine;

public class CustomerController : MonoBehaviour
{
    public enum CustomerPhase
    {
        MovingTowardDoor,
        StandingInLine,
        Seated,
        CallingWaiter,
        WaitingForFood,
        Eating,
        BusTable
    }

    public enum Mood
    {
        Happy,
        Bored,
        Angry
    }

    [SerializeField] private Animator defaultController;
    [SerializeField] private List<AnimatorOverrideController> animControllers = new List<AnimatorOverrideController>();

    public Food[] allFoods;
    public Food[] availableFoods;

    public CustomerPhase currentPhase = CustomerPhase.MovingTowardDoor;
    public Mood currentMood = Mood.Happy;

    private Animator animator;
    private float standingInLineTime;
    private float waitingForFoodTime;

    public Vector3 targetPosition; // The position to which the customer moves
    public GameObject frontCustomer; // The customer in front of this one
    public float satisfactionScore;

    public TableInteraction currentTable; // Reference to the table this customer is seated at

    private bool hasModifiedScoreForMood = false;

    // The food item that the customer orders
    public Food orderedFood;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        animator.SetBool("StandInLine", true);

        // Initialize availableFoods array with the number of unlocked food items
        availableFoods = new Food[GameSettings.foodUnlocked + 1];
    }

    void Start()
    {
        // Loop through to add the unlocked foods to availableFoods
        for (int i = 0; i < (GameSettings.foodUnlocked + 1) && i < allFoods.Length; i++) {
            availableFoods[i] = allFoods[i];
        }

        satisfactionScore = 1.8f;
        standingInLineTime = 0f;
        waitingForFoodTime = 0f;

        // Randomize the animation controller
        int randomNumber = Random.Range(0, animControllers.Count);
        if (randomNumber > 0) {
            animator.runtimeAnimatorController = animControllers[randomNumber];
        }
    }

    void Update()
    {
        if (currentPhase == CustomerPhase.MovingTowardDoor || currentPhase == CustomerPhase.StandingInLine) {
            MoveToPosition();
        }

        if (currentPhase == CustomerPhase.StandingInLine) {
            UpdateMoodForStandingInLine();
            // The customer is waiting for an available table. CustomerManager will handle seating.
            CustomerManager customerManager = FindObjectOfType<CustomerManager>();
            if (customerManager != null) {
                customerManager.TrySeatCustomer(this);
            }
        }

        // Handle the Seated timer
        if (currentPhase == CustomerPhase.Seated) {
            GameSettings.lookAtMenuTime += Time.deltaTime;

            // After 10 seconds, transition to ReadyToOrder
            if (GameSettings.lookAtMenuTime >= 10f) {
                currentPhase = CustomerPhase.CallingWaiter;
                animator.SetBool("CallingWaiter", true);
            }
        }

        if (currentPhase == CustomerPhase.WaitingForFood) {
            UpdateMoodForWaitingForFood();
        }

        if (currentPhase == CustomerPhase.Eating) {
            UpdateEatingTimer();
        }
    }

    private void UpdateEatingTimer()
    {
        if (GameSettings.eatingTime > 0) {
            GameSettings.eatingTime -= Time.deltaTime;
        } else {
            SetBussingTable();
        }
    }

    public void SetEating()
    {
        currentPhase = CustomerPhase.Eating;
        GameSettings.eatingTime = 10f;

        // Check if the ordered food is a drink and trigger the appropriate animation
        if (orderedFood != null && orderedFood.isDrink) {
            // If it's a drink, trigger the drinking animation
            animator.SetBool("Drinking", true);
            animator.SetBool("FoodArrived", true);
        } else {
            // If it's food, trigger the eating animation
            animator.SetBool("FoodArrived", true);
            animator.SetBool("Drinking", false);
        }
    }

    private void SetBussingTable()
    {
        currentTable.BusTable();
        currentPhase = CustomerPhase.BusTable;

        // Stop both eating and drinking animations
        animator.SetBool("DoneEating", true);
        animator.SetBool("Drinking", false);
        animator.SetBool("FoodArrived", false);
    }

    public void FaceAway(CustomerController controller)
    {
        animator.SetBool("backView", true);
        GetComponent<SpriteRenderer>().sortingLayerName = "Walk In Front";
        GetComponent<SpriteRenderer>().sortingOrder = 4;
    }

    void MoveToPosition()
    {
        if (frontCustomer != null) {
            float distanceToFrontCustomer = Vector3.Distance(transform.position, frontCustomer.transform.position);
            if (distanceToFrontCustomer <= GameSettings.customerStopDistance) {
                return;
            }
        }

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, GameSettings.customerMoveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPosition) < 0.1f) {
            currentPhase = CustomerPhase.StandingInLine;
        }
    }

    void UpdateMoodForStandingInLine()
    {
        standingInLineTime += Time.deltaTime;

        if (standingInLineTime >= GameSettings.standingInLineAngryTime) {
            SetMood(Mood.Angry);
        } else if (standingInLineTime >= GameSettings.standingInLineBoredTime) {
            SetMood(Mood.Bored);
        }
    }

    void UpdateMoodForWaitingForFood()
    {
        waitingForFoodTime += Time.deltaTime;

        if (waitingForFoodTime >= GameSettings.waitingForFoodAngryTime) {
            SetMood(Mood.Angry);
        } else if (waitingForFoodTime >= GameSettings.waitingForFoodBoredTime) {
            SetMood(Mood.Bored);
        }
    }

    void SetMood(Mood newMood)
    {
        if (currentMood != newMood) {
            currentMood = newMood;
            hasModifiedScoreForMood = false; // Reset the flag so we can modify the score for the new mood

            switch (newMood) {
                case Mood.Bored:
                    animator.SetBool("Bored", true);
                    break;

                case Mood.Angry:
                    animator.SetBool("Angry", true);
                    break;
            }
        }

        // Apply satisfaction score modification only once when the mood changes
        if (!hasModifiedScoreForMood) {
            if (currentMood == Mood.Bored || currentMood == Mood.Angry) {
                ModifySatisfactionScore(0.2f);
                hasModifiedScoreForMood = true; // Prevent further score modifications for the same mood
            }
        }
    }

    public void SetSeated(Vector3 tableSeatPosition)
    {
        transform.position = tableSeatPosition;
        currentPhase = CustomerPhase.Seated;
        animator.SetBool("StandInLine", false);
    }

    public void SetWaitingForFood()
    {
        currentPhase = CustomerPhase.WaitingForFood;
        animator.SetBool("CallingWaiter", false);
        animator.SetBool("WaitForFood", true);
    }

    public void AssignRandomOrder()
    {
        orderedFood = availableFoods[Random.Range(0, availableFoods.Length)];
    }

    private void ModifySatisfactionScore(float amount)
    {
        satisfactionScore -= amount;
    }

    public float FinalSatisfactionScore()
    {
        return satisfactionScore;
    }
}
