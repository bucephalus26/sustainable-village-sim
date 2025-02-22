using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controller for the villager information panel
/// </summary>
public class VillagerPanel : UIPanel
{
    [Header("Content References")]
    [SerializeField] private Transform contentContainer;
    [SerializeField] private GameObject villagerEntryPrefab;
    [SerializeField] private TMP_InputField searchField;
    [SerializeField] private TMP_Text totalVillagersText;
    [SerializeField] private TMP_Dropdown sortDropdown;

    private List<VillagerEntry> activeEntries = new List<VillagerEntry>();
    private List<GameObject> instantiatedObjects = new List<GameObject>();

    public override void Initialize()
    {
        base.Initialize();

        // Set up search functionality
        if (searchField != null)
        {
            searchField.onValueChanged.AddListener(FilterEntries);
        }

        // Set up sorting functionality
        if (sortDropdown != null)
        {
            sortDropdown.ClearOptions();
            sortDropdown.AddOptions(new List<string> { "Name", "Profession", "State" });
            sortDropdown.onValueChanged.AddListener(SortEntries);
        }

        // Listen for villager events
        EventManager.Instance.AddListener<VillagerEvents.StateChangeEvent>(OnVillagerStateChanged);

        RefreshVillagerList();
    }

    protected override void OnShow()
    {
        base.OnShow();
        RefreshVillagerList();
    }

    public void RefreshVillagerList()
    {
        ClearEntries();

        // Find all villagers in the scene
        Villager[] villagers = FindObjectsByType<Villager>(FindObjectsSortMode.None);

        // Update total count
        if (totalVillagersText != null)
        {
            totalVillagersText.text = $"Total Villagers: {villagers.Length}";
        }

        // Create UI entries for each villager
        foreach (var villager in villagers)
        {
            CreateVillagerEntry(villager);
        }

        // Apply current sort and filter
        if (sortDropdown != null)
        {
            SortEntries(sortDropdown.value);
        }

        if (searchField != null)
        {
            FilterEntries(searchField.text);
        }
    }

    private void CreateVillagerEntry(Villager villager)
    {
        if (villagerEntryPrefab == null || contentContainer == null) return;

        GameObject entryObject = Instantiate(villagerEntryPrefab, contentContainer);
        VillagerEntry entry = entryObject.GetComponent<VillagerEntry>();
        if (entry != null)
        {
            entry.Initialize(villager);
            activeEntries.Add(entry);
        }

        instantiatedObjects.Add(entryObject);
    }

    private void ClearEntries()
    {
        activeEntries.Clear();

        foreach (var obj in instantiatedObjects)
        {
            Destroy(obj);
        }

        instantiatedObjects.Clear();
    }

    private void FilterEntries(string searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            foreach (var entry in activeEntries)
            {
                entry.gameObject.SetActive(true);
            }
            return;
        }

        searchText = searchText.ToLowerInvariant();

        foreach (var entry in activeEntries)
        {
            bool matchesName = entry.VillagerName.ToLowerInvariant().Contains(searchText);
            bool matchesProfession = entry.ProfessionType.ToLowerInvariant().Contains(searchText);
            bool matchesState = entry.CurrentState.ToLowerInvariant().Contains(searchText);

            entry.gameObject.SetActive(matchesName || matchesProfession || matchesState);
        }
    }



    private void SortEntries(int sortIndex)
    {
        List<VillagerEntry> sortedEntries = new List<VillagerEntry>(activeEntries);

        switch (sortIndex)
        {
            case 0: // Sort by Name
                sortedEntries.Sort((a, b) => string.Compare(a.VillagerName, b.VillagerName));
                break;
            case 1: // Sort by Profession
                sortedEntries.Sort((a, b) => string.Compare(a.ProfessionType, b.ProfessionType));
                break;
            case 2: // Sort by State
                sortedEntries.Sort((a, b) => string.Compare(a.CurrentState, b.CurrentState));
                break;
        }

        // Apply new sort order
        for (int i = 0; i < sortedEntries.Count; i++)
        {
            sortedEntries[i].transform.SetSiblingIndex(i);
        }
    }

    private void OnVillagerStateChanged(VillagerEvents.StateChangeEvent evt)
    {
        if (!gameObject.activeSelf) return;

        // Find and update the specific entry for this villager
        foreach (var entry in activeEntries)
        {
            if (entry.VillagerName == evt.VillagerName)
            {
                entry.UpdateStateDisplay(evt.NewState.Name);
                break;
            }
        }
    }

    protected override void Reset()
    {
        if (searchField != null)
        {
            searchField.text = string.Empty;
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (searchField != null)
        {
            searchField.onValueChanged.RemoveListener(FilterEntries);
        }

        if (sortDropdown != null)
        {
            sortDropdown.onValueChanged.RemoveListener(SortEntries);
        }

        if (EventManager.Instance != null)
        {
            EventManager.Instance.RemoveListener<VillagerEvents.StateChangeEvent>(OnVillagerStateChanged);
        }
    }
}