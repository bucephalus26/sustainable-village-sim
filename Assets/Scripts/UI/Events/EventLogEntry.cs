using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class EventLogEntry : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image leftIndicator;

    [Header("Type Colors")]
    [SerializeField] private Color normalTextColor = new(0.2f, 0.2f, 0.2f);
    [SerializeField] private Color warningTextColor = new(0.2f, 0.1f, 0f);
    [SerializeField] private Color errorTextColor = Color.white;

    [SerializeField] private Color normalBgColor = new(1f, 1f, 1f, 0f);
    [SerializeField] private Color warningBgColor = new(1f, 0.5f, 0f, 0.5f);
    [SerializeField] private Color errorBgColor = new(1f, 0.2f, 0.2f, 0.3f);

    [SerializeField] private Color indicatorWarningColor = new(1f, 0.5f, 0f, 1f);
    [SerializeField] private Color indicatorErrorColor = new(1f, 0.2f, 0.2f, 1f);

    // Property for filtering
    public string MessageText => messageText ? messageText.text : string.Empty;
    public string Category { get; private set; }
    public LogEntryType Type { get; private set; }
    public string TimeStamp { get; private set; }

    public void Initialize(string message, string category, LogEntryType type)
    {
        // Store data
        Category = category;
        Type = type;

        // Extract timestamp
        if (message.StartsWith("[") && message.Contains("]"))
        {
            int closingBracket = message.IndexOf("]");
            TimeStamp = message.Substring(1, closingBracket - 1);
        }

        // Set text
        if (messageText != null)
        {
            messageText.text = message;

            // Apply text color based on type
            messageText.color = Type switch
            {
                LogEntryType.Warning => warningTextColor,
                LogEntryType.Error => errorTextColor,
                _ => normalTextColor
            };
        }

        // Set background color
        if (backgroundImage != null)
        {
            backgroundImage.color = Type switch
            {
                LogEntryType.Warning => warningBgColor,
                LogEntryType.Error => errorBgColor,
                _ => normalBgColor
            };
        }

        // Configure left indicator
        if (leftIndicator != null)
        {
            leftIndicator.color = Type switch
            {
                LogEntryType.Warning => indicatorWarningColor,
                LogEntryType.Error => indicatorErrorColor,
                _ => Color.clear
            };
        }
    }

    // Interactive elements
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (backgroundImage != null)
        {
            // Store original color and darken slightly
            Color originalColor = backgroundImage.color;
            backgroundImage.color = new Color(
                originalColor.r * 0.9f,
                originalColor.g * 0.9f,
                originalColor.b * 0.9f,
                Mathf.Min(originalColor.a + 0.1f, 1f)
            );
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Reset to normal appearance
        if (backgroundImage != null)
        {
            backgroundImage.color = Type switch
            {
                LogEntryType.Warning => warningBgColor,
                LogEntryType.Error => errorBgColor,
                _ => normalBgColor
            };
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // On right-click, copy to clipboard
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            GUIUtility.systemCopyBuffer = MessageText;
            Debug.Log("Log entry copied to clipboard: " + MessageText);
        }
    }
}