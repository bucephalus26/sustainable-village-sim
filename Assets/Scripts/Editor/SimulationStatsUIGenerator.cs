using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

#if UNITY_EDITOR
/// <summary>
/// This script automatically generates the complete UI for SimulationStatistics.
/// Place this in an Editor folder and run it from the menu.
/// </summary>
public class SimulationStatsUIGenerator : EditorWindow
{
    private SimulationStatistics statsComponent;
    private TMP_FontAsset fontAsset;
    private Color panelColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
    private Color headerColor = new Color(0.15f, 0.15f, 0.15f, 1f);
    private Color contentColor = new Color(0.1f, 0.1f, 0.1f, 0.7f);
    private Color textColor = Color.white;
    private Color buttonColor = new Color(0.3f, 0.3f, 0.3f, 1f);

    private bool createNewStatsComponent = true;
    private bool autoAssignReferences = true;
    private bool createToggleButton = true;

    [MenuItem("Tools/Generate SimulationStats UI")]
    public static void ShowWindow()
    {
        GetWindow<SimulationStatsUIGenerator>("Stats UI Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Simulation Statistics UI Generator", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        statsComponent = (SimulationStatistics)EditorGUILayout.ObjectField("Stats Component", statsComponent, typeof(SimulationStatistics), true);
        fontAsset = (TMP_FontAsset)EditorGUILayout.ObjectField("TMP Font Asset", fontAsset, typeof(TMP_FontAsset), false);

        EditorGUILayout.Space();
        GUILayout.Label("UI Colors", EditorStyles.boldLabel);
        panelColor = EditorGUILayout.ColorField("Panel Color", panelColor);
        headerColor = EditorGUILayout.ColorField("Header Color", headerColor);
        contentColor = EditorGUILayout.ColorField("Content Color", contentColor);
        textColor = EditorGUILayout.ColorField("Text Color", textColor);
        buttonColor = EditorGUILayout.ColorField("Button Color", buttonColor);

        EditorGUILayout.Space();
        GUILayout.Label("Options", EditorStyles.boldLabel);
        createNewStatsComponent = EditorGUILayout.Toggle("Create Stats Component if Missing", createNewStatsComponent);
        autoAssignReferences = EditorGUILayout.Toggle("Auto-Assign References", autoAssignReferences);
        createToggleButton = EditorGUILayout.Toggle("Create Toggle Button", createToggleButton);

        EditorGUILayout.Space();
        if (GUILayout.Button("Generate UI"))
        {
            GenerateCompleteUI();
        }
    }

    private void GenerateCompleteUI()
    {
        // Make sure we have a SimulationStatistics component
        if (statsComponent == null && createNewStatsComponent)
        {
            GameObject statsObj = new GameObject("SimulationStatistics");
            statsComponent = statsObj.AddComponent<SimulationStatistics>();
        }

        if (statsComponent == null)
        {
            EditorUtility.DisplayDialog("Error", "Please assign a SimulationStatistics component or enable 'Create Stats Component if Missing'.", "OK");
            return;
        }

        // Create the panel prefab first
        GameObject panelPrefab = CreateStatPanelPrefab();

        // Create the main UI
        GameObject canvasObj = CreateCanvas();
        GameObject statsPanelObj = CreateStatsPanel(canvasObj.transform);
        GameObject headerObj = CreateHeader(statsPanelObj.transform);
        GameObject scrollViewObj = CreateScrollView(statsPanelObj.transform);

        // Create individual stat panels
        Transform contentTransform = scrollViewObj.transform.Find("Viewport/Content");
        Dictionary<string, GameObject> panels = CreateStatPanels(contentTransform, panelPrefab);

        // Create toggle button if needed
        if (createToggleButton)
        {
            CreateToggleButton(canvasObj.transform);
        }

        // Assign references to the SimulationStatistics component
        if (autoAssignReferences)
        {
            AssignReferences(statsPanelObj, panels, headerObj);
        }

        Debug.Log("SimulationStats UI generation complete!");
    }

    private GameObject CreateCanvas()
    {
        // Look for existing canvas
        Canvas existingCanvas = FindObjectOfType<Canvas>();
        if (existingCanvas != null && existingCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            return existingCanvas.gameObject;
        }

        // Create new canvas
        GameObject canvasObj = new GameObject("StatsCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        // Create EventSystem if needed
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        return canvasObj;
    }

    private GameObject CreateStatsPanel(Transform canvasTransform)
    {
        GameObject panelObj = new GameObject("StatsPanel");
        panelObj.transform.SetParent(canvasTransform, false);

        RectTransform rect = panelObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = new Vector2(10, -10);
        rect.sizeDelta = new Vector2(300, 600);

        Image image = panelObj.AddComponent<Image>();
        image.color = panelColor;

        return panelObj;
    }

    private GameObject CreateHeader(Transform panelTransform)
    {
        GameObject headerObj = new GameObject("Header");
        headerObj.transform.SetParent(panelTransform, false);

        RectTransform rect = headerObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(0.5f, 1);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(0, 40);

        Image image = headerObj.AddComponent<Image>();
        image.color = headerColor;

        HorizontalLayoutGroup layout = headerObj.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(5, 5, 5, 5);
        layout.spacing = 10;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = true;
        layout.childControlHeight = true;
        layout.childAlignment = TextAnchor.MiddleLeft;

        // Create title
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(headerObj.transform, false);

        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "Village Statistics";
        titleText.fontSize = 18;
        titleText.fontStyle = FontStyles.Bold;
        titleText.color = textColor;
        if (fontAsset != null)
            titleText.font = fontAsset;

        // Create detailed toggle
        GameObject toggleObj = new GameObject("DetailedToggle");
        toggleObj.transform.SetParent(headerObj.transform, false);

        RectTransform toggleRect = toggleObj.AddComponent<RectTransform>();
        toggleRect.sizeDelta = new Vector2(120, 30);

        Toggle toggle = toggleObj.AddComponent<Toggle>();

        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(toggleObj.transform, false);

        RectTransform bgRect = bgObj.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = buttonColor;

        GameObject checkmarkObj = new GameObject("Checkmark");
        checkmarkObj.transform.SetParent(bgObj.transform, false);

        RectTransform checkRect = checkmarkObj.AddComponent<RectTransform>();
        checkRect.anchorMin = new Vector2(0, 0);
        checkRect.anchorMax = new Vector2(1, 1);
        checkRect.offsetMin = new Vector2(2, 2);
        checkRect.offsetMax = new Vector2(-2, -2);

        Image checkImage = checkmarkObj.AddComponent<Image>();
        checkImage.color = new Color(0.7f, 0.7f, 0.7f, 1);

        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(toggleObj.transform, false);

        RectTransform labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0, 0);
        labelRect.anchorMax = new Vector2(1, 1);
        labelRect.offsetMin = new Vector2(25, 0);
        labelRect.offsetMax = Vector2.zero;

        TextMeshProUGUI labelText = labelObj.AddComponent<TextMeshProUGUI>();
        labelText.text = "Detailed Stats";
        labelText.fontSize = 14;
        labelText.color = textColor;
        labelText.alignment = TextAlignmentOptions.MidlineLeft;
        if (fontAsset != null)
            labelText.font = fontAsset;

        toggle.targetGraphic = bgImage;
        toggle.graphic = checkImage;
        toggle.isOn = false;

        return headerObj;
    }

    private GameObject CreateScrollView(Transform panelTransform)
    {
        // Create scroll view
        GameObject scrollViewObj = new GameObject("StatsScrollView");
        scrollViewObj.transform.SetParent(panelTransform, false);

        RectTransform scrollRect = scrollViewObj.AddComponent<RectTransform>();
        scrollRect.anchorMin = Vector2.zero;
        scrollRect.anchorMax = Vector2.one;
        scrollRect.offsetMin = new Vector2(0, 0);
        scrollRect.offsetMax = new Vector2(0, -40);

        ScrollRect scrollRectComp = scrollViewObj.AddComponent<ScrollRect>();
        scrollRectComp.horizontal = false;
        scrollRectComp.vertical = true;

        // Create viewport
        GameObject viewportObj = new GameObject("Viewport");
        viewportObj.transform.SetParent(scrollViewObj.transform, false);

        RectTransform viewportRect = viewportObj.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = Vector2.zero;

        Image viewportImage = viewportObj.AddComponent<Image>();
        viewportImage.color = new Color(1, 1, 1, 0.01f);

        Mask viewportMask = viewportObj.AddComponent<Mask>();
        viewportMask.showMaskGraphic = false;

        // Create content
        GameObject contentObj = new GameObject("Content");
        contentObj.transform.SetParent(viewportObj.transform, false);

        RectTransform contentRect = contentObj.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.sizeDelta = new Vector2(0, 500);

        VerticalLayoutGroup contentLayout = contentObj.AddComponent<VerticalLayoutGroup>();
        contentLayout.padding = new RectOffset(5, 5, 5, 5);
        contentLayout.spacing = 10;
        contentLayout.childControlWidth = true;
        contentLayout.childControlHeight = false;
        contentLayout.childForceExpandWidth = true;
        contentLayout.childForceExpandHeight = false;

        ContentSizeFitter contentFitter = contentObj.AddComponent<ContentSizeFitter>();
        contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Create scrollbar
        GameObject scrollbarObj = new GameObject("Scrollbar");
        scrollbarObj.transform.SetParent(scrollViewObj.transform, false);

        RectTransform scrollbarRect = scrollbarObj.AddComponent<RectTransform>();
        scrollbarRect.anchorMin = new Vector2(1, 0);
        scrollbarRect.anchorMax = new Vector2(1, 1);
        scrollbarRect.pivot = new Vector2(1, 1);
        scrollbarRect.sizeDelta = new Vector2(10, 0);
        scrollbarRect.anchoredPosition = Vector2.zero;

        Image scrollbarImage = scrollbarObj.AddComponent<Image>();
        scrollbarImage.color = new Color(0.3f, 0.3f, 0.3f, 0.8f);

        Scrollbar scrollbar = scrollbarObj.AddComponent<Scrollbar>();
        scrollbar.direction = Scrollbar.Direction.BottomToTop;

        // Create sliding area
        GameObject slidingAreaObj = new GameObject("SlidingArea");
        slidingAreaObj.transform.SetParent(scrollbarObj.transform, false);

        RectTransform slidingAreaRect = slidingAreaObj.AddComponent<RectTransform>();
        slidingAreaRect.anchorMin = Vector2.zero;
        slidingAreaRect.anchorMax = Vector2.one;
        slidingAreaRect.offsetMin = Vector2.zero;
        slidingAreaRect.offsetMax = Vector2.zero;

        // Create handle
        GameObject handleObj = new GameObject("Handle");
        handleObj.transform.SetParent(slidingAreaObj.transform, false);

        RectTransform handleRect = handleObj.AddComponent<RectTransform>();
        handleRect.anchorMin = Vector2.zero;
        handleRect.anchorMax = new Vector2(1, 0.2f);
        handleRect.offsetMin = Vector2.zero;
        handleRect.offsetMax = Vector2.zero;

        Image handleImage = handleObj.AddComponent<Image>();
        handleImage.color = new Color(0.7f, 0.7f, 0.7f, 0.8f);

        // Set up references
        scrollbar.handleRect = handleRect;
        scrollbar.targetGraphic = handleImage;

        scrollRectComp.viewport = viewportRect;
        scrollRectComp.content = contentRect;
        scrollRectComp.verticalScrollbar = scrollbar;

        return scrollViewObj;
    }

    private Dictionary<string, GameObject> CreateStatPanels(Transform contentTransform, GameObject panelPrefab)
    {
        Dictionary<string, GameObject> panels = new Dictionary<string, GameObject>();

        // Define all the panels we need
        string[] panelNames = new string[] {
            "Time", "Day", "Resources", "Villagers", "States", "Mood", "Goals"
        };

        // Initial heights for panels
        Dictionary<string, float> initialHeights = new Dictionary<string, float>() {
            { "Time", 40f },
            { "Day", 40f },
            { "Resources", 100f },
            { "Villagers", 120f },
            { "States", 100f },
            { "Mood", 100f },
            { "Goals", 150f }
        };

        // Sample texts for panels
        Dictionary<string, string> sampleTexts = new Dictionary<string, string>() {
            { "Time", "Time: Day 1, 12:00 - Morning" },
            { "Day", "Day: 1" },
            { "Resources", "Resources:\nFood: 100.0 ($10.00)\nGoods: 50.0 ($15.00)\nWealth: 1000.0\nStone: 200.0 ($5.00)" },
            { "Villagers", "Villagers:\nTotal: 10\nFarmer: 3\nMiner: 2\nCrafter: 2\nTrader: 1\nUnemployed: 2" },
            { "States", "Current Activities:\nWorking: 4\nSocializing: 2\nSleeping: 1\nIdle: 2\nFulfilling Needs: 1" },
            { "Mood", "Happiness:\nAverage: 65.0\nHappy: 3 villagers\nContent: 5 villagers\nUnhappy: 2 villagers" },
            { "Goals", "Goals:\nActive Goals: 12\n\nActive Goal Types:\nEarnMoney: 4 (35% avg)\nUpgradeHome: 3 (50% avg)\nMakeFriends: 3 (25% avg)\nLearnSkill: 2 (10% avg)" }
        };

        Dictionary<string, string> detailedTexts = new Dictionary<string, string>() {
            { "Time", "" },
            { "Day", "" },
            { "Resources", "Production per day:\nFood: +12.5 (2.1 per farmer)\nGoods: +8.3 (1.7 per crafter)\nMarket prices fluctuation: +2.3%" },
            { "Villagers", "Average Needs:\nHunger: 72.4\nEnergy: 68.3\nSocial: 61.9\nComfort: 70.2\nFun: 65.8" },
            { "States", "Time spent today:\nWorking: 42%\nSocializing: 18%\nSleeping: 25%\nIdle: 5%\nFulfilling Needs: 10%" },
            { "Mood", "Happiest: Bob (93.2)\nUnhappiest: Alice (28.7)\nWork efficiency: 95.6%\nSocial quality: 83.4%" },
            { "Goals", "Completed Goals:\nEarnMoney: 12\nUpgradeHome: 8\nMakeFriends: 5\nLearnSkill: 3" }
        };

        // Create each panel
        foreach (string name in panelNames)
        {
            GameObject panelObj = Instantiate(panelPrefab, contentTransform);
            panelObj.name = name + "Panel";

            StatPanelController controller = panelObj.GetComponent<StatPanelController>();
            if (controller != null)
            {
                controller.SetTitle(name);
                controller.SetBasicContent(sampleTexts[name]);
                controller.SetDetailedContent(detailedTexts[name]);
            }

            RectTransform rect = panelObj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, initialHeights[name]);

            panels.Add(name, panelObj);
        }

        return panels;
    }

    private GameObject CreateStatPanelPrefab()
    {
        // Create the prefab in the scene first, then make it a prefab asset
        GameObject prefabRoot = new GameObject("StatPanelPrefab_Temp");

        RectTransform rect = prefabRoot.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(290, 60);

        Image image = prefabRoot.AddComponent<Image>();
        image.color = contentColor;

        VerticalLayoutGroup layout = prefabRoot.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(5, 5, 5, 5);
        layout.spacing = 5;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        ContentSizeFitter fitter = prefabRoot.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Create header
        GameObject headerObj = new GameObject("PanelHeader");
        headerObj.transform.SetParent(prefabRoot.transform, false);

        RectTransform headerRect = headerObj.AddComponent<RectTransform>();
        headerRect.sizeDelta = new Vector2(0, 30);

        Image headerImage = headerObj.AddComponent<Image>();
        headerImage.color = headerColor;

        HorizontalLayoutGroup headerLayout = headerObj.AddComponent<HorizontalLayoutGroup>();
        headerLayout.padding = new RectOffset(5, 5, 5, 5);
        headerLayout.spacing = 5;
        headerLayout.childControlWidth = true;
        headerLayout.childControlHeight = true;
        headerLayout.childForceExpandWidth = false;
        headerLayout.childForceExpandHeight = true;

        // Title text
        GameObject titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(headerObj.transform, false);

        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "Panel Title";
        titleText.fontSize = 16;
        titleText.fontStyle = FontStyles.Bold;
        titleText.color = textColor;
        titleText.alignment = TextAlignmentOptions.MidlineLeft;
        if (fontAsset != null)
            titleText.font = fontAsset;

        // Create expand button
        GameObject buttonObj = new GameObject("ExpandButton");
        buttonObj.transform.SetParent(headerObj.transform, false);

        RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(20, 20);

        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = buttonColor;

        Button button = buttonObj.AddComponent<Button>();
        button.targetGraphic = buttonImage;

        GameObject arrowObj = new GameObject("Arrow");
        arrowObj.transform.SetParent(buttonObj.transform, false);

        RectTransform arrowRect = arrowObj.AddComponent<RectTransform>();
        arrowRect.anchorMin = Vector2.zero;
        arrowRect.anchorMax = Vector2.one;
        arrowRect.offsetMin = new Vector2(2, 2);
        arrowRect.offsetMax = new Vector2(-2, -2);

        TextMeshProUGUI arrowText = arrowObj.AddComponent<TextMeshProUGUI>();
        arrowText.text = "▼";
        arrowText.fontSize = 12;
        arrowText.color = textColor;
        arrowText.alignment = TextAlignmentOptions.Center;
        if (fontAsset != null)
            arrowText.font = fontAsset;

        // Basic content
        GameObject basicObj = new GameObject("BasicContent");
        basicObj.transform.SetParent(prefabRoot.transform, false);

        RectTransform basicRect = basicObj.AddComponent<RectTransform>();
        basicRect.sizeDelta = new Vector2(0, 60);

        TextMeshProUGUI basicText = basicObj.AddComponent<TextMeshProUGUI>();
        basicText.text = "Basic Content";
        basicText.fontSize = 14;
        basicText.color = textColor;
        basicText.alignment = TextAlignmentOptions.TopLeft;
        if (fontAsset != null)
            basicText.font = fontAsset;

        // Detailed content
        GameObject detailedObj = new GameObject("DetailedContent");
        detailedObj.transform.SetParent(prefabRoot.transform, false);

        RectTransform detailedRect = detailedObj.AddComponent<RectTransform>();
        detailedRect.sizeDelta = new Vector2(0, 60);

        TextMeshProUGUI detailedText = detailedObj.AddComponent<TextMeshProUGUI>();
        detailedText.text = "Detailed Content";
        detailedText.fontSize = 12;
        detailedText.color = new Color(0.8f, 0.8f, 0.8f, 1f);
        detailedText.alignment = TextAlignmentOptions.TopLeft;
        if (fontAsset != null)
            detailedText.font = fontAsset;

        CanvasGroup canvasGroup = detailedObj.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        // Add controller script
        StatPanelController controller = prefabRoot.AddComponent<StatPanelController>();
        controller.titleText = titleText;
        controller.basicContentText = basicText;
        controller.detailedContentText = detailedText;
        controller.detailedCanvasGroup = canvasGroup;
        controller.expandButton = button;
        controller.expandButtonRect = arrowRect;

        // Make it a prefab asset if this is running in the editor
        string prefabPath = "Assets/StatPanelPrefab.prefab";

#if UNITY_EDITOR
        // Create the prefab asset
        bool prefabSuccess;
        GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

        if (existingPrefab != null)
        {
            // Update existing prefab
            prefabSuccess = PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
        }
        else
        {
            // Create new prefab
            prefabSuccess = PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
        }

        if (prefabSuccess)
        {
            Debug.Log("Stat panel prefab created successfully at: " + prefabPath);
        }
        else
        {
            Debug.LogError("Failed to create stat panel prefab!");
        }
#endif

        // Load the prefab asset we just created
        GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefabAsset == null)
        {
            prefabAsset = prefabRoot; // Use the scene instance if prefab creation failed
        }
        else
        {
            // Clean up the temporary object
            DestroyImmediate(prefabRoot);
        }

        return prefabAsset;
    }

    private void CreateToggleButton(Transform canvasTransform)
    {
        GameObject buttonObj = new GameObject("StatsToggleButton");
        buttonObj.transform.SetParent(canvasTransform, false);

        RectTransform rect = buttonObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(1, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(1, 1);
        rect.anchoredPosition = new Vector2(-10, -10);
        rect.sizeDelta = new Vector2(120, 30);

        Image image = buttonObj.AddComponent<Image>();
        image.color = panelColor;

        Button button = buttonObj.AddComponent<Button>();
        button.targetGraphic = image;

        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(0.3f, 0.3f, 0.3f, 0.8f);
        colors.pressedColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        button.colors = colors;

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = "Toggle Stats";
        text.fontSize = 14;
        text.color = textColor;
        text.alignment = TextAlignmentOptions.Center;
        if (fontAsset != null)
            text.font = fontAsset;

        // Set up button click action
        if (statsComponent != null)
        {
            button.onClick.AddListener(statsComponent.ToggleStatsPanel);
        }
    }

    private void AssignReferences(GameObject statsPanelObj, Dictionary<string, GameObject> panels, GameObject headerObj)
    {
        if (statsComponent == null) return;

        // Use SerializedObject to modify private serialized fields
        SerializedObject serializedStats = new SerializedObject(statsComponent);

        // Set statsPanel reference
        SerializedProperty statsPanelProp = serializedStats.FindProperty("statsPanel");
        if (statsPanelProp != null)
        {
            statsPanelProp.objectReferenceValue = statsPanelObj;
        }

        // Set the detailed toggle reference
        Toggle detailedToggle = headerObj.GetComponentInChildren<Toggle>();
        if (detailedToggle != null)
        {
            SerializedProperty toggleProp = serializedStats.FindProperty("showDetailedStats");
            if (toggleProp != null)
            {
                toggleProp.objectReferenceValue = detailedToggle;
            }
        }

        // Connect all text displays
        foreach (var panel in panels)
        {
            StatPanelController controller = panel.Value.GetComponent<StatPanelController>();
            if (controller == null) continue;

            string panelName = panel.Key;
            TextMeshProUGUI basicText = controller.BasicContent;

            if (basicText == null) continue;

            string propertyName = "";
            switch (panelName)
            {
                case "Time":
                    propertyName = "timeDisplay";
                    break;
                case "Day":
                    propertyName = "dayDisplay";
                    break;
                case "Resources":
                    propertyName = "resourcesDisplay";
                    break;
                case "Villagers":
                    propertyName = "villagerStatsDisplay";
                    break;
                case "States":
                    propertyName = "stateStatsDisplay";
                    break;
                case "Mood":
                    propertyName = "moodStatsDisplay";
                    break;
                case "Goals":
                    propertyName = "goalStatsDisplay";
                    break;
            }

            if (!string.IsNullOrEmpty(propertyName))
            {
                SerializedProperty textProp = serializedStats.FindProperty(propertyName);
                if (textProp != null)
                {
                    textProp.objectReferenceValue = basicText;
                }
            }
        }

        // Apply all the changes
        serializedStats.ApplyModifiedProperties();
    }
}
#endif