using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Base class for all UI panels
/// </summary>
public abstract class UIPanel : MonoBehaviour
{
    [SerializeField] protected string panelID;
    [SerializeField] protected Button closeButton;

    [SerializeField] protected bool showOnStart = false;
    [SerializeField] protected bool resetOnHide = true;

    public string PanelID => panelID;

    protected virtual void Start()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(Hide);
        } 

        if (!showOnStart)
        {
            gameObject.SetActive(false);
        }
    }

    public virtual void Initialize()
    {
        // Override in subclasses for specific initialisation
    }

    public virtual void Show()
    {
        gameObject.SetActive(true);
        OnShow();
    }

    public virtual void Hide()
    {
        gameObject.SetActive(false);
        if (resetOnHide)
        {
            Reset();
        }
    }

    public virtual void Toggle()
    {
        if (gameObject.activeSelf)
        {
            Hide();
        }
        else
        {
            Show();
        }
    }

    protected virtual void OnShow()
    {
        // Override in subclasses for actions when panel is shown
    }

    protected virtual void Reset()
    {
        // Override in subclasses to reset panel state
    }

    protected virtual void OnDestroy()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(Hide);
        }
    }
}
