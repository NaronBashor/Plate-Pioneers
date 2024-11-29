using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject confirmNewGamePanel;
    [SerializeField] private Button continueButton;
    [SerializeField] private GameObject loadingScreen; // Reference to your loading screen GameObject
    [SerializeField] private Slider loadingBar; // Optional: Reference to a loading bar (Slider) to show progress

    private string myWebsiteURL = "https://www.splitrockgames.com";

    // Minimum fill value for the loading bar
    private float minFillValue = 0.115f;

    private PlayerControls playerControls;

    private void Awake()
    {
        playerControls = new PlayerControls();
        //GameSettings.ResetToDefault();
    }

    private void Start()
    {
        SoundManager.Instance.PlaySound(6, true);

        continueButton.interactable = GameSettings.IsSaveDataAvailable();

        if (confirmNewGamePanel == null) {
            confirmNewGamePanel = GameObject.Find("ConfirmNewGamePanel");
            if (confirmNewGamePanel == null) {
                Debug.LogError("ConfirmNewGamePanel GameObject could not be found in the scene.");
                return;
            }
        }

        confirmNewGamePanel.SetActive(false);
        loadingScreen.SetActive(false); // Ensure the loading screen is inactive initially
        playerControls = new PlayerControls();
    }


    private void OnEnable()
    {
        playerControls.Player.AcceptAdvance.performed += OnSelectButton;
        playerControls.Player.MenuClose.Enable();
    }


    private void OnDisable()
    {
        playerControls.Player.MenuClose.Disable();
        playerControls.Player.AcceptAdvance.performed -= OnSelectButton;
    }

    // Detect when the "A" button (or Enter on the keyboard) is pressed to select the button
    public void OnSelectButton(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && confirmNewGamePanel.activeSelf) {
            // Simulate pressing the currently selected button
            GameObject selectedButton = EventSystem.current.currentSelectedGameObject;
            if (selectedButton != null) {
                Button button = selectedButton.GetComponent<Button>();
                if (button != null) {
                    button.onClick.Invoke(); // Invoke the click event on the selected button
                }
            }
        }
    }

    // This method is assigned to the button's OnClick event
    public void OpenURL()
    {
        Application.OpenURL(myWebsiteURL);
    }

    public void OnOpenConfirmPanel()
    {
        confirmNewGamePanel.SetActive(true);
    }

    public void CloseConfirmPanel()
    {
        if (confirmNewGamePanel != null) {
            confirmNewGamePanel.SetActive(false);
        }
    }

    public void OnNewGameButtonPressed()
    {
        GameSettings.ResetToDefault();
        LoadNewScene("ShopUpgrades"); // Replace "Game" with your game scene name
    }

    public void OnContinueButtonPressed()
    {
        if (GameSettings.IsSaveDataAvailable()) {
            GameSettings.LoadSettings();
            LoadNewScene("ShopUpgrades"); // Replace "Game" with your game scene name
        } else {
            //Debug.Log("No save data found.");
            // Optionally handle the case where no save data exists.
        }
    }

    public void LoadNewScene(string sceneName)
    {
        // Activate the loading screen
        loadingScreen.SetActive(true);

        // Start the coroutine to load the scene asynchronously and update the progress bar
        StartCoroutine(LoadSceneAsync(sceneName));
    }

    IEnumerator LoadSceneAsync(string sceneName)
    {
        // Start loading the scene asynchronously
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);

        // Prevent the scene from activating immediately (useful for more control over loading)
        asyncOperation.allowSceneActivation = false;

        // Simulate the loading bar progress
        float fakeProgress = 0f;

        // Update the loading bar based on the loading progress
        while (!asyncOperation.isDone) {
            // The progress from 0 to 0.9 indicates the loading process; 0.9 to 1 is the activation process.
            float actualProgress = Mathf.Clamp01(asyncOperation.progress / 0.9f);

            // Map the actualProgress from range [0, 1] to [minFillValue, 1]
            float mappedProgress = Mathf.Lerp(minFillValue, 1f, actualProgress);

            // Simulate a smooth progress if the scene loads too fast
            while (fakeProgress < mappedProgress) {
                fakeProgress += Time.deltaTime * 0.5f; // Adjust the speed of the loading bar
                loadingBar.value = Mathf.Clamp(fakeProgress, minFillValue, 1f); // Ensure minFillValue is respected
                yield return null;
            }

            // Allow the scene to activate when the progress reaches near 1.0
            if (asyncOperation.progress >= 0.9f && fakeProgress >= 0.99f) {
                // Smoothly finish the loading
                loadingBar.value = 1f;
                yield return new WaitForSeconds(0.5f); // Delay before scene activation
                asyncOperation.allowSceneActivation = true; // Activate the scene
            }

            yield return null;
        }
    }
}
