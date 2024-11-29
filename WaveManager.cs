using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WaveManager : MonoBehaviour
{
    [System.Serializable]
    public class Wave
    {
        public int customerCount; // Number of customers in this wave (day)
        public float spawnInterval; // Time between each customer spawn
    }

    [SerializeField] private GameObject loadingScreen; // Reference to your loading screen GameObject
    [SerializeField] private Slider loadingBar; // Optional: Reference to a loading bar (Slider) to show progress

    //private float minFillValue = 0.115f;

    public Wave[] waves; // Array of waves (days)
    public CustomerManager customerManager; // Reference to CustomerManager
    public GameObject dayOverPanel; // Panel that shows at the end of the day
    public TextMeshProUGUI dayText; // UI text to display the current day
    public TextMeshProUGUI customerTotalText; // UI text to display the total customers served for the day
    public TextMeshProUGUI moneyEarnedText; // UI text to display the total money earned for the day

    private int currentWaveIndex = 0;
    private int customersSpawnedForDay = 0;
    private int customersServedForDay = 0;
    private float startOfDayMoney;
    private float endOfDayMoney;
    private float moneyEarnedThisRound;

    private bool isSceneLoading = false; // Add a flag to track scene load status

    PlayerControls controls;

    private void Awake()
    {
        controls = new PlayerControls();
    }

    private void OnEnable()
    {
        if (gameObject.activeInHierarchy) {
            controls.Player.AcceptAdvance.performed += OnSelectButton;
            controls.Player.AcceptAdvance.Enable();
        }
    }

    private void OnDisable()
    {
        controls.Player.AcceptAdvance.performed -= OnSelectButton;
        controls.Player.AcceptAdvance.Disable();
    }

    private void Start()
    {
        SoundManager.Instance.PlaySound(1, true);

        if (dayOverPanel != null) {
            dayOverPanel.SetActive(false);
        }

        startOfDayMoney = GameSettings.playerMoney;

        // Procedurally generate waves
        GenerateWaves();

        StartWave(currentWaveIndex);
    }

    // Generate waves based on the number of tables and recipes unlocked
    private void GenerateWaves()
    {
        int totalWaves = 20; // Example: generate 10 waves
        waves = new Wave[totalWaves];

        for (int i = 0; i < totalWaves; i++) {
            int customerCount = CalculateCustomerCount(i); // Calculate customer count based on tables and wave
            float spawnInterval = CalculateSpawnInterval(i); // Adjust spawn interval based on recipes and wave

            waves[i] = new Wave {
                customerCount = customerCount,
                spawnInterval = spawnInterval
            };
        }
    }

    // Calculate customer count based on the number of tables unlocked and the current wave
    private int CalculateCustomerCount(int waveIndex)
    {
        int baseCustomers = 1; // Base number of customers per wave
        int tablesUnlocked = GameSettings.tablesPurchased; // Number of tables unlocked
        int waveModifier = waveIndex + 1; // Modifier based on the wave number

        return baseCustomers + (tablesUnlocked * waveModifier); // Scale customers based on tables and wave
    }

    // Calculate spawn interval based on the number of recipes unlocked and the current wave
    private float CalculateSpawnInterval(int waveIndex)
    {
        float baseInterval = 5f; // Base spawn interval
        int recipesUnlocked = GameSettings.foodUnlocked; // Number of recipes unlocked
        float waveModifier = (waveIndex + 1) * 0.1f; // Modifier based on the wave number

        // Reduce spawn interval as more recipes are unlocked, but keep it within a minimum range
        return Mathf.Max(1.5f, baseInterval - (recipesUnlocked * 0.1f) - waveModifier); // Never lower than 1.5 seconds
    }

    public void StartWave(int waveIndex)
    {
        customersSpawnedForDay = 0;
        customersServedForDay = 0;
        startOfDayMoney = GameSettings.playerMoney;
        StartCoroutine(RunWave(waveIndex));
    }

    IEnumerator RunWave(int waveIndex)
    {
        Wave currentWave = waves[waveIndex];

        if (dayText != null) {
            dayText.text = "Day " + (waveIndex + 1);
        }

        while (customersSpawnedForDay < currentWave.customerCount) {
            customerManager.SpawnCustomersInWave();
            customersSpawnedForDay++;
            yield return new WaitForSeconds(currentWave.spawnInterval);
        }
    }

    public void OnCustomerServed()
    {
        customersServedForDay++;

        if (customersServedForDay >= waves[currentWaveIndex].customerCount) {
            EndOfDay();
        }
    }

    private void EndOfDay()
    {
        endOfDayMoney = GameSettings.playerMoney;
        moneyEarnedThisRound = endOfDayMoney - startOfDayMoney;

        moneyEarnedText.text = "$" + moneyEarnedThisRound.ToString("F0");
        customerTotalText.text = customersServedForDay.ToString();

        Time.timeScale = 0f;

        if (dayOverPanel != null) {
            dayOverPanel.SetActive(true);
            GameSettings.ranMiniGameToday = false;
        }
    }

    public void StartNextDay()
    {
        if (dayOverPanel != null) {
            dayOverPanel.SetActive(false);
        }
        Time.timeScale = 1f;

        NextWave();
    }

    private void NextWave()
    {
        if (currentWaveIndex < waves.Length - 1) {
            currentWaveIndex++;
            StartWave(currentWaveIndex);
        } else {
            Debug.Log("All days completed!");
        }
    }

    void DisablePlayerInput()
    {
        controls.Player.Disable(); // Disable player input during loading
    }

    void EnablePlayerInput()
    {
        controls.Player.Enable(); // Re-enable player input after loading
    }

    public void OnSelectButton(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && dayOverPanel.activeSelf && !isSceneLoading && gameObject.activeInHierarchy) {
            GameObject selectedButton = EventSystem.current.currentSelectedGameObject;
            if (selectedButton != null) {
                Button button = selectedButton.GetComponent<Button>();
                if (button != null) {
                    SoundManager.Instance.PlaySound(0, false);
                    button.onClick.Invoke();
                    isSceneLoading = true; // Set the flag to true to avoid multiple loads
                }
            }
        }
    }

    public void LoadNewScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);

        //if (!isSceneLoading) {
        //    isSceneLoading = true; // Prevent loading the scene multiple times
        //    loadingScreen.SetActive(true);
        //    DisablePlayerInput(); // Disable input during loading
        //    StartCoroutine(LoadSceneAsync(sceneName));
        //}
    }

    //IEnumerator LoadSceneAsync(string sceneName)
    //{
    //    Debug.Log("Starting scene loading: " + sceneName);
    //    AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
    //    asyncOperation.allowSceneActivation = false;

    //    float fakeProgress = 0f;
    //    while (!asyncOperation.isDone) {
    //        float actualProgress = Mathf.Clamp01(asyncOperation.progress / 0.9f);
    //        Debug.Log("Scene loading progress: " + actualProgress);

    //        float mappedProgress = Mathf.Lerp(minFillValue, 1f, actualProgress);

    //        while (fakeProgress < mappedProgress) {
    //            fakeProgress += Time.deltaTime * 0.5f;
    //            loadingBar.value = Mathf.Clamp(fakeProgress, minFillValue, 1f);
    //            yield return null;
    //        }

    //        // Check if loading is complete and ready for activation
    //        if (asyncOperation.progress >= 0.9f && fakeProgress >= 0.99f) {
    //            Debug.Log("Scene loading complete. Activating scene.");
    //            loadingBar.value = 1f;
    //            yield return new WaitForSeconds(0.5f);
    //            asyncOperation.allowSceneActivation = true;
    //        }

    //        yield return null;
    //    }

    //    EnablePlayerInput(); // Re-enable input after the scene is fully loaded
    //    isSceneLoading = false; // Reset the flag when done
    //    Debug.Log("Scene activated: " + sceneName);
    //}


}
