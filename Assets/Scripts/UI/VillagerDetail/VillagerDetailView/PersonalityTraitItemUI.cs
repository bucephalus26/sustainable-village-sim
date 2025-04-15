using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PersonalityTraitItemUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI labelText;
    [SerializeField] private Image barFillImage;

    public void Setup(string label, float fillAmount01, Color barColor)
    {
        if (labelText != null) labelText.text = label;
        if (barFillImage != null)
        {
            barFillImage.fillAmount = Mathf.Clamp01(fillAmount01);
            barFillImage.color = barColor;
        }
    }
}