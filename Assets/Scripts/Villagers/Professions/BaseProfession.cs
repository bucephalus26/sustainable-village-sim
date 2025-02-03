using UnityEngine;

public abstract class BaseProfession : MonoBehaviour, IProfession
{
    protected ProfessionData data;
    protected Villager villager;
    protected ResourceManager resources;

    private float workTimer;
    protected bool isWorking;

    public ProfessionType ProfessionType => data.type;

    public virtual void Initialize(Villager villager, ProfessionData data)
    {
        this.villager = villager;
        this.data = data;
        resources = ResourceManager.Instance;
        workTimer = 0f;
        isWorking = false;
    }

    public void Work()
    {
        if (!isWorking) return;

        workTimer += Time.deltaTime;
        if (workTimer >= data.workInterval)
        {
            PerformWork();
            workTimer = 0f;
        }
    }

    protected abstract void PerformWork();

    public void StartWorking() => isWorking = true;
    public void StopWorking() => isWorking = false;
}
