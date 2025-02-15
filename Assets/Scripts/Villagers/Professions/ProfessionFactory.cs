using UnityEngine;

public class ProfessionFactory
{
    public static IProfession CreateProfession(Villager villager, ProfessionData professionData)
    {
        IProfession profession = null;

        switch (professionData.type)
        {
            case ProfessionType.Farmer:
                profession = villager.gameObject.AddComponent<Farmer>();
                break;
            case ProfessionType.Shopkeeper:
                profession = villager.gameObject.AddComponent<Shopkeeper>();
                break;
            case ProfessionType.Priest:
                profession = villager.gameObject.AddComponent<Priest>();
                break;
            default:
                Debug.LogWarning("Unknown Profession Type");
                break;
        }

        if (profession != null)
        {
            profession.Initialize(villager, professionData);
        }
        return profession;
    }
}