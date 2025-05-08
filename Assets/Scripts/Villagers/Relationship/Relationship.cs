using UnityEngine;

[System.Serializable]
public class Relationship
{
    public Villager TargetVillager { get; private set; }
    public RelationshipType Type { get; set; } 

    public float Strength { get; private set; }

    public float LastInteractionTime { get; private set; } // Game time of last significant interaction

    // Constructor
    public Relationship(Villager target, RelationshipType initialType = RelationshipType.Acquaintance, float initialStrength = 5f)
    {
        if (target == null)
        {
            Debug.LogError("Cannot create relationship with a null target Villager.");
            return;
        }
        TargetVillager = target;
        Type = initialType;
        Strength = Mathf.Clamp(initialStrength, -100f, 100f); 
        LastInteractionTime = Time.time; 
    }

    public void ModifyStrength(float amount)
    {
        Strength = Mathf.Clamp(Strength + amount, -100f, 100f);
        UpdateLastInteractionTime();
        Debug.Log($"Relationship strength with {TargetVillager.villagerName} changed by {amount:F1}. New strength: {Strength:F1}");
    }

    public void UpdateLastInteractionTime()
    {
        LastInteractionTime = Time.time;
    }

    public bool IsFamily() => Type == RelationshipType.Family_Parent || Type == RelationshipType.Family_Child || Type == RelationshipType.Family_Sibling || Type == RelationshipType.Spouse;
    public bool IsPositive() => Strength > 10f;
    public bool IsNegative() => Strength < -10f;
}