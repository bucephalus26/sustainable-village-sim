using UnityEngine;

public class Farmer : MonoBehaviour, IProfession 
{
    private float workInterval = 5f; // Time between work cycles (e.g., 5 seconds)
    private float timer = 0f;

    public void Work()
    {
        timer += Time.deltaTime;
        if (timer < workInterval)
        {
            // Farmer-specific duties
            Debug.Log($"{gameObject.name} is farming, generated 10 food.");
            timer = 0f;
        }
    }

    public string GetProfessionName()
    {
        return "Farmer";
    }

}
