using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class SceneFader : MonoBehaviour
{
    [SerializeField] private GameObject loadingScreen; // Reference to your loading screen GameObject
    [SerializeField] private Slider loadingBar; // Optional: Reference to a loading bar (Slider) to show progress

    // Minimum fill value for the loading bar
    private float minFillValue = 0.115f;

    public PlayerController playerController;
    public GameObject confirmPanel;
    public Button yesButton; // Reference to the Yes button
    public Button noButton;  // Reference to the No button

    PlayerControls controls;

    private void Awake()
    {
        controls = new PlayerControls();
    }

    private void OnEnable()
    {
        // Enable the action for accepting (A button)
        controls.Player.AcceptAdvance.performed += OnSelectButton;
        controls.Player.AcceptAdvance.Enable();
    }

    private void OnDisable()
    {
        controls.Player.AcceptAdvance.performed -= OnSelectButton;
        controls.Player.AcceptAdvance.Disable();
    }

    public void OnClosePanel()
    {
        if (confirmPanel != null && confirmPanel.activeSelf) {
            confirmPanel.SetActive(false);
            playerController.CanMove = true;
        }
    }

    // Detect when the "A" button (or Enter on the keyboard) is pressed to select the button
    public void OnSelectButton(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && confirmPanel.activeSelf) {
            // Simulate pressing the currently selected button
            GameObject selectedButton = EventSystem.current.currentSelectedGameObject;
            if (selectedButton != null) {
                Button button = selectedButton.GetComponent<Button>();
                if (button != null) {
                    SoundManager.Instance.PlaySound(0, false);
                    button.onClick.Invoke(); // Invoke the click event on the selected button
                }
            }
        }
    }

    public void LoadNewScene(string sceneName)
    {
        // Activate the loading screen
        loadingScreen.SetActive(true);

        // Reset the loading bar to the minimum value (optional, for a smooth transition)
        loadingBar.value = 0f;

        // Start the coroutine to load the scene asynchronously and update the progress bar
        StartCoroutine(LoadSceneAsync(sceneName));
    }

    IEnumerator LoadSceneAsync(string sceneName)
    {
        // Start loading the scene asynchronously
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);

        // Prevent the scene from activating immediately (useful for more control over loading)
        asyncOperation.allowSceneActivation = false;

        // Update the loading bar based on the loading progress
        while (!asyncOperation.isDone) {
            // The progress goes from 0 to 0.9; the remaining 0.9 to 1 is the activation process.
            float progress = Mathf.Clamp01(asyncOperation.progress / 0.9f);

            // Map the progress value to the loading bar's range (minFillValue to 1f)
            loadingBar.value = Mathf.Lerp(minFillValue, 1f, progress);

            // If the loading process is almost complete (progress >= 0.9), allow the scene activation
            if (asyncOperation.progress >= 0.9f) {
                // Optionally smooth the loading bar to fill completely
                loadingBar.value = 1f;

                // Wait a brief moment before activating the new scene
                yield return new WaitForSeconds(0.5f);

                // Activate the scene
                asyncOperation.allowSceneActivation = true;
            }

            yield return null; // Continue the loop every frame
        }
    }

}
