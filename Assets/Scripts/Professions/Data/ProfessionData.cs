using UnityEngine;

[CreateAssetMenu(fileName = "ProfessionData", menuName = "Village/ProfessionData")]
public class ProfessionData : ScriptableObject
{
    [Header("Basic Info")]
    public string professionName;
    public ProfessionType type;
    public string description;

    [Header("Work Settings")]
    public float workInterval = 3f; // How many game hours between work outputs
    public ResourceType primaryResourceType = ResourceType.None;
    public float resourceOutput = 2f;
    public float wealthGeneration = 1f;

    [Header("Schedule")]
    public TimeOfDay[] workingHours = { TimeOfDay.Morning, TimeOfDay.Afternoon };
    public TimeOfDay[] socialHours = { TimeOfDay.Evening };
    public TimeOfDay[] restingHours = { TimeOfDay.Night };

    [Header("Visual Settings")]
    public Color spriteColor = Color.white;
    public Sprite icon;
}