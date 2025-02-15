using System.Collections.Generic;
using UnityEngine;


public interface INeedsManager
{
    Need GetMostUrgentNeed();
    void FulfillNeed(Need need);
    void UpdateNeeds();
    Transform GetNeedLocation(Need need);
    IReadOnlyList<Need> GetAllNeeds();  // For UI/monitoring
    bool HasUrgentNeeds { get; }  // Quick check for state management
}
