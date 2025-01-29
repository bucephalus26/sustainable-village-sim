using UnityEngine;

public class Shopkeeper : MonoBehaviour, IProfession
{
    private float workInterval = 5f;
    private float timer = 0f;

    public void Work()
    {
        timer += Time.deltaTime;
        if (timer >= workInterval)
        {
            // Simulate wealth generation (e.g., 5 units every 5 seconds)
            Debug.Log($"{gameObject.name} generated 5 wealth.");
            timer = 0f;
        }
    }

    public string GetProfessionName()
    {
        return "ShopKeeper";
    }

}
