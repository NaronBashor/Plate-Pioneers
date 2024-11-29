using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TutorialController : MonoBehaviour
{
    PlayerController player;

    public List<GameObject> tutorialPanels = new List<GameObject>();

    private int index;
    private bool firstTime;
    private bool canAdvance = true; // This flag will control input

    PlayerControls controls;

    private void Awake()
    {
        controls = new PlayerControls();

        firstTime = GameSettings.firstTime;
        index = 0;

        foreach (GameObject go in tutorialPanels) {
            go.SetActive(false);
        }
    }

    private void OnEnable()
    {
        if (firstTime) {
            // Enable the action for accepting (A button)
            controls.Player.AcceptAdvance.performed += AdvancePanel;
            controls.Player.AcceptAdvance.Enable();
        } else {
            Time.timeScale = 1f;
        }
    }

    private void OnDisable()
    {
        controls.Player.AcceptAdvance.performed -= AdvancePanel;
        controls.Player.AcceptAdvance.Disable();
    }

    private void Start()
    {
        if (firstTime) {
            tutorialPanels[index].SetActive(true);
        }
    }

    private void Update()
    {
        if (firstTime) {
            player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
            player.CanMove = false;
        }
    }

    bool CheckIfAnyPanelActive()
    {
        // Iterate over the list of tutorial panels
        for (int i = 0; i < tutorialPanels.Count; i++) {
            // If any panel is active, return true immediately
            if (tutorialPanels[i].gameObject.activeInHierarchy) {
                return true;
            }
        }

        canAdvance = false;
        // If none are active, return false
        return false;
    }

    public void AdvancePanel(InputAction.CallbackContext ctx)
    {
        // Check if the script is active in the hierarchy and if any panel is active
        if (canAdvance && !player.CanMove && CheckIfAnyPanelActive() && gameObject.activeInHierarchy) {
            SoundManager.Instance.PlaySound(0, false);
            StartCoroutine(Delay());
        }
    }

    IEnumerator Delay()
    {
        canAdvance = false; // Lock advancing to prevent multiple inputs
        yield return new WaitForSeconds(0.75f); // 1 second delay between panel transitions

        foreach (GameObject go in tutorialPanels) {
            go.SetActive(false);
        }

        index++;
        if (index > tutorialPanels.Count - 1) {
            GameSettings.firstTime = false;
            firstTime = false;
            player.CanMove = true;
        } else {
            tutorialPanels[index].SetActive(true);
        }

        canAdvance = true; // Unlock advancing after the delay
    }
}
