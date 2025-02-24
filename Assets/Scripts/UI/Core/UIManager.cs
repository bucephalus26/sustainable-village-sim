using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Centralised manager for all UI panels
/// </summary>
public class UIManager : MonoBehaviour
{
    private static UIManager instance;
    public static UIManager Instance => instance;

    [Header("References")]
    [SerializeField] private List<UIPanel> panels = new();
    private Dictionary<string, UIPanel> panelLookup = new();

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        // register all panels
        foreach (var panel in panels)
        {
            if (panel != null)
            {
                panelLookup[panel.PanelID] = panel;
                panel.Initialize();
            }
        }
    }

    public T GetPanel<T>(string panelID) where T: UIPanel
    {
        if (panelLookup.TryGetValue(panelID, out UIPanel panel))
        {
            return panel as T;
        }
        return null;
    }

    public void ShowPanel(string panelID)
    {
        if (panelLookup.TryGetValue(panelID, out UIPanel panel))
        {
            panel.Show();
        }
        else
        {
            Debug.LogWarning($"Panel with ID '{panelID}' not found");
        }
    }

    public void HidePanel(string panelID)
    {
        if (panelLookup.TryGetValue(panelID, out UIPanel panel))
        {
            panel.Hide();
        }
        else
        {
            Debug.LogWarning($"Panel with ID '{panelID}' not found");
        }
    }

    public void TogglePanel(string panelID)
    {
        if (panelLookup.TryGetValue(panelID, out UIPanel panel))
        {
            panel.Toggle();
        }
        else
        {
            Debug.LogWarning($"Panel with ID '{panelID}' not found");
        }
    }

}
