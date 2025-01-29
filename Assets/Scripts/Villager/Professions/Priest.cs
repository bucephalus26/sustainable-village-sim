using UnityEngine;

public class Priest : MonoBehaviour, IProfession
{

    public void Work()
    {
        // Simulate increasing village happiness
        Debug.Log($"{gameObject.name} is leading a prayer.");
    }

    public string GetProfessionName()
    {
        return "Priest";
    }

}
