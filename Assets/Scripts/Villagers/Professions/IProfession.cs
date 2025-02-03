using UnityEngine;

public interface IProfession
{
    void Work(); // Method to handle profession-specific work (e.g., farming, selling goods)
    void Initialize(Villager villager, ProfessionData data);
    ProfessionType ProfessionType { get; }
}
