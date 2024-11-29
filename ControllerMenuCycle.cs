using UnityEngine;
using UnityEngine.InputSystem; // Include Input System namespace
using UnityEngine.UI;

public class ControllerMenuCycle : MonoBehaviour
{
    [SerializeField] private Button[] buttons; // Buttons array for selection
    private int buttonIndex;

    public bool isHorizontalMenu = false;
    private bool hide = false;

    // Reference to the InputAction for menu navigation
    private PlayerControls controls;

    private void Awake()
    {
        controls = new PlayerControls();
    }

    private void OnEnable()
    {
        // Enable the Menu Navigation input and bind it
        controls.Player.MenuNav.performed += OnMenuNav; // Up/Down Navigation
        controls.Player.MenuNav.Enable();

        controls.Player.MenuNav.performed += OnHorizontalMenuNav; // Left/Right Navigation
        controls.Player.MenuNav.Enable();
    }

    private void OnDisable()
    {
        // Disable the input when not in use
        controls.Player.MenuNav.Disable();
        controls.Player.MenuNav.performed -= OnMenuNav;

        controls.Player.MenuNav.Disable();
        controls.Player.MenuNav.performed -= OnHorizontalMenuNav;
        hide = false;
    }

    private void Start()
    {
        buttonIndex = 0;
        ChangeSelectedButton();
    }

    // Handle vertical (up/down) navigation
    private void OnMenuNav(InputAction.CallbackContext context)
    {
        // Disable vertical input if the Confirm New Game Panel is active
        if (GameObject.Find("Confirm New Game Panel") != null && GameObject.Find("Confirm New Game Panel").activeInHierarchy) {
            return; // Exit the method if the panel is active
        }

        if (isHorizontalMenu || hide) return; // Skip if it's a horizontal menu
        Vector2 input = context.ReadValue<Vector2>();

        if (input.y > 0) // D-Pad/Arrow Up
        {
            DecreaseButtonIndex();
        } else if (input.y < 0) // D-Pad/Arrow Down
          {
            IncreaseButtonIndex();
        }
    }

    // Handle horizontal (left/right) navigation
    private void OnHorizontalMenuNav(InputAction.CallbackContext context)
    {
        if (!isHorizontalMenu || hide) return; // Skip if it's a vertical menu
        Vector2 input = context.ReadValue<Vector2>();

        if (input.x > 0) // Right arrow or D-Pad right
        {
            IncreaseButtonIndex();
        } else if (input.x < 0) // Left arrow or D-Pad left
          {
            DecreaseButtonIndex();
        }
    }

    // Change the selected button
    private void ChangeSelectedButton()
    {
        buttons[buttonIndex].Select();
    }

    public void IncreaseButtonIndex()
    {
        buttonIndex++;
        if (buttonIndex > buttons.Length - 1) {
            buttonIndex = 0; // Wrap around to the first button
        }
        ChangeSelectedButton();
    }

    public void DecreaseButtonIndex()
    {
        buttonIndex--;
        if (buttonIndex < 0) {
            buttonIndex = buttons.Length - 1; // Wrap around to the last button
        }
        ChangeSelectedButton();
    }
}
