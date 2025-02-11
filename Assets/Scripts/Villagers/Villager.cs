using UnityEngine;

public class Villager : MonoBehaviour
{
    [Header("Basic Attributes")]
    public string villagerName;
    public int age;
    public float health;
    public float personalWealth = 0f;

    [Header("Profession Settings")]
    [SerializeField] private ProfessionData professionData;

    private VillagerContext stateManager;

    void Start()
    {
        stateManager = gameObject.AddComponent<VillagerContext>();
        stateManager.Initialize(professionData);
    }

    public void EarnWealth(float amount)
    {
        personalWealth += amount;
    }
}