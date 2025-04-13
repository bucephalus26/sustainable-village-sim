using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProfessionDisplayItem : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI professionNameText;
    [SerializeField] private TextMeshProUGUI countText;
    [SerializeField] private Image professionIcon;

    public ProfessionType ProfessionType { get; private set; }

    public void Initialize(ProfessionType type, ProfessionData data)
    {
        this.ProfessionType = type;

        string displayName = GetFormattedProfessionName(type);
        Color iconColor = Color.grey; // Default

        if (data != null)
        {
            if (data.icon != null)
            {
                iconColor = Color.white;
            }
            else
            {
                iconColor = data.spriteColor;
            }
        }
        else if (type == ProfessionType.Unemployed)
        {
            displayName = "Unemployed";
            iconColor = new Color(0.75f, 0.22f, 0.17f, 0.8f);
        }

        // Apply to UI
        if (professionNameText != null)
        {
            professionNameText.text = displayName;
        }

        if (professionIcon != null)
        {
            professionIcon.color = iconColor;  // Assign color
        }

        UpdateCount(0);
    }

    public void UpdateCount(int count)
    {
        if (countText != null)
        {
            countText.text = count.ToString();
        }
    }

    private string GetFormattedProfessionName(ProfessionType type)
    {
        string name = type.ToString();
        if (type == ProfessionType.Unemployed) return "Unemployed";
        if (name.EndsWith("f")) return name.Substring(0, name.Length - 1) + "ves";
        if (name.EndsWith("s") || name.EndsWith("sh") || name.EndsWith("ch") || name.EndsWith("x") || name.EndsWith("z")) return name + "es";
        if (name.EndsWith("y") && name.Length > 1 && !"aeiou".Contains(name[name.Length - 2].ToString().ToLower())) return name.Substring(0, name.Length - 1) + "ies";
        return name + "s";
    }

}
