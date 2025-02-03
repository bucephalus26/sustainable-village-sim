using UnityEngine;

[CreateAssetMenu(fileName = "ProfessionData", menuName = "Villager/ProfessionData")]
public class ProfessionData : ScriptableObject
{
    public ProfessionType type;
    public string professionName;

    [Header("Work Settings")]
    public float workInterval;
    public float resourceOutput;
    public float wealthGeneration;

    [Header("Visual Settings")]
    public Color spriteColor = Color.white;
}
