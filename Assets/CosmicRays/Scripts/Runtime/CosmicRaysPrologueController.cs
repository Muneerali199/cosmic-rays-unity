using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class CosmicRaysPrologueController : MonoBehaviour
{
    public const string PrologueSceneName = "Prologue";

    private const float WalkSpeed = 3.35f;
    private const float RunSpeed = 4.85f;
    private const float InteractionDistance = 2.65f;
    private const float MouseSensitivity = 0.075f;
    private const float EyeHeight = 1.64f;

    private static readonly Color VoidColor = new Color(0.004f, 0.012f, 0.022f, 1f);
    private static readonly Color CyanColor = new Color(0.42f, 0.9f, 0.94f, 1f);
    private static readonly Color AmberColor = new Color(1f, 0.66f, 0.22f, 1f);
    private static readonly Color MagentaColor = new Color(0.93f, 0.32f, 0.66f, 1f);
    private static readonly Color DangerColor = new Color(1f, 0.35f, 0.25f, 1f);

    private enum PrologueStage
    {
        Title,
        Dinner,
        Bedroom,
        Lab,
        Puzzle,
        Document,
        Ending
    }

    private enum DocumentMode
    {
        None,
        MissionFile,
        Note
    }

    private sealed class DialogueBeat
    {
        public string Speaker;
        public string Text;
        public string FirstChoice;
        public string FirstReply;
        public string SecondChoice;
        public string SecondReply;

        public bool HasChoices
        {
            get { return !string.IsNullOrEmpty(FirstChoice) && !string.IsNullOrEmpty(SecondChoice); }
        }

        public DialogueBeat(string speaker, string text)
        {
            Speaker = speaker;
            Text = text;
        }

        public DialogueBeat(
            string speaker,
            string text,
            string firstChoice,
            string firstReply,
            string secondChoice,
            string secondReply)
        {
            Speaker = speaker;
            Text = text;
            FirstChoice = firstChoice;
            FirstReply = firstReply;
            SecondChoice = secondChoice;
            SecondReply = secondReply;
        }
    }

    private readonly List<DialogueBeat> _dinnerStory = new List<DialogueBeat>();
    private readonly List<Button> _puzzleButtons = new List<Button>();
    private readonly MagneticRoutingPuzzle _puzzle = new MagneticRoutingPuzzle();

    private PrologueStage _stage;
    private DocumentMode _documentMode;
    private Transform _worldRoot;
    private Camera _sceneCamera;
    private CosmicRaysPrologueUI _ui;
    private AudioSource _ambientSource;
    private Coroutine _fadeRoutine;

    private GameObject _player;
    private CharacterController _characterController;
    private Transform _labDoor;
    private Transform _terminal;
    private GameObject _noteObject;

    private float _yaw;
    private float _pitch;
    private float _verticalVelocity;
    private float _cinematicTime;
    private bool _cursorLocked;

    private int _dialogueIndex;
    private int _pendingDialogueIndex = -1;
    private bool _showingChoiceReply;
    private bool _puzzleSolved;
    private bool _missionRead;

    private void Start()
    {
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
        QualitySettings.shadowDistance = 42f;
        BuildStory();
        ConfigureCamera();
        CreatePersistentSystems();
        _ui = CosmicRaysPrologueUI.Build(
            BeginDinner,
            AdvanceDialogue,
            RunParticleTest,
            ResetPuzzle,
            UnlockMissionFile,
            ClosePuzzle,
            CloseMissionFile,
            ShowEnding,
            BeginEnceladusMission,
            ShowTitle);
        BuildPuzzleButtons();
        ShowTitle();
    }

    private void OnDestroy()
    {
        if (!Application.isBatchMode)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void Update()
    {
        if (_stage == PrologueStage.Puzzle || _stage == PrologueStage.Document)
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                if (_stage == PrologueStage.Puzzle)
                {
                    ClosePuzzle();
                }
                else if (_documentMode == DocumentMode.MissionFile)
                {
                    CloseMissionFile();
                }
                else
                {
                    CloseNote();
                }
            }
            return;
        }

        if (_stage == PrologueStage.Bedroom || _stage == PrologueStage.Lab)
        {
            UpdateExploration();
        }
    }

    private void LateUpdate()
    {
        if (_sceneCamera == null)
        {
            return;
        }

        if (_player != null &&
            (_stage == PrologueStage.Bedroom ||
             _stage == PrologueStage.Lab ||
             _stage == PrologueStage.Puzzle ||
             _stage == PrologueStage.Document))
        {
            _sceneCamera.transform.position = _player.transform.position + Vector3.up * EyeHeight;
            _sceneCamera.transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
            return;
        }

        _cinematicTime += Time.deltaTime;
        switch (_stage)
        {
            case PrologueStage.Title:
                CosmicRaysPrologueWorlds.PositionTitleCamera(_sceneCamera, _cinematicTime);
                break;
            case PrologueStage.Dinner:
                CosmicRaysPrologueWorlds.PositionDinnerCamera(_sceneCamera, _cinematicTime);
                break;
            case PrologueStage.Ending:
                CosmicRaysPrologueWorlds.PositionEndingCamera(_sceneCamera, _cinematicTime);
                break;
        }
    }

    private void BuildStory()
    {
        _dinnerStory.Clear();
        _dinnerStory.Add(new DialogueBeat(
            "Alice",
            "Areeba, what if I told you we found a way to catch a storm from the Sun?"));
        _dinnerStory.Add(new DialogueBeat("Areeba", "Catch sunlight?"));
        _dinnerStory.Add(new DialogueBeat(
            "Alice",
            "Not light. Particles. Before I explain, tell me: what is the fastest thing we know?",
            "The speed of light",
            "Exactly. Nothing carrying information can outrun light, but some particles travel incredibly close to it.",
            "The speed of sound",
            "Sound is fast, but light is much faster. Some particles travel incredibly close to light speed."));
        _dinnerStory.Add(new DialogueBeat(
            "Alice",
            "Cosmic rays are high-energy particles that mostly reach us from beyond our Solar System."));
        _dinnerStory.Add(new DialogueBeat("Areeba", "Then what is the storm from the Sun?"));
        _dinnerStory.Add(new DialogueBeat(
            "Alice",
            "Solar energetic particles, or SEPs. Solar eruptions can accelerate them to extraordinary energies. SPA-077 was built to study them."));
        _dinnerStory.Add(new DialogueBeat("Areeba", "SPA-077? Is that your secret project?"));
        _dinnerStory.Add(new DialogueBeat(
            "Alice",
            "Tomorrow. For now, get some sleep... and promise me you will not enter the lab."));
    }

    private void ConfigureCamera()
    {
        _sceneCamera = Camera.main;
        if (_sceneCamera == null)
        {
            Camera[] cameras = FindObjectsByType<Camera>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            if (cameras.Length > 0)
            {
                _sceneCamera = cameras[0];
            }
        }

        if (_sceneCamera == null)
        {
            var cameraObject = new GameObject("Prologue Camera", typeof(Camera));
            _sceneCamera = cameraObject.GetComponent<Camera>();
        }

        _sceneCamera.gameObject.tag = "MainCamera";
        _sceneCamera.transform.SetParent(null);
        _sceneCamera.orthographic = false;
        _sceneCamera.fieldOfView = 60f;
        _sceneCamera.nearClipPlane = 0.04f;
        _sceneCamera.farClipPlane = 300f;
        _sceneCamera.clearFlags = CameraClearFlags.Skybox;
        _sceneCamera.backgroundColor = VoidColor;
        _sceneCamera.allowHDR = true;
        _sceneCamera.allowMSAA = true;

        if (FindAnyObjectByType<AudioListener>() == null)
        {
            _sceneCamera.gameObject.AddComponent<AudioListener>();
        }

        UniversalAdditionalCameraData cameraData = _sceneCamera.GetUniversalAdditionalCameraData();
        cameraData.renderPostProcessing = true;
        cameraData.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
        cameraData.antialiasingQuality = AntialiasingQuality.High;
        cameraData.renderShadows = true;

        Camera[] allCameras = FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int index = 0; index < allCameras.Length; index++)
        {
            if (allCameras[index] != _sceneCamera)
            {
                allCameras[index].enabled = false;
            }
        }

        _worldRoot = new GameObject("Generated Prologue World").transform;

        Light[] lights = FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int index = 0; index < lights.Length; index++)
        {
            lights[index].enabled = false;
        }
    }

    private void CreatePersistentSystems()
    {
        var volumeObject = new GameObject("COSMIC RAYS Post Processing");
        volumeObject.transform.SetParent(transform, false);
        Volume volume = volumeObject.AddComponent<Volume>();
        volume.isGlobal = true;
        volume.priority = 3f;

        var profile = ScriptableObject.CreateInstance<VolumeProfile>();
        profile.name = "COSMIC RAYS Cinematic Grade";
        volume.profile = profile;

        Bloom bloom = profile.Add<Bloom>(true);
        bloom.intensity.Override(0.72f);
        bloom.threshold.Override(0.86f);
        bloom.scatter.Override(0.62f);
        bloom.tint.Override(new Color(0.82f, 0.92f, 1f));

        Vignette vignette = profile.Add<Vignette>(true);
        vignette.intensity.Override(0.29f);
        vignette.smoothness.Override(0.44f);
        vignette.color.Override(new Color(0.003f, 0.008f, 0.016f));

        ChromaticAberration aberration = profile.Add<ChromaticAberration>(true);
        aberration.intensity.Override(0.075f);

        FilmGrain grain = profile.Add<FilmGrain>(true);
        grain.type.Override(FilmGrainLookup.Medium1);
        grain.intensity.Override(0.18f);
        grain.response.Override(0.72f);

        ColorAdjustments grade = profile.Add<ColorAdjustments>(true);
        grade.postExposure.Override(0.12f);
        grade.contrast.Override(13f);
        grade.saturation.Override(7f);
        grade.colorFilter.Override(new Color(1f, 0.975f, 0.94f));

        Tonemapping tonemapping = profile.Add<Tonemapping>(true);
        tonemapping.mode.Override(TonemappingMode.ACES);

        var audioObject = new GameObject("COSMIC RAYS Ambient Audio");
        audioObject.transform.SetParent(transform, false);
        _ambientSource = audioObject.AddComponent<AudioSource>();
        _ambientSource.playOnAwake = false;
        _ambientSource.loop = true;
        _ambientSource.spatialBlend = 0f;
    }

    private void ShowTitle()
    {
        Time.timeScale = 1f;
        _stage = PrologueStage.Title;
        _documentMode = DocumentMode.None;
        _puzzleSolved = false;
        _missionRead = false;
        _puzzle.Reset();
        _dialogueIndex = 0;
        _pendingDialogueIndex = -1;
        _showingChoiceReply = false;
        _pitch = 0f;
        _verticalVelocity = 0f;
        _cinematicTime = 0f;

        DestroyPlayer();
        ClearWorld();
        CosmicRaysPrologueWorlds.BuildTitle(_worldRoot);

        _ui.HideAllPanels();
        _ui.HudPanel.gameObject.SetActive(false);
        _ui.TitlePanel.gameObject.SetActive(true);
        _ui.SetInteraction(string.Empty);
        _ui.SetCinematic(true);
        SetCursorLocked(false);
        FocusNamed(_ui.TitlePanel, "BeginPrologueButton");
        PlayAmbient(true);
        SetFade(1f);
        FadeIn(1.15f);
    }

    private void BeginDinner()
    {
        _stage = PrologueStage.Dinner;
        _documentMode = DocumentMode.None;
        _cinematicTime = 0f;
        DestroyPlayer();
        ClearWorld();
        CosmicRaysPrologueWorlds.BuildDinner(_worldRoot);

        _ui.HideAllPanels();
        _ui.HudPanel.gameObject.SetActive(true);
        _ui.DialoguePanel.gameObject.SetActive(true);
        _ui.SetHud(
            "PROLOGUE 01 / GREENBELT, MARYLAND / 04 OCT 2028 / 19:42",
            "OBJECTIVE // LISTEN TO ALICE",
            "RESIDENCE // DINNER",
            false,
            false);
        _ui.SetInteraction(string.Empty);
        _ui.SetCinematic(true);
        SetCursorLocked(false);
        PlayAmbient(false);

        _dialogueIndex = 0;
        _pendingDialogueIndex = -1;
        _showingChoiceReply = false;
        RenderDialogue();
        SetFade(1f);
        FadeIn(0.85f);
    }

    private void RenderDialogue()
    {
        if (_dialogueIndex < 0 || _dialogueIndex >= _dinnerStory.Count)
        {
            EnterBedroom();
            return;
        }

        DialogueBeat beat = _dinnerStory[_dialogueIndex];
        _ui.SpeakerLabel.text = beat.Speaker.ToUpperInvariant();
        _ui.DialogueLabel.text = beat.Text;
        _ui.ClearChoices();
        _ui.ContinueButton.gameObject.SetActive(!beat.HasChoices);

        if (beat.HasChoices)
        {
            Button first = CosmicRaysPrologueUI.CreateChoiceButton(
                _ui.ChoiceContainer,
                "ChoiceOne",
                beat.FirstChoice,
                () => SelectDialogueChoice(beat.FirstReply));
            Button second = CosmicRaysPrologueUI.CreateChoiceButton(
                _ui.ChoiceContainer,
                "ChoiceTwo",
                beat.SecondChoice,
                () => SelectDialogueChoice(beat.SecondReply));
            CosmicRaysPrologueUI.StyleButton(first, new Color(0.6f, 0.32f, 0.08f, 1f));
            CosmicRaysPrologueUI.StyleButton(second, new Color(0.08f, 0.32f, 0.42f, 1f));
            Focus(first);
        }
        else
        {
            Focus(_ui.ContinueButton);
        }
    }

    private void SelectDialogueChoice(string reply)
    {
        _showingChoiceReply = true;
        _pendingDialogueIndex = _dialogueIndex + 1;
        _ui.SpeakerLabel.text = "ALICE";
        _ui.DialogueLabel.text = reply;
        _ui.ClearChoices();
        _ui.ContinueButton.gameObject.SetActive(true);
        Focus(_ui.ContinueButton);
    }

    private void AdvanceDialogue()
    {
        if (_showingChoiceReply)
        {
            _dialogueIndex = _pendingDialogueIndex;
            _showingChoiceReply = false;
            _pendingDialogueIndex = -1;
        }
        else
        {
            _dialogueIndex++;
        }
        RenderDialogue();
    }

    private void EnterBedroom()
    {
        _stage = PrologueStage.Bedroom;
        _documentMode = DocumentMode.None;
        _cinematicTime = 0f;
        DestroyPlayer();
        ClearWorld();
        CosmicRaysPrologueWorlds.ExplorationAnchors anchors =
            CosmicRaysPrologueWorlds.BuildBedroom(_worldRoot);
        _labDoor = anchors.Door;
        CreatePlayer(anchors.SpawnPosition, anchors.SpawnYaw);

        _ui.HideAllPanels();
        _ui.HudPanel.gameObject.SetActive(true);
        _ui.SetHud(
            "PROLOGUE 02 / AREEBA'S ROOM / 22:42",
            "OBJECTIVE // FIND MOM'S PRIVATE LAB",
            "RESIDENCE // SECURE",
            true,
            true);
        _ui.SetInteraction(string.Empty);
        _ui.SetCinematic(false);
        PlayAmbient(false);
        SetCursorLocked(true);
        ClearUiFocus();
        SetFade(1f);
        FadeIn(0.85f);
        RefreshInteractionPrompt();
    }

    private void EnterLab()
    {
        _stage = PrologueStage.Lab;
        _documentMode = DocumentMode.None;
        _cinematicTime = 0f;
        DestroyPlayer();
        ClearWorld();
        CosmicRaysPrologueWorlds.ExplorationAnchors anchors =
            CosmicRaysPrologueWorlds.BuildLab(_worldRoot);
        _terminal = anchors.Terminal;
        _noteObject = anchors.Note != null ? anchors.Note.gameObject : null;
        CreatePlayer(anchors.SpawnPosition, anchors.SpawnYaw);

        _ui.HideAllPanels();
        _ui.HudPanel.gameObject.SetActive(true);
        _ui.SetHud(
            "PROLOGUE 03 / ALICE'S LAB / RESTRICTED",
            "OBJECTIVE // ACCESS THE SPA-077 TERMINAL",
            "CONTAINMENT // STABLE",
            true,
            true);
        _ui.SetInteraction(string.Empty);
        _ui.SetCinematic(false);
        PlayAmbient(false);
        SetCursorLocked(true);
        ClearUiFocus();
        SetFade(1f);
        FadeIn(0.9f);
        RefreshInteractionPrompt();
    }

    private void ShowPuzzle()
    {
        _stage = PrologueStage.Puzzle;
        _documentMode = DocumentMode.None;
        _ui.HideAllPanels();
        _ui.HudPanel.gameObject.SetActive(true);
        _ui.PuzzlePanel.gameObject.SetActive(true);
        _ui.SetHud(
            "SPA-077 / MAGNETIC CALIBRATION",
            "OBJECTIVE // BUILD A STABLE PARTICLE ROUTE",
            "TERMINAL // LOCAL ACCESS",
            false,
            false);
        _ui.SetInteraction(string.Empty);
        _ui.SetCinematic(false);
        SetCursorLocked(false);
        ClearUiFocus();

        _ui.PuzzleFeedback.text = _puzzleSolved
            ? "Stable route confirmed. The mission file is available."
            : "Route not tested. Select any coil to rotate it clockwise.";
        _ui.PuzzleFeedback.color = _puzzleSolved ? CyanColor : AmberColor;
        _ui.RunTestButton.gameObject.SetActive(!_puzzleSolved);
        _ui.ResetPuzzleButton.gameObject.SetActive(!_puzzleSolved);
        _ui.UnlockFileButton.gameObject.SetActive(_puzzleSolved);
        RefreshPuzzleButtons(null);
        if (_puzzleSolved)
        {
            Focus(_ui.UnlockFileButton);
        }
        else
        {
            FocusFirstRotatablePuzzleButton();
        }
    }

    private void BuildPuzzleButtons()
    {
        _puzzleButtons.Clear();
        for (int index = 0; index < _puzzle.Tiles.Count; index++)
        {
            int tileIndex = index;
            MagneticRoutingPuzzle.Tile tile = _puzzle.Tiles[tileIndex];
            Button button = CosmicRaysUIFactory.CreateButton(
                _ui.PuzzleGrid,
                "PuzzleTile" + tile.Id,
                tile.Id,
                () => RotatePuzzleTile(tileIndex),
                86f);
            _puzzleButtons.Add(button);
        }

        RefreshPuzzleButtons(null);
    }

    private void RotatePuzzleTile(int tileIndex)
    {
        if (_puzzleSolved || tileIndex < 0 || tileIndex >= _puzzle.Tiles.Count)
        {
            return;
        }

        MagneticRoutingPuzzle.Tile tile = _puzzle.Tiles[tileIndex];
        if (tile.Type == MagneticRoutingPuzzle.TileType.Core)
        {
            _ui.PuzzleFeedback.text =
                "B2 is the protected research core. It cannot carry a particle. Route around it.";
            _ui.PuzzleFeedback.color = MagentaColor;
            RefreshPuzzleButtons(null);
            Focus(_puzzleButtons[tileIndex]);
            return;
        }

        if (!_puzzle.Rotate(tileIndex))
        {
            return;
        }

        _ui.PuzzleFeedback.text =
            "Coil " + tile.Id + " rotated. Connections: " +
            MagneticRoutingPuzzle.DescribeConnections(tile) + ".";
        _ui.PuzzleFeedback.color = AmberColor;
        RefreshPuzzleButtons(null);
        Focus(_puzzleButtons[tileIndex]);
    }

    private void RunParticleTest()
    {
        if (_puzzleSolved)
        {
            return;
        }

        MagneticRoutingPuzzle.RouteResult result = _puzzle.Trace();
        _ui.PuzzleFeedback.text = result.Message;
        _ui.PuzzleFeedback.color = result.Success ? CyanColor : DangerColor;
        RefreshPuzzleButtons(result);

        if (!result.Success)
        {
            FocusFirstRotatablePuzzleButton();
            return;
        }

        _puzzleSolved = true;
        _ui.RunTestButton.gameObject.SetActive(false);
        _ui.ResetPuzzleButton.gameObject.SetActive(false);
        _ui.UnlockFileButton.gameObject.SetActive(true);
        RefreshPuzzleButtons(result);
        _ui.SetHud(
            "SPA-077 / MAGNETIC CALIBRATION",
            "OBJECTIVE // OPEN THE SPA-077 FILE",
            "CONTAINMENT // STABLE",
            false,
            false);
        Focus(_ui.UnlockFileButton);
    }

    private void ResetPuzzle()
    {
        if (_puzzleSolved)
        {
            return;
        }

        _puzzle.Reset();
        _ui.PuzzleFeedback.text = "Coils reset. Build a route from source to collector.";
        _ui.PuzzleFeedback.color = AmberColor;
        _ui.RunTestButton.gameObject.SetActive(true);
        _ui.ResetPuzzleButton.gameObject.SetActive(true);
        _ui.UnlockFileButton.gameObject.SetActive(false);
        RefreshPuzzleButtons(null);
        FocusFirstRotatablePuzzleButton();
    }

    private void RefreshPuzzleButtons(MagneticRoutingPuzzle.RouteResult route)
    {
        for (int index = 0; index < _puzzleButtons.Count; index++)
        {
            Button button = _puzzleButtons[index];
            MagneticRoutingPuzzle.Tile tile = _puzzle.Tiles[index];
            Text label = button.GetComponentInChildren<Text>();
            bool core = tile.Type == MagneticRoutingPuzzle.TileType.Core;
            label.fontSize = 19;
            label.text = core
                ? "B2\nPROTECTED CORE"
                : tile.Id + "\n" + MagneticRoutingPuzzle.DescribeConnections(tile);
            button.interactable = true;
            CosmicRaysPrologueUI.StyleButton(
                button,
                core
                    ? new Color(0.5f, 0.09f, 0.28f, 1f)
                    : new Color(0.06f, 0.24f, 0.3f, 1f));
        }

        if (route == null)
        {
            return;
        }

        Color routeColor = route.Success ? CyanColor : DangerColor;
        for (int pathIndex = 0; pathIndex < route.Path.Count; pathIndex++)
        {
            int tileIndex = route.Path[pathIndex];
            if (tileIndex >= 0 && tileIndex < _puzzleButtons.Count)
            {
                CosmicRaysPrologueUI.StyleButton(_puzzleButtons[tileIndex], routeColor);
            }
        }

        if (!route.Success && route.Message.Contains("core"))
        {
            CosmicRaysPrologueUI.StyleButton(_puzzleButtons[4], MagentaColor);
        }
    }

    private void FocusFirstRotatablePuzzleButton()
    {
        for (int index = 0; index < _puzzleButtons.Count; index++)
        {
            if (_puzzle.Tiles[index].Rotatable)
            {
                Focus(_puzzleButtons[index]);
                return;
            }
        }
    }

    private void ClosePuzzle()
    {
        _ui.PuzzlePanel.gameObject.SetActive(false);
        _stage = PrologueStage.Lab;
        _documentMode = DocumentMode.None;
        _ui.HudPanel.gameObject.SetActive(true);
        _ui.SetHud(
            "PROLOGUE 03 / ALICE'S LAB / RESTRICTED",
            _puzzleSolved
                ? "OBJECTIVE // OPEN THE SPA-077 FILE"
                : "OBJECTIVE // ACCESS THE SPA-077 TERMINAL",
            "CONTAINMENT // STABLE",
            true,
            true);
        _ui.SetInteraction(string.Empty);
        _ui.SetCinematic(false);
        SetCursorLocked(true);
        ClearUiFocus();
        RefreshInteractionPrompt();
    }

    private void UnlockMissionFile()
    {
        if (!_puzzleSolved)
        {
            return;
        }

        _ui.PuzzlePanel.gameObject.SetActive(false);
        ShowMissionFile();
    }

    private void ShowMissionFile()
    {
        _stage = PrologueStage.Document;
        _documentMode = DocumentMode.MissionFile;
        _ui.HideAllPanels();
        _ui.HudPanel.gameObject.SetActive(true);
        _ui.MissionFilePanel.gameObject.SetActive(true);
        _ui.SetHud(
            "SPA-077 / DECLASSIFIED",
            "OBJECTIVE // READ MOM'S RESEARCH",
            "FILE ACCESS // GRANTED",
            false,
            false);
        _ui.SetInteraction(string.Empty);
        _ui.SetCinematic(false);
        SetCursorLocked(false);
        FocusNamed(_ui.MissionFilePanel, "CloseFileButton");
    }

    private void CloseMissionFile()
    {
        _missionRead = true;
        _documentMode = DocumentMode.None;
        _ui.MissionFilePanel.gameObject.SetActive(false);
        _stage = PrologueStage.Lab;
        _ui.HudPanel.gameObject.SetActive(true);
        if (_noteObject != null)
        {
            _noteObject.SetActive(true);
        }
        _ui.SetHud(
            "PROLOGUE 03 / ALICE'S LAB / RESTRICTED",
            "OBJECTIVE // FIND THE NOTE BENEATH ALICE'S DESK",
            "CONTAINMENT // STABLE",
            true,
            true);
        _ui.SetInteraction(string.Empty);
        _ui.SetCinematic(false);
        SetCursorLocked(true);
        ClearUiFocus();
        RefreshInteractionPrompt();
    }

    private void ShowNote()
    {
        _stage = PrologueStage.Document;
        _documentMode = DocumentMode.Note;
        _ui.HideAllPanels();
        _ui.HudPanel.gameObject.SetActive(false);
        _ui.NotePanel.gameObject.SetActive(true);
        _ui.SetInteraction(string.Empty);
        _ui.SetCinematic(false);
        SetCursorLocked(false);
        FocusNamed(_ui.NotePanel, "FoldNoteButton");
    }

    private void CloseNote()
    {
        _documentMode = DocumentMode.None;
        _ui.NotePanel.gameObject.SetActive(false);
        _stage = PrologueStage.Lab;
        _ui.HudPanel.gameObject.SetActive(true);
        _ui.SetHud(
            "PROLOGUE 03 / ALICE'S LAB / RESTRICTED",
            "OBJECTIVE // FIND THE NOTE BENEATH ALICE'S DESK",
            "CONTAINMENT // STABLE",
            true,
            true);
        _ui.SetInteraction(string.Empty);
        _ui.SetCinematic(false);
        SetCursorLocked(true);
        ClearUiFocus();
        RefreshInteractionPrompt();
    }

    private void ShowEnding()
    {
        _stage = PrologueStage.Ending;
        _documentMode = DocumentMode.None;
        _cinematicTime = 0f;
        DestroyPlayer();
        ClearWorld();
        CosmicRaysPrologueWorlds.BuildEnding(_worldRoot);

        _ui.HideAllPanels();
        _ui.HudPanel.gameObject.SetActive(true);
        _ui.EndingPanel.gameObject.SetActive(true);
        _ui.SetHud(
            "CHAPTER 01 / ENCELADUS RELAY / SATURN",
            "SIGNAL SOURCE // ENCELADUS SOUTH POLAR PLUME",
            "RELAY 07 // SIGNAL LOCKED",
            false,
            false);
        _ui.SetInteraction(string.Empty);
        _ui.SetCinematic(true);
        _ui.EndingMessage.text = "The transmission carries one word in Alice's voice: Areeba.";
        _ui.EndingMessage.color = new Color(0.82f, 0.88f, 0.9f);
        SetCursorLocked(false);
        FocusNamed(_ui.EndingPanel, "BeginMissionButton");
        PlayAmbient(true);
        SetFade(1f);
        FadeIn(1.1f);
    }

    private void BeginEnceladusMission()
    {
        if (!Application.CanStreamedLevelBeLoaded(CosmicRaysGameplayController.GameplaySceneName))
        {
            _ui.EndingMessage.text =
                "Gameplay scene is not ready. Run Tools > COSMIC RAYS > Build Playable Project.";
            _ui.EndingMessage.color = DangerColor;
            return;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(CosmicRaysGameplayController.GameplaySceneName);
    }

    private void UpdateExploration()
    {
        if (!_cursorLocked &&
            Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            SetCursorLocked(true);
        }

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            SetCursorLocked(!_cursorLocked);
        }

        if (_cursorLocked)
        {
            UpdateLook();
            UpdateMovement();
        }

        bool keyboardInteract = Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
        bool gamepadInteract = Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame;
        if (keyboardInteract || gamepadInteract)
        {
            TryInteract();
        }

        RefreshInteractionPrompt();
    }

    private void UpdateLook()
    {
        Vector2 look = Vector2.zero;
        if (Mouse.current != null)
        {
            look += Mouse.current.delta.ReadValue() * MouseSensitivity;
        }

        if (Gamepad.current != null)
        {
            look += Gamepad.current.rightStick.ReadValue() * 2.1f;
        }

        _yaw += look.x;
        _pitch = Mathf.Clamp(_pitch - look.y, -78f, 78f);
        if (_yaw > 360f || _yaw < -360f)
        {
            _yaw = Mathf.Repeat(_yaw, 360f);
        }
    }

    private void UpdateMovement()
    {
        if (_characterController == null)
        {
            return;
        }

        Vector2 input = ReadMovement();
        Vector3 forward = Quaternion.Euler(0f, _yaw, 0f) * Vector3.forward;
        Vector3 right = Quaternion.Euler(0f, _yaw, 0f) * Vector3.right;
        Vector3 direction = forward * input.y + right * input.x;
        if (direction.sqrMagnitude > 1f)
        {
            direction.Normalize();
        }

        bool running = Keyboard.current != null &&
                       (Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed);
        float speed = running ? RunSpeed : WalkSpeed;

        if (_characterController.isGrounded && _verticalVelocity < 0f)
        {
            _verticalVelocity = -1.2f;
        }
        else
        {
            _verticalVelocity -= 19.6f * Time.deltaTime;
        }

        Vector3 velocity = direction * speed + Vector3.up * _verticalVelocity;
        _characterController.Move(velocity * Time.deltaTime);
        _player.transform.rotation = Quaternion.Euler(0f, _yaw, 0f);
    }

    private Vector2 ReadMovement()
    {
        Vector2 movement = Vector2.zero;
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) movement.y += 1f;
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) movement.y -= 1f;
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) movement.x -= 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) movement.x += 1f;
        }

        Gamepad gamepad = Gamepad.current;
        if (gamepad != null)
        {
            movement += gamepad.leftStick.ReadValue();
        }
        return Vector2.ClampMagnitude(movement, 1f);
    }

    private void TryInteract()
    {
        if (_player == null)
        {
            return;
        }

        if (_stage == PrologueStage.Bedroom && IsNear(_labDoor))
        {
            EnterLab();
            return;
        }

        if (_stage != PrologueStage.Lab)
        {
            return;
        }

        if (_missionRead &&
            _noteObject != null &&
            _noteObject.activeInHierarchy &&
            IsNear(_noteObject.transform))
        {
            ShowNote();
            return;
        }

        if (IsNear(_terminal))
        {
            if (_puzzleSolved)
            {
                ShowMissionFile();
            }
            else
            {
                ShowPuzzle();
            }
        }
    }

    private void RefreshInteractionPrompt()
    {
        if (_player == null || _ui == null)
        {
            return;
        }

        string prompt = string.Empty;
        if (_stage == PrologueStage.Bedroom && IsNear(_labDoor))
        {
            prompt = "E  //  ENTER MOM'S PRIVATE LAB";
        }
        else if (_stage == PrologueStage.Lab &&
                 _missionRead &&
                 _noteObject != null &&
                 _noteObject.activeInHierarchy &&
                 IsNear(_noteObject.transform))
        {
            prompt = "E  //  PICK UP ALICE'S NOTE";
        }
        else if (_stage == PrologueStage.Lab && IsNear(_terminal))
        {
            prompt = _puzzleSolved
                ? "E  //  OPEN THE SPA-077 FILE"
                : "E  //  ACCESS THE MAGNETIC CALIBRATION";
        }

        _ui.SetInteraction(prompt);
    }

    private bool IsNear(Transform target)
    {
        return target != null &&
               _player != null &&
               Vector3.Distance(_player.transform.position, target.position) <= InteractionDistance;
    }

    private void CreatePlayer(Vector3 position, float yaw)
    {
        _player = new GameObject("Areeba");
        _player.transform.SetParent(_worldRoot, false);
        _player.transform.position = position;
        _yaw = yaw;
        _pitch = 0f;
        _verticalVelocity = -1.2f;
        _player.transform.rotation = Quaternion.Euler(0f, yaw, 0f);

        _characterController = _player.AddComponent<CharacterController>();
        _characterController.height = 1.82f;
        _characterController.radius = 0.3f;
        _characterController.center = new Vector3(0f, 0.92f, 0f);
        _characterController.slopeLimit = 45f;
        _characterController.stepOffset = 0.3f;
        _characterController.skinWidth = 0.025f;
        _characterController.minMoveDistance = 0f;
    }

    private void DestroyPlayer()
    {
        _player = null;
        _characterController = null;
    }

    private void ClearWorld()
    {
        _player = null;
        _characterController = null;
        _labDoor = null;
        _terminal = null;
        _noteObject = null;

        if (_worldRoot == null)
        {
            return;
        }

        for (int index = _worldRoot.childCount - 1; index >= 0; index--)
        {
            GameObject child = _worldRoot.GetChild(index).gameObject;
            child.SetActive(false);
            Destroy(child);
        }
    }

    private void PlayAmbient(bool space)
    {
        if (_ambientSource == null)
        {
            return;
        }

        _ambientSource.Stop();
        _ambientSource.clip = CosmicRaysAudio.CreateRoomTone(space);
        _ambientSource.volume = space ? 0.14f : 0.11f;
        _ambientSource.Play();
    }

    private void SetCursorLocked(bool locked)
    {
        if (Application.isBatchMode)
        {
            _cursorLocked = false;
            return;
        }

        _cursorLocked = locked;
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }

    private void SetFade(float alpha)
    {
        if (_ui != null)
        {
            _ui.SetFade(alpha);
        }
    }

    private void FadeIn(float duration)
    {
        if (_fadeRoutine != null)
        {
            StopCoroutine(_fadeRoutine);
        }
        _fadeRoutine = StartCoroutine(FadeFromBlack(duration));
    }

    private IEnumerator FadeFromBlack(float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            SetFade(1f - Mathf.Clamp01(elapsed / duration));
            yield return null;
        }
        SetFade(0f);
        _fadeRoutine = null;
    }

    private static void Focus(Button button)
    {
        if (button != null && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(button.gameObject);
        }
    }

    private static void FocusNamed(Transform root, string name)
    {
        if (root == null)
        {
            return;
        }

        Transform child = root.Find(name);
        if (child != null)
        {
            Focus(child.GetComponent<Button>());
        }
    }

    private static void ClearUiFocus()
    {
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }
}
