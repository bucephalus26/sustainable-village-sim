using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class TimeControlPanel : MonoBehaviour
{
    [Header("Time Display")]
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI dayText;
    [SerializeField] private TextMeshProUGUI timeOfDayText;

    [Header("Controls")]
    [SerializeField] private Button playPauseButton;
    [SerializeField] private Image playPauseIcon;
    [SerializeField] private Sprite playSprite;
    [SerializeField] private Sprite pauseSprite;
    [SerializeField] private List<SpeedButton> speedButtons = new();

    // Reference to managers
    private TimeManager timeManager;
    private GameManager gameManager;

    // Keep track of current speed setting
    private float currentSpeed = 1f;
    private bool isPaused = false;

    [System.Serializable]
    public class SpeedButton
    {
        public Button button;
        public float speedValue = 1f;
        public TextMeshProUGUI buttonText;
    }

    private void Start()
    {
        // Get references
        timeManager = TimeManager.Instance;
        gameManager = GameManager.Instance;

        // Initialise UI elements
        if (timeManager != null)
        {
            UpdateTimeDisplay();
        }

        // Set up button event listeners
        if (playPauseButton != null)
        {
            playPauseButton.onClick.AddListener(TogglePause);
        }

        // Set up speed buttons
        foreach (var speedButton in speedButtons)
        {
            if (speedButton.button != null)
            {
                speedButton.button.onClick.AddListener(() => SetSpeed(speedButton.speedValue));
            }
        }

        // Update UI to match initial state
        UpdateControlsDisplay();
    }

    public void UpdateTimeDisplay()
    {
        if (timeManager == null) return;

        // Update time text
        if (timeText != null)
        {
            timeText.text = timeManager.GetFormattedTime();
        }

        // Update day text
        if (dayText != null)
        {
            dayText.text = $"Day {timeManager.CurrentDay}";
        }

        // Update time of day text
        if (timeOfDayText != null)
        {
            timeOfDayText.text = timeManager.GetTimeOfDayName();
        }
    }

    private void UpdateControlsDisplay()
    {
        // Update play/pause button
        if (playPauseIcon != null)
        {
            playPauseIcon.sprite = isPaused ? playSprite : pauseSprite;
        }

        // Update speed buttons
        foreach (var speedButton in speedButtons)
        {
            if (speedButton.button != null)
            {
                bool isActive = !isPaused && Mathf.Approximately(currentSpeed, speedButton.speedValue);
                // Visual indication of active speed
                speedButton.button.GetComponent<Image>().color = isActive ?
                    new Color(0.2f, 0.6f, 0.9f, 1f) :
                    new Color(0.2f, 0.6f, 0.9f, 0.7f);
            }
        }
    }

    public void TogglePause()
    {
        if (gameManager == null) return;

        isPaused = !isPaused;
        gameManager.TogglePause();
        UpdateControlsDisplay();
    }

    public void SetSpeed(float speed)
    {
        if (gameManager == null) return;

        currentSpeed = speed;

        // If currently paused, also unpause
        if (isPaused)
        {
            isPaused = false;
            gameManager.TogglePause();
        }

        // Set the simulation speed
        if (speed < 1f)
        {
            gameManager.DecreaseSimulationSpeed();
        }
        else
        {
            // First reset to 1x
            while (gameManager.GetSimulationSpeed() > 1f)
            {
                gameManager.DecreaseSimulationSpeed();
            }

            // Then increase to desired speed
            while (gameManager.GetSimulationSpeed() < speed)
            {
                gameManager.IncreaseSimulationSpeed();
            }
        }

        UpdateControlsDisplay();
    }

    private void Update()
    {
        // Update time display every frame for smooth time transition
        if (timeManager != null)
        {
            UpdateTimeDisplay();
        }
    }
}