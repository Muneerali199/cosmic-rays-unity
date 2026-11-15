using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

/// <summary>Playable Level 1: restore Enceladus Relay 07 and trace Alice's signal.</summary>
public sealed class CosmicRaysLevelOneController : MonoBehaviour
{
    private const float EyeHeight = 1.64f;
    private const float WalkSpeed = 3.35f;
    private const float RunSpeed = 4.9f;
    private const float InteractionDistance = 2.9f;
    private const float MouseSensitivity = 0.075f;

    private static readonly Color Cyan = new Color(0.4f, 0.9f, 0.96f, 1f);
    private static readonly Color Amber = new Color(1f, 0.66f, 0.22f, 1f);
    private static readonly Color Magenta = new Color(0.94f, 0.3f, 0.65f, 1f);

    private enum LevelStep
    {
        Intro,
        Relay,
        Reactor,
        PlumeScanner,
        Complete
    }

    private CosmicRaysGameplayController _gameplay;
    private LevelStep _step = LevelStep.Intro;
    private Transform _worldRoot;
    private Camera _camera;
    private GameObject _player;
    private CharacterController _characterController;
    private Transform _relay;
    private Transform _reactor;
    private Transform _plumeScanner;
    private GameObject _relayMarker;
    private GameObject _reactorMarker;
    private GameObject _scannerMarker;
    private RectTransform _introPanel;
    private RectTransform _completePanel;
    private RectTransform _hudPanel;
    private Text _objectiveLabel;
    private Text _statusLabel;
    private Text _promptLabel;
    private GameObject _promptBar;
    private float _yaw;
    private float _pitch;
    private float _verticalVelocity;
    private bool _initialized;
    private bool _cursorLocked;

    public void Initialize(CosmicRaysGameplayController gameplay)
    {
        _gameplay = gameplay;
    }

    private void Start()
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;
        if (_gameplay == null)
        {
            _gameplay = GetComponent<CosmicRaysGameplayController>();
        }

        ConfigureCamera();
        BuildWorld();
        BuildInterface();
        ShowIntro();
    }

    private void Update()
    {
        if (_step == LevelStep.Intro || _step == LevelStep.Complete || Time.timeScale <= 0f)
        {
            return;
        }

        if (!_cursorLocked && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
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

        RefreshPrompt();
    }

    private void LateUpdate()
    {
        if (_camera == null || _player == null)
        {
            return;
        }

        _camera.transform.position = _player.transform.position + Vector3.up * EyeHeight;
        _camera.transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
        if (!Application.isBatchMode)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void ConfigureCamera()
    {
        _camera = Camera.main;
        if (_camera == null)
        {
            _camera = FindAnyObjectByType<Camera>();
        }
        if (_camera == null)
        {
            var cameraObject = new GameObject("Level 1 Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.tag = "MainCamera";
            _camera = cameraObject.GetComponent<Camera>();
        }

        _camera.orthographic = false;
        _camera.fieldOfView = 61f;
        _camera.nearClipPlane = 0.04f;
        _camera.farClipPlane = 250f;
        _camera.clearFlags = CameraClearFlags.Skybox;
        _camera.allowHDR = true;

        UniversalAdditionalCameraData cameraData = _camera.GetUniversalAdditionalCameraData();
        cameraData.renderPostProcessing = true;
        cameraData.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
        cameraData.antialiasingQuality = AntialiasingQuality.High;
        cameraData.renderShadows = true;

        Light[] existingLights = FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int index = 0; index < existingLights.Length; index++)
        {
            existingLights[index].enabled = false;
        }

        CreatePostProcessing();
    }

    private void CreatePostProcessing()
    {
        var volumeObject = new GameObject("LEVEL 1 Cinematic Grade");
        volumeObject.transform.SetParent(transform, false);
        Volume volume = volumeObject.AddComponent<Volume>();
        volume.isGlobal = true;
        volume.priority = 4f;

        var profile = ScriptableObject.CreateInstance<VolumeProfile>();
        volume.profile = profile;

        Bloom bloom = profile.Add<Bloom>(true);
        bloom.intensity.Override(0.78f);
        bloom.threshold.Override(0.82f);
        bloom.scatter.Override(0.65f);

        Vignette vignette = profile.Add<Vignette>(true);
        vignette.intensity.Override(0.3f);
        vignette.smoothness.Override(0.46f);

        ColorAdjustments grade = profile.Add<ColorAdjustments>(true);
        grade.postExposure.Override(0.08f);
        grade.contrast.Override(15f);
        grade.saturation.Override(5f);
        grade.colorFilter.Override(new Color(0.91f, 0.97f, 1f));

        Tonemapping tonemapping = profile.Add<Tonemapping>(true);
        tonemapping.mode.Override(TonemappingMode.ACES);
    }

    private void BuildWorld()
    {
        _worldRoot = new GameObject("Generated Level 1 - Enceladus Relay").transform;
        CosmicRaysPrologueWorlds.ExplorationAnchors anchors =
            CosmicRaysPrologueWorlds.BuildLab(_worldRoot);

        _relay = anchors.Terminal;
        Transform reactor = _worldRoot.Find("ReactorCore");
        _reactor = reactor != null ? reactor : _worldRoot;

        GameObject scanner = CosmicRaysWorldBuilder.AddModel(
            _worldRoot,
            "WorkBay",
            new Vector3(-5.6f, 0.11f, 2.95f),
            90f,
            1.05f);
        _plumeScanner = scanner != null ? scanner.transform : _worldRoot;

        CosmicRaysWorldBuilder.AddModel(
            _worldRoot,
            "ComputerMonitor",
            new Vector3(-5.15f, 0.82f, 2.95f),
            90f,
            0.9f,
            false);
        CosmicRaysWorldBuilder.AddModel(
            _worldRoot,
            "WallConsole",
            new Vector3(-8.32f, 0.75f, 1.35f),
            90f,
            1.08f,
            false);

        CosmicRaysWorldBuilder.AddPointLight(
            _worldRoot,
            "Plume Analysis Glow",
            new Vector3(-5.55f, 1.55f, 2.95f),
            Cyan,
            3f,
            4.5f);
        CosmicRaysWorldBuilder.AddSpotLight(
            _worldRoot,
            "Relay Entry Light",
            new Vector3(-1.3f, 2.65f, -2.35f),
            Quaternion.Euler(70f, 25f, 0f),
            new Color(0.52f, 0.78f, 1f),
            2.8f,
            7f,
            58f,
            true);

        _relayMarker = CreateMarker("Relay Signal Marker", _relay, Cyan);
        _reactorMarker = CreateMarker("Reactor Marker", _reactor, Magenta);
        _scannerMarker = CreateMarker("Plume Scanner Marker", _plumeScanner, Amber);

        CreatePlayer(anchors.SpawnPosition, anchors.SpawnYaw);
        CreateAmbientAudio();
        UpdateMarkers();
    }

    private GameObject CreateMarker(string name, Transform target, Color color)
    {
        Vector3 position = target != null
            ? target.position + Vector3.up * 2.25f
            : Vector3.up * 2f;
        GameObject marker = CosmicRaysWorldBuilder.AddHologram(
            _worldRoot,
            name,
            position,
            color);
        marker.transform.localScale = Vector3.one * 0.58f;
        return marker;
    }

    private void CreateAmbientAudio()
    {
        var audioObject = new GameObject("Enceladus Relay Ambience");
        audioObject.transform.SetParent(transform, false);
        AudioSource source = audioObject.AddComponent<AudioSource>();
        source.clip = CosmicRaysAudio.CreateRoomTone(false);
        source.loop = true;
        source.playOnAwake = false;
        source.volume = 0.12f;
        source.spatialBlend = 0f;
        source.Play();
    }

    private void CreatePlayer(Vector3 position, float yaw)
    {
        _player = new GameObject("Adult Areeba");
        _player.transform.SetParent(_worldRoot, false);
        _player.transform.position = position;
        _yaw = yaw;
        _pitch = 0f;
        _verticalVelocity = -1.2f;

        _characterController = _player.AddComponent<CharacterController>();
        _characterController.height = 1.82f;
        _characterController.radius = 0.3f;
        _characterController.center = new Vector3(0f, 0.92f, 0f);
        _characterController.slopeLimit = 45f;
        _characterController.stepOffset = 0.3f;
        _characterController.skinWidth = 0.025f;
    }

    private void BuildInterface()
    {
        RectTransform root = CosmicRaysUIFactory.CreateCanvasRoot("COSMIC RAYS Level 1 UI");

        _hudPanel = CosmicRaysUIFactory.CreatePanel(root, "Level 1 HUD", false);
        Image hudImage = _hudPanel.GetComponent<Image>();
        hudImage.color = Color.clear;
        hudImage.raycastTarget = false;

        Text location = CosmicRaysUIFactory.CreateAnchoredLabel(
            _hudPanel,
            "Level Location",
            "LEVEL 01  /  ENCELADUS RELAY 07  /  07 DEC 2038",
            new Vector2(0.035f, 0.89f),
            new Vector2(0.72f, 0.95f),
            22,
            TextAnchor.MiddleLeft);
        location.color = Cyan;

        _objectiveLabel = CosmicRaysUIFactory.CreateAnchoredLabel(
            _hudPanel,
            "Level Objective",
            string.Empty,
            new Vector2(0.035f, 0.83f),
            new Vector2(0.76f, 0.89f),
            24,
            TextAnchor.MiddleLeft);
        _objectiveLabel.color = Amber;

        _statusLabel = CosmicRaysUIFactory.CreateAnchoredLabel(
            _hudPanel,
            "Relay Status",
            "RELAY LINK // OFFLINE",
            new Vector2(0.72f, 0.89f),
            new Vector2(0.965f, 0.95f),
            18,
            TextAnchor.MiddleRight);
        _statusLabel.color = new Color(0.58f, 0.72f, 0.78f);

        Text controls = CosmicRaysUIFactory.CreateAnchoredLabel(
            _hudPanel,
            "Level Controls",
            "WASD MOVE   MOUSE LOOK   SHIFT RUN   E INTERACT   ESC CURSOR",
            new Vector2(0.035f, 0.035f),
            new Vector2(0.62f, 0.085f),
            17,
            TextAnchor.MiddleLeft);
        controls.color = new Color(0.58f, 0.72f, 0.78f, 0.86f);

        _promptBar = CosmicRaysUIFactory.CreatePanel(_hudPanel, "Level Interaction Prompt").gameObject;
        RectTransform promptRect = _promptBar.GetComponent<RectTransform>();
        promptRect.anchorMin = new Vector2(0.31f, 0.105f);
        promptRect.anchorMax = new Vector2(0.69f, 0.17f);
        promptRect.offsetMin = Vector2.zero;
        promptRect.offsetMax = Vector2.zero;
        _promptBar.GetComponent<Image>().color = new Color(0.005f, 0.028f, 0.045f, 0.88f);
        _promptLabel = CosmicRaysUIFactory.CreateAnchoredLabel(
            promptRect,
            "Level Prompt",
            string.Empty,
            new Vector2(0.04f, 0f),
            new Vector2(0.96f, 1f),
            22,
            TextAnchor.MiddleCenter);
        _promptLabel.color = new Color(1f, 0.84f, 0.55f);
        _promptBar.SetActive(false);

        CreateCrosshair(_hudPanel);
        BuildIntro(root);
        BuildComplete(root);
    }

    private void BuildIntro(RectTransform root)
    {
        _introPanel = CosmicRaysUIFactory.CreatePanel(root, "Level 1 Intro");
        _introPanel.GetComponent<Image>().color = new Color(0.002f, 0.009f, 0.02f, 0.92f);

        Text kicker = CosmicRaysUIFactory.CreateAnchoredLabel(
            _introPanel,
            "Intro Kicker",
            "COSMIC RAYS MISSION  /  ENCELADUS",
            new Vector2(0.08f, 0.75f),
            new Vector2(0.72f, 0.81f),
            20,
            TextAnchor.MiddleLeft);
        kicker.color = Cyan;

        Text title = CosmicRaysUIFactory.CreateAnchoredLabel(
            _introPanel,
            "Intro Title",
            "LEVEL 01\nTHE FROZEN SIGNAL",
            new Vector2(0.075f, 0.48f),
            new Vector2(0.78f, 0.75f),
            70,
            TextAnchor.MiddleLeft);
        title.color = new Color(0.92f, 0.96f, 1f);

        Text briefing = CosmicRaysUIFactory.CreateAnchoredLabel(
            _introPanel,
            "Intro Briefing",
            "Relay 07 stopped responding after receiving Alice's transmission. Restore the station, stabilize SPA-077, and scan the plume sample before the signal disappears.",
            new Vector2(0.08f, 0.35f),
            new Vector2(0.67f, 0.47f),
            25,
            TextAnchor.MiddleLeft);
        briefing.color = new Color(0.76f, 0.84f, 0.88f);

        Button begin = CosmicRaysUIFactory.CreateButton(
            _introPanel,
            "BeginLevelOneButton",
            "ENTER RELAY 07",
            BeginLevel,
            62f);
        RectTransform buttonRect = (RectTransform)begin.transform;
        buttonRect.anchorMin = new Vector2(0.08f, 0.23f);
        buttonRect.anchorMax = new Vector2(0.31f, 0.3f);
        buttonRect.offsetMin = Vector2.zero;
        buttonRect.offsetMax = Vector2.zero;
        CosmicRaysPrologueUI.StyleButton(begin, new Color(0.07f, 0.32f, 0.42f, 1f));
    }

    private void BuildComplete(RectTransform root)
    {
        _completePanel = CosmicRaysUIFactory.CreatePanel(root, "Level 1 Complete");
        _completePanel.GetComponent<Image>().color = new Color(0.002f, 0.012f, 0.024f, 0.94f);

        Text tag = CosmicRaysUIFactory.CreateAnchoredLabel(
            _completePanel,
            "Complete Tag",
            "RELAY 07 // ONLINE // SIGNAL LOCKED",
            new Vector2(0.1f, 0.72f),
            new Vector2(0.8f, 0.78f),
            20,
            TextAnchor.MiddleLeft);
        tag.color = Cyan;

        Text title = CosmicRaysUIFactory.CreateAnchoredLabel(
            _completePanel,
            "Complete Title",
            "LEVEL 01 COMPLETE",
            new Vector2(0.095f, 0.55f),
            new Vector2(0.85f, 0.72f),
            64,
            TextAnchor.MiddleLeft);
        title.color = new Color(0.92f, 0.96f, 1f);

        Text message = CosmicRaysUIFactory.CreateAnchoredLabel(
            _completePanel,
            "Complete Message",
            "The plume scan contains a second transmission in Alice's voice. Coordinates point beneath Enceladus' south polar ice.\n\nLEVEL 02 // BENEATH THE ICE // LOCKED",
            new Vector2(0.1f, 0.37f),
            new Vector2(0.72f, 0.54f),
            25,
            TextAnchor.MiddleLeft);
        message.color = new Color(0.75f, 0.84f, 0.88f);

        Button explore = CosmicRaysUIFactory.CreateButton(
            _completePanel,
            "ContinueExploringButton",
            "CONTINUE EXPLORING",
            ContinueExploring,
            62f);
        RectTransform buttonRect = (RectTransform)explore.transform;
        buttonRect.anchorMin = new Vector2(0.1f, 0.24f);
        buttonRect.anchorMax = new Vector2(0.35f, 0.31f);
        buttonRect.offsetMin = Vector2.zero;
        buttonRect.offsetMax = Vector2.zero;
        CosmicRaysPrologueUI.StyleButton(explore, new Color(0.12f, 0.34f, 0.42f, 1f));

        _completePanel.gameObject.SetActive(false);
    }

    private static void CreateCrosshair(RectTransform parent)
    {
        Image horizontal = CosmicRaysUIFactory.CreateAnchoredImage(
            parent,
            "Crosshair Horizontal",
            new Vector2(0.493f, 0.499f),
            new Vector2(0.507f, 0.501f),
            new Color(0.72f, 0.94f, 1f, 0.78f));
        Image vertical = CosmicRaysUIFactory.CreateAnchoredImage(
            parent,
            "Crosshair Vertical",
            new Vector2(0.499f, 0.488f),
            new Vector2(0.501f, 0.512f),
            new Color(0.72f, 0.94f, 1f, 0.78f));
        horizontal.raycastTarget = false;
        vertical.raycastTarget = false;
    }

    private void ShowIntro()
    {
        _step = LevelStep.Intro;
        _introPanel.gameObject.SetActive(true);
        _completePanel.gameObject.SetActive(false);
        _hudPanel.gameObject.SetActive(false);
        if (_gameplay != null)
        {
            _gameplay.SetGameplayHudVisible(false);
        }
        Time.timeScale = 0f;
        SetCursorLocked(false);
        Focus(_introPanel.GetComponentInChildren<Button>());
    }

    private void BeginLevel()
    {
        _step = LevelStep.Relay;
        _introPanel.gameObject.SetActive(false);
        _hudPanel.gameObject.SetActive(true);
        Time.timeScale = 1f;
        SetCursorLocked(true);
        ClearFocus();
        UpdateObjective();
        UpdateMarkers();
        RefreshPrompt();
    }

    private void TryInteract()
    {
        if (_step == LevelStep.Relay && IsNear(_relay))
        {
            _step = LevelStep.Reactor;
            _statusLabel.text = "RELAY LINK // AUTHENTICATED";
            _statusLabel.color = Cyan;
            UpdateObjective();
            UpdateMarkers();
            CosmicRaysAudio.PlayClick();
            return;
        }

        if (_step == LevelStep.Reactor && IsNear(_reactor))
        {
            _step = LevelStep.PlumeScanner;
            _statusLabel.text = "CONTAINMENT // STABLE";
            _statusLabel.color = Magenta;
            UpdateObjective();
            UpdateMarkers();
            CosmicRaysAudio.PlayClick();
            return;
        }

        if (_step == LevelStep.PlumeScanner && IsNear(_plumeScanner))
        {
            CompleteLevel();
        }
    }

    private void CompleteLevel()
    {
        _step = LevelStep.Complete;
        _statusLabel.text = "ALICE SIGNAL // LOCKED";
        _statusLabel.color = Amber;
        UpdateMarkers();
        _promptBar.SetActive(false);

        if (_gameplay != null && !_gameplay.IsObjectiveComplete("month_01_inspect_base"))
        {
            _gameplay.CompleteObjective("month_01_inspect_base");
        }

        _completePanel.gameObject.SetActive(true);
        Time.timeScale = 0f;
        SetCursorLocked(false);
        Focus(_completePanel.GetComponentInChildren<Button>());
        CosmicRaysAudio.PlayClick();
    }

    private void ContinueExploring()
    {
        _completePanel.gameObject.SetActive(false);
        _hudPanel.gameObject.SetActive(true);
        Time.timeScale = 1f;
        SetCursorLocked(true);
        ClearFocus();
        _objectiveLabel.text = "OBJECTIVE COMPLETE // EXPLORE RELAY 07";
    }

    private void UpdateObjective()
    {
        switch (_step)
        {
            case LevelStep.Relay:
                _objectiveLabel.text = "OBJECTIVE 01 / 03 // RESTORE THE RELAY CONSOLE";
                break;
            case LevelStep.Reactor:
                _objectiveLabel.text = "OBJECTIVE 02 / 03 // STABILIZE THE SPA-077 CORE";
                break;
            case LevelStep.PlumeScanner:
                _objectiveLabel.text = "OBJECTIVE 03 / 03 // ANALYZE THE ENCELADUS PLUME SAMPLE";
                break;
        }
    }

    private void UpdateMarkers()
    {
        if (_relayMarker != null) _relayMarker.SetActive(_step == LevelStep.Relay);
        if (_reactorMarker != null) _reactorMarker.SetActive(_step == LevelStep.Reactor);
        if (_scannerMarker != null) _scannerMarker.SetActive(_step == LevelStep.PlumeScanner);
    }

    private void RefreshPrompt()
    {
        string prompt = string.Empty;
        if (_step == LevelStep.Relay && IsNear(_relay))
        {
            prompt = "E  //  AUTHENTICATE RELAY 07";
        }
        else if (_step == LevelStep.Reactor && IsNear(_reactor))
        {
            prompt = "E  //  STABILIZE CONTAINMENT CORE";
        }
        else if (_step == LevelStep.PlumeScanner && IsNear(_plumeScanner))
        {
            prompt = "E  //  RUN PLUME SPECTRUM SCAN";
        }

        _promptLabel.text = prompt;
        _promptBar.SetActive(!string.IsNullOrEmpty(prompt));
    }

    private bool IsNear(Transform target)
    {
        return target != null && _player != null &&
               Vector3.Distance(_player.transform.position, target.position) <= InteractionDistance;
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
        Vector3 direction = Vector3.ClampMagnitude(forward * input.y + right * input.x, 1f);

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

        _characterController.Move((direction * speed + Vector3.up * _verticalVelocity) * Time.deltaTime);
        _player.transform.rotation = Quaternion.Euler(0f, _yaw, 0f);
    }

    private static Vector2 ReadMovement()
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
        if (Gamepad.current != null)
        {
            movement += Gamepad.current.leftStick.ReadValue();
        }
        return Vector2.ClampMagnitude(movement, 1f);
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

    private static void Focus(Button button)
    {
        if (button != null && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(button.gameObject);
        }
    }

    private static void ClearFocus()
    {
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }
}
