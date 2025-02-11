using UnityEngine;

public class ProfessionManager : MonoBehaviour
{
    private IProfession profession;
    private Villager villager;

    public void Initialize(Villager villager)
    {
        this.villager = villager;
    }

    public void AssignProfession(ProfessionData professionData)
    {
        // Remove existing profession component if any
        if (profession != null)
        {
            Destroy((MonoBehaviour)profession);
        }

        // Create new profession
        profession = ProfessionFactory.CreateProfession(villager, professionData);
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = professionData.spriteColor;
        }
    }

    public void HandleWork(bool canWork)
    {
        if (profession == null) return;

        var currentProfession = (BaseProfession)profession;
        if (canWork)
        {
            currentProfession.StartWorking();
            profession.Work();
        }
        else
        {
            currentProfession.StopWorking();
        }
    }

    public ProfessionType GetProfessionType()
    {
        return profession.ProfessionType;
    }

}
