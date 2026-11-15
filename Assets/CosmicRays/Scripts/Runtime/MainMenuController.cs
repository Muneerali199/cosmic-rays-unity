using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>Builds and controls the main menu, save continuation, and settings screens.</summary>
public sealed class MainMenuController : MonoBehaviour
{
    private CosmicRaysSaveData _availableSave;
    private RectTransform _menuPanel;
    private RectTransform _settingsPanel;
    private RectTransform _confirmPanel;
    private Text _statusLabel;
    private Button _newGameButton;
    private Button _continueButton;
    private Button _settingsButton;
    private string _saveNotice;
    private bool _hasExistingSave;

    private void Start()
    {
        CosmicRaysSettings.Initialize();
        BuildInterface();
        RefreshContinueState();
        CosmicRaysUIFactory.Focus(_continueButton.interactable ? _continueButton : _newGameButton);
    }

    private void BuildInterface()
    {
        RectTransform root = CosmicRaysUIFactory.CreateCanvasRoot("COSMIC RAYS Main Menu UI");
        _menuPanel = CosmicRaysUIFactory.CreatePanel(root, "Main Menu");
        RectTransform menuColumn = CosmicRaysUIFactory.CreateColumn(_menuPanel, "Menu Content");

        CosmicRaysUIFactory.CreateLabel(menuColumn, "Title", "COSMIC RAYS", 50, 100f);
        CosmicRaysUIFactory.CreateLabel(
            menuColumn,
            "Subtitle",
            "Six-month research mission on Enceladus",
            26,
            54f);
        _newGameButton = CosmicRaysUIFactory.CreateButton(menuColumn, "NewGameButton", "New Game", BeginNewGame);
        _continueButton = CosmicRaysUIFactory.CreateButton(menuColumn, "ContinueButton", "Continue", ContinueMission);
        _settingsButton = CosmicRaysUIFactory.CreateButton(menuColumn, "SettingsButton", "Settings", OpenSettings);
        CosmicRaysUIFactory.CreateButton(menuColumn, "ExitButton", "Exit", ExitApplication);
        _statusLabel = CosmicRaysUIFactory.CreateLabel(menuColumn, "Status", string.Empty, 20, 72f);

        _settingsPanel = CosmicRaysUIFactory.CreatePanel(root, "Settings");
        CosmicRaysSettingsUI.Build(_settingsPanel, OnSettingsBack);
        _settingsPanel.gameObject.SetActive(false);

        _confirmPanel = CosmicRaysUIFactory.CreatePanel(root, "New Game Confirmation");
        RectTransform confirmColumn = CosmicRaysUIFactory.CreateColumn(_confirmPanel, "Confirmation Content");
        CosmicRaysUIFactory.CreateLabel(confirmColumn, "ConfirmTitle", "START A NEW MISSION?", 38, 72f);
        CosmicRaysUIFactory.CreateLabel(
            confirmColumn,
            "ConfirmText",
            "Starting a new mission will replace the existing local progress.",
            24,
            90f);
        CosmicRaysUIFactory.CreateButton(confirmColumn, "ConfirmNewGameButton", "Start New Mission", StartNewGame);
        CosmicRaysUIFactory.CreateButton(confirmColumn, "CancelNewGameButton", "Cancel", CancelNewGame);
        _confirmPanel.gameObject.SetActive(false);

    }

    private void RefreshContinueState()
    {
        _hasExistingSave = CosmicRaysSaveSystem.HasAnySave;
        string loadMessage;
        if (CosmicRaysSaveSystem.TryLoad(out _availableSave, out loadMessage))
        {
            _saveNotice = loadMessage;
            _continueButton.interactable = true;
            if (!string.IsNullOrEmpty(loadMessage))
            {
                SetStatus(loadMessage);
            }
        }
        else
        {
            _availableSave = null;
            _saveNotice = null;
            _continueButton.interactable = false;
            if (_hasExistingSave && !string.IsNullOrEmpty(loadMessage))
            {
                SetStatus(loadMessage);
            }
        }
    }

    /// <summary>Begins a new-game flow, requesting confirmation before replacing an existing save.</summary>
    public void BeginNewGame()
    {
        _hasExistingSave = CosmicRaysSaveSystem.HasAnySave;
        if (_hasExistingSave)
        {
            _menuPanel.gameObject.SetActive(false);
            _confirmPanel.gameObject.SetActive(true);
            CosmicRaysUIFactory.Focus(_confirmPanel.GetComponentInChildren<Button>());
            return;
        }
        StartNewGame();
    }

    /// <summary>Opens the settings overlay.</summary>
    public void OpenSettings()
    {
        _menuPanel.gameObject.SetActive(false);
        _settingsPanel.gameObject.SetActive(true);
        CosmicRaysUIFactory.Focus(_settingsPanel.GetComponentInChildren<Slider>());
    }

    /// <summary>Exits Play Mode in the Editor or requests application shutdown in a build.</summary>
    public void ExitApplication()
    {
        ExitGame();
    }

    private void StartNewGame()
    {
        if (!Application.CanStreamedLevelBeLoaded(CosmicRaysGameplayController.GameplaySceneName))
        {
            string error = "Gameplay scene is not in Build Settings. Run Tools/COSMIC RAYS/Setup Scenes and Data.";
            Debug.LogError(error);
            _confirmPanel.gameObject.SetActive(false);
            _menuPanel.gameObject.SetActive(true);
            SetStatus(error);
            CosmicRaysUIFactory.Focus(_newGameButton);
            return;
        }

        SurvivalConfig config = CosmicRaysRuntimeContent.LoadSurvivalConfig();
        CosmicRaysSaveData newSave;
        try
        {
            newSave = CosmicRaysSession.CreateNewGame(config);
        }
        catch (System.ArgumentException exception)
        {
            Debug.LogError("The new mission could not be created. " + exception.Message, this);
            _confirmPanel.gameObject.SetActive(false);
            _menuPanel.gameObject.SetActive(true);
            SetStatus(exception.Message);
            CosmicRaysUIFactory.Focus(_newGameButton);
            return;
        }

        string notice;
        if (!CosmicRaysSaveSystem.TrySave(newSave, out notice))
        {
            Debug.LogError(notice);
            _confirmPanel.gameObject.SetActive(false);
            _menuPanel.gameObject.SetActive(true);
            SetStatus(notice);
            CosmicRaysUIFactory.Focus(_newGameButton);
            return;
        }

        notice = "New mission started.";
        CosmicRaysSession.SetPendingSave(newSave, notice);
        LoadGameplay();
    }

    /// <summary>Loads the existing saved mission when a valid save is available.</summary>
    public void ContinueMission()
    {
        if (_availableSave == null)
        {
            RefreshContinueState();
            return;
        }

        CosmicRaysSession.SetPendingSave(
            _availableSave,
            string.IsNullOrEmpty(_saveNotice) ? "Mission progress loaded." : _saveNotice);
        LoadGameplay();
    }

    private void OnSettingsBack()
    {
        _settingsPanel.gameObject.SetActive(false);
        _menuPanel.gameObject.SetActive(true);
        CosmicRaysUIFactory.Focus(_settingsButton);
    }

    private void CancelNewGame()
    {
        _confirmPanel.gameObject.SetActive(false);
        _menuPanel.gameObject.SetActive(true);
        CosmicRaysUIFactory.Focus(_newGameButton);
    }

    private void LoadGameplay()
    {
        Time.timeScale = 1f;
        if (!Application.CanStreamedLevelBeLoaded(CosmicRaysGameplayController.GameplaySceneName))
        {
            string error = "Gameplay scene is not in Build Settings. Run Tools/COSMIC RAYS/Setup Scenes and Data.";
            Debug.LogError(error);
            _confirmPanel.gameObject.SetActive(false);
            _settingsPanel.gameObject.SetActive(false);
            _menuPanel.gameObject.SetActive(true);
            SetStatus(error);
            return;
        }
        _confirmPanel.gameObject.SetActive(false);
        _settingsPanel.gameObject.SetActive(false);
        SceneManager.LoadScene(CosmicRaysGameplayController.GameplaySceneName);
    }

    private void SetStatus(string message)
    {
        if (_statusLabel != null)
        {
            _statusLabel.text = message;
        }
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
