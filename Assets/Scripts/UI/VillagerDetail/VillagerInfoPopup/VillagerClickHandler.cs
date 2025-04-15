using UnityEngine;

public class VillagerClickHandler : MonoBehaviour
{
    private Villager villager;

    void Awake()
    {
        villager = GetComponent<Villager>();
    }

    private void OnMouseDown()
    {
        Debug.Log($"Clicked on: {villager.villagerName}");
        UIManager.Instance.ShowVillagerPopup(villager);

    }

}
