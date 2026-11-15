using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>Coordinates survival, inventory, mission progress, gameplay UI, and scene transitions.</summary>
public sealed class CosmicRaysGameplayController : MonoBehaviour
{
    /// <summary>Build Settings scene name for the gameplay scene.</summary>
    public const string GameplaySceneName = "Gameplay";
    /// <summary>Build Settings scene name for the main menu.</summary>
    public const string MainMenuSceneName = "MainMenu";

    private const string PausePanel = "Pause";
    private const string SettingsPanel = "Settings";
    private const string InventoryPanel = "Inventory";
    private const string MissionPanel = "Mission";
    private const string LocationsPanel = "Locations";
    private const string LabPanel = "Lab";
    private const string ResearchBasePanel = "ResearchBase";
    private const string MissionCompletePanel = "MissionComplete";
    private const string GameOverPanel = "GameOver";

    /// <summary>Currently active gameplay controller.</summary>
    public static CosmicRaysGameplayController Active { get; private set; }

    /// <summary>Raised after survival values change.</summary>
    public event Action ResourcesChanged;
    /// <summary>Raised after an inventory stack changes.</summary>
    public event Action InventoryChanged;
    /// <summary>Raised after mission objective progress changes.</summary>
    public event Action MissionProgressChanged;
    /// <summary>Raised when the gameplay UI screen changes.</summary>
    public event Action<string> ScreenChanged;
    /// <summary>Raised after all configured mission objectives are completed.</summary>
    public event Action MissionCompleted;
    /// <summary>Raised when the mission enters its failed state.</summary>
    public event Action<string> MissionFailed;
    /// <summary>Raised whenever a survival resource reaches its configured minimum.</summary>
    public event Action<CosmicResource> ResourceDepleted;

    private readonly Dictionary<string, RectTransform> _panels =
        new Dictionary<string, RectTransform>(StringComparer.Ordinal);
    private readonly Stack<string> _panelHistory = new Stack<string>();
    private SurvivalConfig _config;
    private SurvivalStatus _survival;
    private InventoryState _inventory;
    private MissionProgress _missionProgress;
    private CosmicRaysSaveData _saveData;
    private RectTransform _hudPanel;
    private RectTransform _inventoryContent;
    private RectTransform _missionContent;
    private Text _oxygenLabel;
    private Text _waterLabel;
    private Text _energyLabel;
    private Text _warningLabel;
    private Text _feedbackLabel;
    private Text _gameOverReasonLabel;
    private Image _oxygenFill;
    private Image _waterFill;
    private Image _energyFill;
    private string _activePanelId;
    private bool _terminal;
    private float _autosaveTimer;

    private void Awake()
    {
        if (Active != null && Active != this)
        {
            Debug.LogError("More than one COSMIC RAYS gameplay controller is active in the scene.", this);
            enabled = false;
            return;
        }
        Active = this;
    }

    private void Start()
    {
        CosmicRaysSettings.Initialize();
        _config = CosmicRaysRuntimeContent.LoadSurvivalConfig();
        if (_config == null)
        {
            Debug.LogError("Gameplay cannot start without the SurvivalConfig asset.", this);
            if (Active == this) Active = null;
            enabled = false;
            return;
        }
        string configError;
        if (!_config.Validate(out configError))
        {
            Debug.LogError("Gameplay cannot start. " + configError, this);
            if (Active == this) Active = null;
            enabled = false;
            return;
        }

        string loadMessage = null;
        CosmicRaysSaveData pendingSave = CosmicRaysSession.TakePendingSave();
        if (pendingSave != null)
        {
            if (CosmicRaysSaveSystem.ValidateData(pendingSave, out loadMessage))
            {
                _saveData = pendingSave;
            }
            else
            {
                Debug.LogError("The pending mission state was rejected. " + loadMessage, this);
                loadMessage = "The pending mission state was invalid; saved progress was loaded instead.";
            }
        }

        if (_saveData == null)
        {
            CosmicRaysSaveData loadedSave;
            string saveMessage;
            if (CosmicRaysSaveSystem.TryLoad(out loadedSave, out saveMessage))
            {
                _saveData = loadedSave;
                if (string.IsNullOrEmpty(loadMessage))
                {
                    loadMessage = saveMessage;
                }
            }
            else
            {
                _saveData = CosmicRaysSession.CreateNewGame(_config);
                if (CosmicRaysSaveSystem.HasAnySave)
                {
                    Debug.LogError(saveMessage);
                    loadMessage = saveMessage;
                }
            }
        }

        _survival = new SurvivalStatus(_config, _saveData.resources);
        _inventory = new InventoryState(
            CosmicRaysRuntimeContent.LoadItems(),
            _saveData.inventory);
        _missionProgress = new MissionProgress(
            CosmicRaysRuntimeContent.LoadObjectives(),
            _saveData.completedObjectiveIds);

        _survival.Changed += OnSurvivalChanged;
        _survival.Depleted += OnResourceDepleted;
        _inventory.Changed += OnInventoryChanged;
        _missionProgress.Changed += OnMissionProgressChanged;

        BuildInterface();
        RefreshResources();
        RefreshInventory();
        RefreshMissionObjectives();

        CosmicRaysLevelOneController levelOne = GetComponent<CosmicRaysLevelOneController>();
        if (levelOne == null)
        {
            levelOne = gameObject.AddComponent<CosmicRaysLevelOneController>();
        }
        levelOne.Initialize(this);

        string sessionNotice = CosmicRaysSession.Notice;
        if (!string.IsNullOrEmpty(sessionNotice))
        {
            ShowFeedback(sessionNotice);
            CosmicRaysSession.ClearNotice();
        }
        else if (!string.IsNullOrEmpty(loadMessage))
        {
            ShowFeedback(loadMessage);
        }

        if (_config.failMissionWhenResourceIsEmpty && _survival.IsDepleted)
        {
            ShowGameOver("A critical life-support resource was empty when the mission resumed.");
        }
        else if (_missionProgress.IsComplete)
        {
            ShowMissionComplete();
        }
    }

    private void Update()
    {
        if (_survival == null || _terminal)
        {
            return;
        }

        _survival.Tick(Time.deltaTime);
        _autosaveTimer += Time.unscaledDeltaTime;
        if (_autosaveTimer >= 60f)
        {
            _autosaveTimer = 0f;
            SaveProgress();
        }
    }

    private void OnDestroy()
    {
        if (_survival != null)
        {
            _survival.Changed -= OnSurvivalChanged;
            _survival.Depleted -= OnResourceDepleted;
        }
        if (_inventory != null) _inventory.Changed -= OnInventoryChanged;
        if (_missionProgress != null) _missionProgress.Changed -= OnMissionProgressChanged;
        if (Active == this)
        {
            Active = null;
            Time.timeScale = 1f;
        }
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused && _survival != null && !_terminal)
        {
            SaveProgress();
        }
    }

    private void OnApplicationQuit()
    {
        if (_survival != null && !_terminal)
        {
            SaveProgress();
        }
    }

    /// <summary>Adds an item stack, saves the changed inventory, and returns the accepted quantity.</summary>
    public int CollectItem(InventoryItemDefinition definition, int quantity)
    {
        if (_inventory == null || definition == null)
        {
            Debug.LogError("Cannot collect an item before the gameplay inventory is ready.");
            return 0;
        }

        int added = _inventory.Add(definition, quantity);
        if (added <= 0)
        {
            ShowFeedback("Inventory full: " + GetItemDisplayName(definition) + " was not collected.");
            return 0;
        }

        ShowFeedback("Collected " + GetItemDisplayName(definition) + " x" + added + ".");
        SaveProgress();
        return added;
    }

    /// <summary>Marks a configured mission objective complete and returns whether progress changed.</summary>
    public bool CompleteObjective(string objectiveId)
    {
        if (_terminal || _missionProgress == null)
        {
            return false;
        }

        bool completed = _missionProgress.Complete(objectiveId);
        if (!completed)
        {
            Debug.LogWarning("Unknown or already completed mission objective: " + objectiveId);
            return false;
        }

        if (_missionProgress.IsComplete)
        {
            ShowMissionComplete();
        }
        else
        {
            SaveProgress();
        }
        return true;
    }

    /// <summary>Gets the current value for a survival resource, or zero before gameplay is ready.</summary>
    public float GetResourceValue(CosmicResource resource)
    {
        return _survival != null ? _survival.CurrentFor(resource) : 0f;
    }

    /// <summary>Gets the configured minimum for a resource, or zero before gameplay is ready.</summary>
    public float GetResourceMinimum(CosmicResource resource)
    {
        return _survival != null ? _survival.MinimumFor(resource) : 0f;
    }

    /// <summary>Gets the configured maximum for a resource, or zero before gameplay is ready.</summary>
    public float GetResourceMaximum(CosmicResource resource)
    {
        return _survival != null ? _survival.MaximumFor(resource) : 0f;
    }

    /// <summary>Gets the current quantity for an inventory item ID.</summary>
    public int GetItemCount(string itemId)
    {
        return _inventory != null ? _inventory.GetCount(itemId) : 0;
    }

    /// <summary>Returns whether a configured objective is complete.</summary>
    public bool IsObjectiveComplete(string objectiveId)
    {
        return _missionProgress != null && _missionProgress.IsObjectiveComplete(objectiveId);
    }

    /// <summary>Shows or hides the generic campaign HUD while a playable level supplies its own HUD.</summary>
    public void SetGameplayHudVisible(bool visible)
    {
        if (_hudPanel != null)
        {
            _hudPanel.gameObject.SetActive(visible);
        }
    }

    /// <summary>Ends the active mission and opens the Mission Failed screen with the supplied reason.</summary>
    public void FailMission(string reason)
    {
        if (_terminal || _survival == null)
        {
            return;
        }

        ShowGameOver(string.IsNullOrWhiteSpace(reason) ? "The mission could not continue." : reason);
    }

    /// <summary>Opens the pause menu when no gameplay overlay is already open.</summary>
    public void PauseGameplay()
    {
        if (!_terminal && _activePanelId == null)
        {
            PushPanel(PausePanel);
        }
    }

    /// <summary>Closes all gameplay overlays and resumes gameplay.</summary>
    public void ResumeGameplay()
    {
        if (_terminal)
        {
            return;
        }

        foreach (RectTransform panel in _panels.Values)
        {
            panel.gameObject.SetActive(false);
        }
        _panelHistory.Clear();
        _activePanelId = null;
        Time.timeScale = 1f;
        ScreenChanged?.Invoke("Gameplay");
        CosmicRaysUIFactory.Focus(FindHudButton("Pause"));
    }

    /// <summary>Opens the inventory overlay.</summary>
    public void OpenInventory()
    {
        PushPanel(InventoryPanel);
    }

    /// <summary>Opens the settings overlay.</summary>
    public void OpenSettings()
    {
        PushPanel(SettingsPanel);
    }

    /// <summary>Opens the mission objectives overlay.</summary>
    public void OpenMissionLog()
    {
        PushPanel(MissionPanel);
    }

    /// <summary>Opens the location selection overlay.</summary>
    public void OpenLocations()
    {
        PushPanel(LocationsPanel);
    }

    /// <summary>Saves the current survival, inventory, and mission state.</summary>
    public bool SaveProgress()
    {
        if (_survival == null || _inventory == null || _missionProgress == null)
        {
            Debug.LogError("COSMIC RAYS progress cannot be saved before gameplay systems are initialized.", this);
            return false;
        }

        _saveData = new CosmicRaysSaveData
        {
            resources = _survival.Capture(),
            inventory = _inventory.Capture(),
            completedObjectiveIds = _missionProgress.CaptureCompletedIds()
        };

        string error;
        if (!CosmicRaysSaveSystem.TrySave(_saveData, out error))
        {
            Debug.LogError(error, this);
            ShowFeedback("SAVE FAILED: " + error);
            return false;
        }
        return true;
    }

    private void BuildInterface()
    {
        RectTransform root = CosmicRaysUIFactory.CreateCanvasRoot("COSMIC RAYS Gameplay UI");
        _hudPanel = CosmicRaysUIFactory.CreatePanel(root, "Mission HUD", false);
        Image hudBackground = _hudPanel.GetComponent<Image>();
        hudBackground.color = new Color(0f, 0f, 0f, 0f);
        hudBackground.raycastTarget = false;

        _oxygenLabel = CreateMeter("Oxygen", 0.04f, 0.94f, new Color(0.18f, 0.72f, 0.92f, 1f), out _oxygenFill);
        _waterLabel = CreateMeter("Water", 0.35f, 0.94f, new Color(0.24f, 0.72f, 0.48f, 1f), out _waterFill);
        _energyLabel = CreateMeter("Energy", 0.66f, 0.94f, new Color(0.94f, 0.68f, 0.22f, 1f), out _energyFill);
        _warningLabel = CosmicRaysUIFactory.CreateAnchoredLabel(
            _hudPanel,
            "ResourceWarning",
            string.Empty,
            new Vector2(0.08f, 0.82f),
            new Vector2(0.92f, 0.9f),
            24,
            TextAnchor.MiddleCenter);
        _feedbackLabel = CosmicRaysUIFactory.CreateAnchoredLabel(
            _hudPanel,
            "Feedback",
            string.Empty,
            new Vector2(0.08f, 0.13f),
            new Vector2(0.92f, 0.2f),
            20,
            TextAnchor.MiddleCenter);

        BuildHudButtons(_hudPanel);
        BuildPausePanel(root);
        BuildSettingsPanel(root);
        BuildInventoryPanel(root);
        BuildMissionPanel(root);
        BuildLocationsPanel(root);
        BuildLabPanel(root);
        BuildResearchBasePanel(root);
        BuildMissionCompletePanel(root);
        BuildGameOverPanel(root);
    }

    private Text CreateMeter(string title, float xMin, float xMax, Color fillColor, out Image fill)
    {
        Text label = CosmicRaysUIFactory.CreateAnchoredLabel(
            _hudPanel,
            title + " Label",
            title,
            new Vector2(xMin, 0.93f),
            new Vector2(xMax, 0.99f),
            24,
            TextAnchor.MiddleLeft);

        Image background = CosmicRaysUIFactory.CreateAnchoredImage(
            _hudPanel,
            title + " Meter Background",
            new Vector2(xMin, 0.9f),
            new Vector2(xMax, 0.925f),
            new Color(0.1f, 0.14f, 0.18f, 0.95f));
        fill = CosmicRaysUIFactory.CreateAnchoredImage(
            background.transform,
            title + " Meter Fill",
            Vector2.zero,
            Vector2.one,
            fillColor);
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillOrigin = (int)Image.OriginHorizontal.Left;
        label.text = title + ": --";
        return label;
    }

    private void BuildHudButtons(RectTransform parent)
    {
        var rowObject = new GameObject("Gameplay Controls", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        RectTransform row = rowObject.GetComponent<RectTransform>();
        row.SetParent(parent, false);
        row.anchorMin = new Vector2(0.04f, 0.025f);
        row.anchorMax = new Vector2(0.96f, 0.115f);
        row.offsetMin = Vector2.zero;
        row.offsetMax = Vector2.zero;

        HorizontalLayoutGroup layout = rowObject.GetComponent<HorizontalLayoutGroup>();
        layout.spacing = 10f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = true;

        AddFlexibleButton(row, "Pause", "Pause", () =>
        {
            SaveProgress();
            PauseGameplay();
        });
        AddFlexibleButton(row, "Inventory", "Inventory", OpenInventory);
        AddFlexibleButton(row, "Mission Log", "Mission Log", OpenMissionLog);
        AddFlexibleButton(row, "Locations", "Base / Lab", OpenLocations);
    }

    private static Button AddFlexibleButton(
        Transform parent,
        string name,
        string label,
        UnityEngine.Events.UnityAction onClick)
    {
        Button button = CosmicRaysUIFactory.CreateButton(parent, name, label, onClick, 56f);
        button.GetComponent<LayoutElement>().flexibleWidth = 1f;
        return button;
    }

    private void BuildPausePanel(RectTransform root)
    {
        RectTransform panel = AddPanel(root, PausePanel);
        RectTransform column = CosmicRaysUIFactory.CreateColumn(panel, "Pause Content");
        CosmicRaysUIFactory.CreateLabel(column, "Title", "PAUSED", 38, 72f);
        CosmicRaysUIFactory.CreateButton(column, "ResumeButton", "Resume", ResumeGameplay);
        CosmicRaysUIFactory.CreateButton(column, "SettingsButton", "Settings", OpenSettings);
        CosmicRaysUIFactory.CreateButton(column, "MainMenuButton", "Save and Return to Main Menu", ReturnToMainMenu);
        CosmicRaysUIFactory.CreateButton(column, "ExitButton", "Exit", ExitGame);
    }

    private void BuildSettingsPanel(RectTransform root)
    {
        RectTransform panel = AddPanel(root, SettingsPanel);
        CosmicRaysSettingsUI.Build(panel, PopPanel);
    }

    private void BuildInventoryPanel(RectTransform root)
    {
        RectTransform panel = AddPanel(root, InventoryPanel);
        RectTransform column = CosmicRaysUIFactory.CreateColumn(panel, "Inventory Content");
        CosmicRaysUIFactory.CreateLabel(column, "Title", "INVENTORY", 38, 70f);
        CosmicRaysUIFactory.CreateLabel(
            column,
            "Hint",
            "Use oxygen, water, and energy supplies to restore the matching meter.",
            21,
            70f);
        _inventoryContent = CosmicRaysUIFactory.CreateScrollContent(column, "Inventory Items");
        CosmicRaysUIFactory.CreateButton(column, "BackButton", "Back", PopPanel);
    }

    private void BuildMissionPanel(RectTransform root)
    {
        RectTransform panel = AddPanel(root, MissionPanel);
        RectTransform column = CosmicRaysUIFactory.CreateColumn(panel, "Mission Content");
        CosmicRaysUIFactory.CreateLabel(column, "Title", "MISSION OBJECTIVES", 38, 70f);
        _missionContent = CosmicRaysUIFactory.CreateScrollContent(column, "Mission Objectives");
        CosmicRaysUIFactory.CreateButton(column, "BackButton", "Back", PopPanel);
    }

    private void BuildLocationsPanel(RectTransform root)
    {
        RectTransform panel = AddPanel(root, LocationsPanel);
        RectTransform column = CosmicRaysUIFactory.CreateColumn(panel, "Location Content");
        CosmicRaysUIFactory.CreateLabel(column, "Title", "RESEARCH BASE", 38, 70f);
        CosmicRaysUIFactory.CreateLabel(
            column,
            "Description",
            "Choose a project screen. The final base and laboratory environments are supplied by the 3D design team.",
            22,
            90f);
        CosmicRaysUIFactory.CreateButton(column, "ResearchBaseButton", "Habitat / Research Base", () => PushPanel(ResearchBasePanel));
        CosmicRaysUIFactory.CreateButton(column, "LabButton", "SPA-077 Laboratory", () => PushPanel(LabPanel));
        CosmicRaysUIFactory.CreateButton(column, "MissionButton", "Mission Objectives", () => PushPanel(MissionPanel));
        CosmicRaysUIFactory.CreateButton(column, "BackButton", "Back", PopPanel);
    }

    private void BuildLabPanel(RectTransform root)
    {
        RectTransform panel = AddPanel(root, LabPanel);
        RectTransform column = CosmicRaysUIFactory.CreateColumn(panel, "Laboratory Content");
        CosmicRaysUIFactory.CreateLabel(column, "Title", "SPA-077: SOLAR PARTICLE ABSORBER", 34, 86f);
        CosmicRaysUIFactory.CreateLabel(
            column,
            "Briefing",
            "Project concept: an experimental solar-orbiting research system intended to detect and study Solar Energetic Particles over a ten-year mission. Proposed systems include a Starlite thermal layer, particle detector, energy analyser, magnetic sensors, and experimental collection and storage research.",
            23,
            300f);
        CosmicRaysUIFactory.CreateLabel(
            column,
            "Caveat",
            "Particle collection and storage are experimental parts of the concept, not established technology.",
            21,
            84f);
        CosmicRaysUIFactory.CreateButton(column, "BackButton", "Back", PopPanel);
    }

    private void BuildResearchBasePanel(RectTransform root)
    {
        RectTransform panel = AddPanel(root, ResearchBasePanel);
        RectTransform column = CosmicRaysUIFactory.CreateColumn(panel, "Research Base Content");
        CosmicRaysUIFactory.CreateLabel(column, "Title", "ENCELADUS HABITAT RESEARCH", 36, 78f);
        CosmicRaysUIFactory.CreateLabel(
            column,
            "Briefing",
            "The six-month mission upgrades the research base into a reliable human habitat. Its objectives cover base and atmosphere inspection; oxygen, water and air processing; reliable energy and heating; SEP technology tests; habitat and research trials; and a final assessment before return.",
            23,
            300f);
        CosmicRaysUIFactory.CreateButton(column, "MissionButton", "View Mission Objectives", () => PushPanel(MissionPanel));
        CosmicRaysUIFactory.CreateButton(column, "BackButton", "Back", PopPanel);
    }

    private void BuildMissionCompletePanel(RectTransform root)
    {
        RectTransform panel = AddPanel(root, MissionCompletePanel);
        RectTransform column = CosmicRaysUIFactory.CreateColumn(panel, "Mission Complete Content");
        CosmicRaysUIFactory.CreateLabel(column, "Title", "MISSION COMPLETE", 42, 90f);
        CosmicRaysUIFactory.CreateLabel(
            column,
            "Summary",
            "All configured mission objectives are complete. The Enceladus research-base assessment is ready.",
            25,
            130f);
        CosmicRaysUIFactory.CreateButton(column, "MainMenuButton", "Return to Main Menu", ReturnToMainMenu);
        CosmicRaysUIFactory.CreateButton(column, "ExitButton", "Exit", ExitGame);
    }

    private void BuildGameOverPanel(RectTransform root)
    {
        RectTransform panel = AddPanel(root, GameOverPanel);
        RectTransform column = CosmicRaysUIFactory.CreateColumn(panel, "Game Over Content");
        CosmicRaysUIFactory.CreateLabel(column, "Title", "MISSION FAILED", 42, 90f);
        _gameOverReasonLabel = CosmicRaysUIFactory.CreateLabel(
            column,
            "Reason",
            "A critical life-support resource was depleted.",
            25,
            110f);
        CosmicRaysUIFactory.CreateButton(column, "RestartButton", "Start a New Mission", StartNewMission);
        CosmicRaysUIFactory.CreateButton(column, "MainMenuButton", "Return to Main Menu", ReturnToMainMenu);
        CosmicRaysUIFactory.CreateButton(column, "ExitButton", "Exit", ExitGame);
    }

    private RectTransform AddPanel(RectTransform root, string id)
    {
        RectTransform panel = CosmicRaysUIFactory.CreatePanel(root, id);
        panel.gameObject.SetActive(false);
        _panels.Add(id, panel);
        return panel;
    }

    private void RefreshResources()
    {
        if (_survival == null)
        {
            return;
        }

        UpdateMeter(_oxygenLabel, _oxygenFill, CosmicResource.Oxygen);
        UpdateMeter(_waterLabel, _waterFill, CosmicResource.Water);
        UpdateMeter(_energyLabel, _energyFill, CosmicResource.Energy);

        var warnings = new List<string>();
        AddLowWarning(warnings, CosmicResource.Oxygen);
        AddLowWarning(warnings, CosmicResource.Water);
        AddLowWarning(warnings, CosmicResource.Energy);
        _warningLabel.text = warnings.Count == 0
            ? "Life-support resources nominal"
            : "WARNING: " + string.Join(" / ", warnings) + " LOW";
        _warningLabel.color = warnings.Count == 0
            ? Color.white
            : new Color(1f, 0.78f, 0.36f, 1f);
    }

    private void UpdateMeter(Text label, Image fill, CosmicResource resource)
    {
        float current = _survival.CurrentFor(resource);
        float minimum = _survival.MinimumFor(resource);
        float maximum = _survival.MaximumFor(resource);
        label.text = resource + ": " + current.ToString("0") + " / " + maximum.ToString("0") +
                     " (min " + minimum.ToString("0") + ")";
        float range = maximum - minimum;
        fill.fillAmount = range > 0f ? Mathf.Clamp01((current - minimum) / range) : 0f;
    }

    private void AddLowWarning(List<string> warnings, CosmicResource resource)
    {
        float minimum = _survival.MinimumFor(resource);
        float maximum = _survival.MaximumFor(resource);
        float range = maximum - minimum;
        float availableFraction = range > 0f
            ? (_survival.CurrentFor(resource) - minimum) / range
            : 0f;
        if (availableFraction <= _survival.WarningThreshold)
        {
            warnings.Add(resource.ToString().ToUpperInvariant());
        }
    }

    private void RefreshInventory()
    {
        if (_inventoryContent == null || _inventory == null)
        {
            return;
        }

        ClearChildren(_inventoryContent);
        List<InventoryItemDefinition> definitions = _inventory.GetDefinitionsWithItems();
        List<InventoryStackData> unresolvedStacks = _inventory.GetUnresolvedStacks();
        if (definitions.Count == 0 && unresolvedStacks.Count == 0)
        {
            CosmicRaysUIFactory.CreateLabel(
                _inventoryContent,
                "EmptyInventory",
                "No supplies are currently carried. Collect supplies from the environment.",
                22,
                100f,
                TextAnchor.MiddleLeft);
            return;
        }

        for (int i = 0; i < definitions.Count; i++)
        {
            InventoryItemDefinition definition = definitions[i];
            string itemId = definition.itemId;
            int count = _inventory.GetCount(itemId);
            string displayName = GetItemDisplayName(definition);
            if (definition.restoresResource != CosmicResource.None)
            {
                CosmicRaysUIFactory.CreateLabel(
                    _inventoryContent,
                    "ItemDescription_" + itemId,
                    displayName + " x" + count + "\n" + GetItemDescription(definition),
                    21,
                    96f,
                    TextAnchor.MiddleLeft);
                Button useButton = CosmicRaysUIFactory.CreateButton(
                    _inventoryContent,
                    "UseItem_" + itemId,
                    "Use " + displayName,
                    () => UseItem(itemId));
                useButton.interactable = definition.restoreAmount > 0f;
            }
            else
            {
                CosmicRaysUIFactory.CreateLabel(
                    _inventoryContent,
                    "ItemDescription_" + itemId,
                    displayName + " x" + count + "\n" + GetItemDescription(definition) + "\nNot consumable",
                    21,
                    120f,
                    TextAnchor.MiddleLeft);
            }
        }

        for (int i = 0; i < unresolvedStacks.Count; i++)
        {
            InventoryStackData stack = unresolvedStacks[i];
            CosmicRaysUIFactory.CreateLabel(
                _inventoryContent,
                "UnresolvedItem_" + stack.itemId,
                stack.itemId + " x" + stack.quantity +
                "\nDefinition unavailable. This saved stack is preserved but cannot currently be used.",
                21,
                120f,
                TextAnchor.MiddleLeft);
        }
    }

    private void RefreshMissionObjectives()
    {
        if (_missionContent == null || _missionProgress == null)
        {
            return;
        }

        ClearChildren(_missionContent);
        if (!_missionProgress.HasObjectives)
        {
            CosmicRaysUIFactory.CreateLabel(
                _missionContent,
                "NoObjectives",
                "No mission objective definitions are loaded. Run the Editor setup tool and confirm the resources were imported.",
                22,
                120f,
                TextAnchor.MiddleLeft);
            return;
        }

        IReadOnlyList<MissionObjectiveDefinition> objectives = _missionProgress.Objectives;
        for (int i = 0; i < objectives.Count; i++)
        {
            MissionObjectiveDefinition objective = objectives[i];
            string state = _missionProgress.IsObjectiveComplete(objective.objectiveId) ? "COMPLETED" : "PENDING";
            string text = "MONTH " + objective.missionMonth + "  |  " + state + "\n" +
                          objective.title + "\n" + objective.description;
            CosmicRaysUIFactory.CreateLabel(
                _missionContent,
                "Objective_" + objective.objectiveId,
                text,
                22,
                130f,
                TextAnchor.MiddleLeft);
        }
    }

    private void UseItem(string itemId)
    {
        string message;
        if (_inventory.TryUse(itemId, _survival, out message))
        {
            ShowFeedback(message);
            SaveProgress();
        }
        else
        {
            ShowFeedback(message);
        }
    }

    private void OnSurvivalChanged()
    {
        RefreshResources();
        ResourcesChanged?.Invoke();
    }

    private void OnInventoryChanged()
    {
        RefreshInventory();
        InventoryChanged?.Invoke();
    }

    private void OnMissionProgressChanged()
    {
        RefreshMissionObjectives();
        MissionProgressChanged?.Invoke();
    }

    private void OnResourceDepleted(CosmicResource resource)
    {
        if (_terminal)
        {
            return;
        }

        ResourceDepleted?.Invoke(resource);
        if (_config.failMissionWhenResourceIsEmpty)
        {
            ShowGameOver(resource + " was depleted. The mission cannot continue.");
        }
        else
        {
            ShowFeedback("WARNING: " + resource + " depleted.");
        }
    }

    private void ShowMissionComplete()
    {
        if (_terminal)
        {
            return;
        }
        SaveProgress();
        ShowTerminalPanel(MissionCompletePanel);
        MissionCompleted?.Invoke();
    }

    private void ShowGameOver(string reason)
    {
        if (_terminal)
        {
            return;
        }
        SaveProgress();
        _gameOverReasonLabel.text = reason;
        ShowTerminalPanel(GameOverPanel);
        MissionFailed?.Invoke(reason);
    }

    private void ShowTerminalPanel(string panelId)
    {
        _terminal = true;
        _panelHistory.Clear();
        foreach (RectTransform panel in _panels.Values)
        {
            panel.gameObject.SetActive(false);
        }
        _activePanelId = panelId;
        _panels[panelId].gameObject.SetActive(true);
        Time.timeScale = 0f;
        ScreenChanged?.Invoke(panelId);
        CosmicRaysUIFactory.Focus(_panels[panelId].GetComponentInChildren<Button>());
    }

    private void PushPanel(string panelId)
    {
        if (_terminal || !_panels.ContainsKey(panelId) || _activePanelId == panelId)
        {
            return;
        }

        if (_activePanelId != null)
        {
            _panels[_activePanelId].gameObject.SetActive(false);
        }
        _panelHistory.Push(_activePanelId);
        _activePanelId = panelId;
        _panels[panelId].gameObject.SetActive(true);
        Time.timeScale = 0f;
        ScreenChanged?.Invoke(panelId);

        if (panelId == InventoryPanel) RefreshInventory();
        if (panelId == MissionPanel) RefreshMissionObjectives();
        SelectFirstControl(_panels[panelId]);
    }

    private void PopPanel()
    {
        if (_activePanelId != null)
        {
            _panels[_activePanelId].gameObject.SetActive(false);
        }

        _activePanelId = _panelHistory.Count > 0 ? _panelHistory.Pop() : null;
        if (_activePanelId != null)
        {
            _panels[_activePanelId].gameObject.SetActive(true);
            SelectFirstControl(_panels[_activePanelId]);
            Time.timeScale = 0f;
            ScreenChanged?.Invoke(_activePanelId);
        }
        else
        {
            Time.timeScale = 1f;
            ScreenChanged?.Invoke("Gameplay");
            CosmicRaysUIFactory.Focus(FindHudButton("Pause"));
        }
    }

    /// <summary>Saves progress and loads the main menu scene.</summary>
    public void ReturnToMainMenu()
    {
        SaveProgress();
        if (!Application.CanStreamedLevelBeLoaded(MainMenuSceneName))
        {
            string error = "MainMenu scene is not in Build Settings. Run Tools/COSMIC RAYS/Setup Scenes and Data.";
            Debug.LogError(error);
            ShowFeedback(error);
            return;
        }
        Time.timeScale = 1f;
        SceneManager.LoadScene(MainMenuSceneName);
    }

    /// <summary>Starts a fresh mission after verifying that the gameplay scene can be loaded.</summary>
    public void StartNewMission()
    {
        if (!Application.CanStreamedLevelBeLoaded(GameplaySceneName))
        {
            Debug.LogError("Gameplay scene is not in Build Settings. Run Tools/COSMIC RAYS/Setup Scenes and Data.");
            ShowFeedback("Gameplay scene is missing from Build Settings.");
            return;
        }

        SurvivalConfig config = CosmicRaysRuntimeContent.LoadSurvivalConfig();
        CosmicRaysSaveData freshData;
        try
        {
            freshData = CosmicRaysSession.CreateNewGame(config);
        }
        catch (ArgumentException exception)
        {
            Debug.LogError("The new mission could not be created. " + exception.Message, this);
            ShowFeedback(exception.Message);
            return;
        }

        string error;
        if (!CosmicRaysSaveSystem.TrySave(freshData, out error))
        {
            Debug.LogError(error);
            ShowFeedback("Could not save the new mission. " + error);
            return;
        }

        CosmicRaysSession.SetPendingSave(freshData, "New mission started.");
        Time.timeScale = 1f;
        SceneManager.LoadScene(GameplaySceneName);
    }

    private void ShowFeedback(string message)
    {
        if (_feedbackLabel != null)
        {
            _feedbackLabel.text = message ?? string.Empty;
        }
    }

    private static void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Destroy(parent.GetChild(i).gameObject);
        }
    }

    private static void SelectFirstControl(RectTransform panel)
    {
        Button firstButton = panel.GetComponentInChildren<Button>();
        if (firstButton != null)
        {
            CosmicRaysUIFactory.Focus(firstButton);
            return;
        }
        CosmicRaysUIFactory.Focus(panel.GetComponentInChildren<Slider>());
    }

    private Button FindHudButton(string name)
    {
        Button[] buttons = _hudPanel.GetComponentsInChildren<Button>();
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i].gameObject.name == name)
            {
                return buttons[i];
            }
        }
        return null;
    }

    private static string GetItemDisplayName(InventoryItemDefinition definition)
    {
        return string.IsNullOrWhiteSpace(definition.displayName) ? definition.itemId : definition.displayName;
    }

    private static string GetItemDescription(InventoryItemDefinition definition)
    {
        return string.IsNullOrWhiteSpace(definition.description)
            ? "No description provided."
            : definition.description;
    }

    private static void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
