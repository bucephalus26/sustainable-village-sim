using UnityEngine;

public interface IProfession
{
    void Work(); // Method to handle profession-specific work (e.g., farming, selling goods)
    string GetProfessionName(); // Returns the profession name
}
