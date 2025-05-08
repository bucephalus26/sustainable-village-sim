using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Globalization;

public class GoalItemUI : MonoBehaviour
{
    [Header("References - Assign in Inspector")]
    [SerializeField] private Image leftBorder;
    [SerializeField] private Image badgeBackground;
    [SerializeField] private TextMeshProUGUI badgeText;
    [SerializeField] private TextMeshProUGUI goalDescriptionText;
    [SerializeField] private Image progressBarFill;
    [SerializeField] private TextMeshProUGUI percentageText;
    [SerializeField] private TextMeshProUGUI progressText;

    public void Setup(Goal goal, Color goalColor)
    {
        if (goal == null) return;

        if (leftBorder != null) leftBorder.color = goalColor;
        if (badgeBackground != null) badgeBackground.color = goalColor;
        if (progressBarFill != null) progressBarFill.color = goalColor;

        if (badgeText != null) badgeText.text = GetGoalTypeName(goal.type);
        if (goalDescriptionText != null) goalDescriptionText.text = goal.description;

        // Calculate and Set Progress
        float progressRatio = (goal.target > 0) ? Mathf.Clamp01(goal.progress / goal.target) : 0f;

        if (progressBarFill != null)
        {
            progressBarFill.fillAmount = progressRatio;
        }

        if (percentageText != null)
        {
            percentageText.text = $"{progressRatio * 100f:F0}% Complete";
        }

        if (progressText != null)
        {
            string currentProgressStr = FormatProgressValue(goal.progress, goal.type);
            string targetStr = FormatProgressValue(goal.target, goal.type);
            progressText.text = $"{currentProgressStr}/{targetStr}";

            if (goal.type == GoalType.SocialProminence)
            {
                progressText.text += " villagers";
            }
            else if (goal.type == GoalType.WorkMastery)
            {
                progressText.text += "%";
            }
        }
    }

    // format the goal type name for the badge
    private string GetGoalTypeName(GoalType type)
    {
        switch (type)
        {
            case GoalType.AccumulateWealth: return "Wealth";
            case GoalType.SocialProminence: return "Social";
            case GoalType.WorkMastery: return "Mastery";
            case GoalType.VillageContributor: return "Contributor";
            default:
                return System.Text.RegularExpressions.Regex.Replace(type.ToString(), "(\\B[A-Z])", " $1");
        }
    }

    // formats progress/target values
    private string FormatProgressValue(float value, GoalType type)
    {
        switch (type)
        {
            case GoalType.SocialProminence:
                return $"{Mathf.FloorToInt(value)}"; // Show whole numbers for people count
            default:
                return value.ToString("N1", CultureInfo.InvariantCulture);
        }
    }
}