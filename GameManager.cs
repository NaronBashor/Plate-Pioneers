using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI currentMoneyText;
    [SerializeField] private GameObject gameControlPanel;
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject howToPlayPanel;
    [SerializeField] private List<GameObject> cooksStands = new List<GameObject>();

    private PlayerControls controls;

    private void Awake()
    {
        controls = new PlayerControls();

        pauseMenuPanel.SetActive(true);
        howToPlayPanel.SetActive(true);
    }

    void Start()
    {
        SoundManager.Instance.StopSound(5);
        SoundManager.Instance.PlaySound(4, true);

        for (int i = 0; i < cooksStands.Count; i++) {
            cooksStands[i].SetActive(false);
        }

        for (int i = 0; i < cooksStands.Count && i < (GameSettings.foodUnlocked + 1); i++) {
            cooksStands[i].SetActive(true);
        }

        pauseMenuPanel.SetActive(false);
        howToPlayPanel.SetActive(false);

        gameControlPanel.SetActive(!GameSettings.gameControlsPanelShown);
        if (gameControlPanel.activeSelf) {
            Time.timeScale = 0f;
        }
    }

    private void OnEnable()
    {
        controls.Player.Enable();
        controls.Player.PauseMenu.performed += OnPauseMenu;
    }

    private void OnDisable()
    {
        controls.Player.Disable();
        controls.Player.PauseMenu.performed -= OnPauseMenu;
    }

    private void Update()
    {
        currentMoneyText.text = GameSettings.playerMoney.ToString();
    }

    public void OnPauseMenu(InputAction.CallbackContext context)
    {
        if (!pauseMenuPanel.activeSelf) {
            SoundManager.Instance.PlaySound(0, false);
            Time.timeScale = 0f;
            pauseMenuPanel.SetActive(true);
        }
    }

    private void ClosePauseMenu()
    {
        if (pauseMenuPanel != null) {
            if (pauseMenuPanel.activeSelf) {
                SoundManager.Instance.PlaySound(0, false);
                Time.timeScale = 1f;
                pauseMenuPanel.SetActive(false);
            }
        }
    }

    public void OnHowToPlayButtonPressed()
    {
        if (howToPlayPanel.activeSelf) {
            SoundManager.Instance.PlaySound(0, false);
            howToPlayPanel.SetActive(false);
        } else {
            SoundManager.Instance.PlaySound(0, false);
            howToPlayPanel.SetActive(true);
        }
    }

    public void OnClosePanelButtonPressed(InputAction.CallbackContext context)
    {
        if (howToPlayPanel.activeSelf) {
            // Close the HowToPlay panel
            howToPlayPanel.SetActive(false);

            // If it's the first time, set gameControlsPanelShown to true
            if (!GameSettings.gameControlsPanelShown) {
                SoundManager.Instance.PlaySound(0, false);
                GameSettings.gameControlsPanelShown = true;
            }

            // If the pause menu is also open, close it too
            if (pauseMenuPanel.activeSelf) {
                
            }
            Time.timeScale = 1f;

            return; // Exit the method after closing the HowToPlay panel
        }

        // If gameControlPanel is open, close it and resume the game
        if (gameControlPanel.activeSelf) {
            gameControlPanel.SetActive(false);
            Time.timeScale = 1f;
            GameSettings.gameControlsPanelShown = true;
            return; // Exit the method after closing the game control panel
        }

        // If only the pause menu is open, close it
        if (pauseMenuPanel.activeSelf) {
            ClosePauseMenu();
        }
    }

    public void OnClosePanelButtonPressed()
    {
        if (howToPlayPanel.activeSelf) {
            // Close the HowToPlay panel
            howToPlayPanel.SetActive(false);

            // If it's the first time, set gameControlsPanelShown to true
            if (!GameSettings.gameControlsPanelShown) {
                GameSettings.gameControlsPanelShown = true;
            }

            // If the pause menu is also open, close it too
            if (pauseMenuPanel.activeSelf) {
                ClosePauseMenu();
            }
            Time.timeScale = 1f;

            return; // Exit the method after closing the HowToPlay panel
        }

        // If gameControlPanel is open, close it and resume the game
        if (gameControlPanel.activeSelf) {
            gameControlPanel.SetActive(false);
            Time.timeScale = 1f;
            GameSettings.gameControlsPanelShown = true;
            return; // Exit the method after closing the game control panel
        }

        // If only the pause menu is open, close it
        if (pauseMenuPanel.activeSelf) {
            ClosePauseMenu();
        }
    }

    public void OnLoadHouseScene()
    {
        SceneManager.LoadScene("ShopUpgrades");
        SoundManager.Instance.PlaySound(0, false);
    }

    [ContextMenu("Load Default Settings")]
    public void LoadDefaultSettings()
    {
        GameSettings.ResetToDefault();
    }
}
