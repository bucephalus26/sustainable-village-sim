using UnityEngine;

[System.Serializable]
public class Goal
{
    public GoalType type;
    public string description;
    public float progress;
    public float target;
    public bool completed;

    public float ProgressPercentage => Mathf.Clamp01(progress / target) * 100f;

    public override string ToString()
    {
        return $"{description} ({ProgressPercentage:F0}%)";
    }
}
