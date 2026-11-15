using System;
using UnityEngine;
using UnityEngine.UI;

public sealed class CosmicRaysPrologueUI
{
    private static readonly Color PanelColor = new Color(0.012f, 0.035f, 0.06f, 0.97f);
    private static readonly Color Cyan = new Color(0.42f, 0.9f, 0.94f, 1f);
    private static readonly Color Amber = new Color(1f, 0.66f, 0.22f, 1f);
    private static readonly Color Magenta = new Color(0.93f, 0.32f, 0.66f, 1f);
    private static readonly Color Muted = new Color(0.58f, 0.7f, 0.76f, 1f);

    public RectTransform HudPanel;
    public RectTransform TitlePanel;
    public RectTransform DialoguePanel;
    public RectTransform ChoiceContainer;
    public RectTransform PuzzlePanel;
    public RectTransform PuzzleGrid;
    public RectTransform MissionFilePanel;
    public RectTransform NotePanel;
    public RectTransform EndingPanel;
    public RectTransform InteractionBar;

    public Text LocationLabel;
    public Text ObjectiveLabel;
    public Text InteractionLabel;
    public Text SpeakerLabel;
    public Text DialogueLabel;
    public Text PuzzleFeedback;
    public Text EndingMessage;
    public Text HintLabel;
    public Text RightHudLabel;

    public Button ContinueButton;
    public Button RunTestButton;
    public Button ResetPuzzleButton;
    public Button UnlockFileButton;

    public GameObject CrosshairRoot;
    public GameObject ControlsRoot;
    public GameObject LetterboxRoot;
    public Image FadeImage;

    private CosmicRaysPrologueUI()
    {
    }

    public static CosmicRaysPrologueUI Build(
        Action beginPrologue,
        Action advanceDialogue,
        Action runTest,
        Action resetPuzzle,
        Action unlockFile,
        Action exitTerminal,
        Action closeFile,
        Action foldNote,
        Action beginMission,
        Action replay)
    {
        var ui = new CosmicRaysPrologueUI();
        RectTransform safeArea = CosmicRaysUIFactory.CreateCanvasRoot("COSMIC RAYS Prologue UI");
        ui.BuildLetterbox(safeArea);
        ui.BuildHud(safeArea);
        ui.BuildTitle(safeArea, beginPrologue);
        ui.BuildDialogue(safeArea, advanceDialogue);
        ui.BuildPuzzle(safeArea, runTest, resetPuzzle, unlockFile, exitTerminal);
        ui.BuildMissionFile(safeArea, closeFile);
        ui.BuildNote(safeArea, foldNote);
        ui.BuildEnding(safeArea, beginMission, replay);
        ui.BuildFade(safeArea);
        ui.HideAllPanels();
        return ui;
    }

    public void SetHud(string location, string objective, string right, bool crosshair, bool controls)
    {
        LocationLabel.text = location;
        ObjectiveLabel.text = objective;
        RightHudLabel.text = right;
        CrosshairRoot.SetActive(crosshair);
        ControlsRoot.SetActive(controls);
    }

    public void SetInteraction(string prompt)
    {
        bool visible = !string.IsNullOrEmpty(prompt);
        InteractionBar.gameObject.SetActive(visible);
        InteractionLabel.text = prompt;
    }

    public void SetCinematic(bool cinematic)
    {
        LetterboxRoot.SetActive(cinematic);
    }

    public void SetFade(float alpha)
    {
        Color color = FadeImage.color;
        color.a = Mathf.Clamp01(alpha);
        FadeImage.color = color;
        FadeImage.raycastTarget = alpha > 0.985f;
    }

    public void ClearChoices()
    {
        for (int index = ChoiceContainer.childCount - 1; index >= 0; index--)
        {
            GameObject child = ChoiceContainer.GetChild(index).gameObject;
            child.SetActive(false);
            UnityEngine.Object.Destroy(child);
        }
    }

    public void HideAllPanels()
    {
        TitlePanel.gameObject.SetActive(false);
        DialoguePanel.gameObject.SetActive(false);
        PuzzlePanel.gameObject.SetActive(false);
        MissionFilePanel.gameObject.SetActive(false);
        NotePanel.gameObject.SetActive(false);
        EndingPanel.gameObject.SetActive(false);
    }

    public static void StyleButton(Button button, Color color)
    {
        if (button == null)
        {
            return;
        }

        ColorBlock colors = button.colors;
        colors.normalColor = color;
        colors.highlightedColor = Color.Lerp(color, Color.white, 0.25f);
        colors.selectedColor = Color.Lerp(color, Color.white, 0.34f);
        colors.pressedColor = Color.Lerp(color, Color.black, 0.22f);
        colors.disabledColor = new Color(0.2f, 0.24f, 0.27f, 0.65f);
        button.colors = colors;

        if (button.image != null)
        {
            button.image.color = Color.white;
        }

        Outline outline = button.GetComponent<Outline>();
        if (outline != null)
        {
            outline.effectColor = new Color(color.r, color.g, color.b, 0.72f);
        }
    }

    public static Button CreateChoiceButton(
        RectTransform container,
        string name,
        string label,
        UnityEngine.Events.UnityAction action)
    {
        Button button = CosmicRaysUIFactory.CreateButton(container, name, label, action, 52f);
        StyleButton(button, new Color(0.1f, 0.32f, 0.42f, 1f));
        return button;
    }

    private void BuildLetterbox(RectTransform root)
    {
        LetterboxRoot = new GameObject("Cinematic Letterbox", typeof(RectTransform));
        RectTransform letterbox = LetterboxRoot.GetComponent<RectTransform>();
        letterbox.SetParent(root, false);
        letterbox.anchorMin = Vector2.zero;
        letterbox.anchorMax = Vector2.one;
        letterbox.offsetMin = Vector2.zero;
        letterbox.offsetMax = Vector2.zero;

        Image top = CreateAnchoredImage(
            letterbox,
            "Letterbox Top",
            new Vector2(0f, 0.952f),
            Vector2.one,
            Color.black);
        Image bottom = CreateAnchoredImage(
            letterbox,
            "Letterbox Bottom",
            Vector2.zero,
            new Vector2(1f, 0.048f),
            Color.black);
        top.raycastTarget = false;
        bottom.raycastTarget = false;
    }

    private void BuildHud(RectTransform root)
    {
        HudPanel = CosmicRaysUIFactory.CreatePanel(root, "Mission HUD", false);
        HudPanel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0f);
        HudPanel.GetComponent<Image>().raycastTarget = false;

        CreateAnchoredImage(
            HudPanel,
            "Top Rule",
            new Vector2(0.035f, 0.925f),
            new Vector2(0.965f, 0.927f),
            new Color(0.32f, 0.82f, 0.92f, 0.45f));
        CreateAnchoredImage(
            HudPanel,
            "Top Rule Accent",
            new Vector2(0.035f, 0.925f),
            new Vector2(0.18f, 0.929f),
            Amber);

        LocationLabel = CosmicRaysUIFactory.CreateAnchoredLabel(
            HudPanel,
            "Location",
            string.Empty,
            new Vector2(0.035f, 0.875f),
            new Vector2(0.7f, 0.925f),
            21,
            TextAnchor.MiddleLeft);
        LocationLabel.color = Cyan;

        ObjectiveLabel = CosmicRaysUIFactory.CreateAnchoredLabel(
            HudPanel,
            "Objective",
            string.Empty,
            new Vector2(0.035f, 0.82f),
            new Vector2(0.7f, 0.875f),
            22,
            TextAnchor.MiddleLeft);
        ObjectiveLabel.color = Amber;

        RightHudLabel = CosmicRaysUIFactory.CreateAnchoredLabel(
            HudPanel,
            "Signal Readout",
            "SPA-077 // LINK: STANDBY",
            new Vector2(0.7f, 0.875f),
            new Vector2(0.965f, 0.925f),
            17,
            TextAnchor.MiddleRight);
        RightHudLabel.color = Muted;

        ControlsRoot = new GameObject("Controls Hint", typeof(RectTransform));
        RectTransform controls = ControlsRoot.GetComponent<RectTransform>();
        controls.SetParent(HudPanel, false);
        controls.anchorMin = new Vector2(0.035f, 0.055f);
        controls.anchorMax = new Vector2(0.52f, 0.09f);
        controls.offsetMin = Vector2.zero;
        controls.offsetMax = Vector2.zero;
        HintLabel = CosmicRaysUIFactory.CreateAnchoredLabel(
            controls,
            "Hint",
            "WASD  MOVE     MOUSE  LOOK     E  INTERACT     ESC  CURSOR",
            Vector2.zero,
            Vector2.one,
            17,
            TextAnchor.MiddleLeft);
        HintLabel.color = new Color(0.58f, 0.72f, 0.78f, 0.82f);

        InteractionBar = CreateAnchoredPanel(
            HudPanel,
            "Interaction Bar",
            new Vector2(0.285f, 0.105f),
            new Vector2(0.715f, 0.165f),
            new Color(0.01f, 0.035f, 0.055f, 0.76f));
        InteractionBar.GetComponent<Image>().raycastTarget = false;
        InteractionLabel = CosmicRaysUIFactory.CreateAnchoredLabel(
            InteractionBar,
            "Prompt",
            string.Empty,
            new Vector2(0.04f, 0f),
            new Vector2(0.96f, 1f),
            22,
            TextAnchor.MiddleCenter);
        InteractionLabel.color = new Color(0.92f, 0.84f, 0.6f);
        InteractionBar.gameObject.SetActive(false);

        CrosshairRoot = new GameObject("Crosshair", typeof(RectTransform));
        RectTransform crosshair = CrosshairRoot.GetComponent<RectTransform>();
        crosshair.SetParent(HudPanel, false);
        crosshair.anchorMin = new Vector2(0.5f, 0.5f);
        crosshair.anchorMax = new Vector2(0.5f, 0.5f);
        crosshair.sizeDelta = new Vector2(40f, 40f);
        CreateCenteredImage(crosshair, "Dot", 5f, new Color(0.78f, 0.94f, 1f, 0.95f));
        CreateRing(crosshair, 26f, new Color(0.48f, 0.86f, 0.95f, 0.3f));
        CrosshairRoot.SetActive(false);
    }

    private void BuildTitle(RectTransform root, Action beginPrologue)
    {
        TitlePanel = CosmicRaysUIFactory.CreatePanel(root, "Title Screen");
        TitlePanel.GetComponent<Image>().color = new Color(0.005f, 0.012f, 0.025f, 0.48f);

        RectTransform fade = CosmicRaysUIFactory.CreateGradientPanel(
            TitlePanel,
            "Title Lower Gradient",
            false,
            new Color(0.005f, 0.012f, 0.025f, 0.97f));
        fade.anchorMin = new Vector2(0f, 0f);
        fade.anchorMax = new Vector2(1f, 0.82f);

        CreateAnchoredImage(
            TitlePanel,
            "Title Vertical Accent",
            new Vector2(0.066f, 0.32f),
            new Vector2(0.069f, 0.79f),
            new Color(1f, 0.56f, 0.2f, 0.85f));

        Text kicker = CosmicRaysUIFactory.CreateAnchoredLabel(
            TitlePanel,
            "Kicker",
            "A COSMIC RAYS STORY  /  PROLOGUE",
            new Vector2(0.085f, 0.765f),
            new Vector2(0.62f, 0.81f),
            19,
            TextAnchor.MiddleLeft);
        kicker.color = Cyan;

        Text title = CosmicRaysUIFactory.CreateAnchoredLabel(
            TitlePanel,
            "Title",
            "COSMIC\nRAYS",
            new Vector2(0.082f, 0.5f),
            new Vector2(0.64f, 0.76f),
            108,
            TextAnchor.MiddleLeft);
        title.color = new Color(1f, 0.87f, 0.67f);
        title.lineSpacing = 0.82f;

        Text subtitle = CosmicRaysUIFactory.CreateAnchoredLabel(
            TitlePanel,
            "Subtitle",
            "A R E E B A ' S   P R O M I S E",
            new Vector2(0.085f, 0.44f),
            new Vector2(0.68f, 0.5f),
            29,
            TextAnchor.MiddleLeft);
        subtitle.color = Amber;

        Text logline = CosmicRaysUIFactory.CreateAnchoredLabel(
            TitlePanel,
            "Logline",
            "A solar mission. A missing scientist.\nA signal waiting beyond Saturn.",
            new Vector2(0.085f, 0.36f),
            new Vector2(0.61f, 0.44f),
            24,
            TextAnchor.MiddleLeft);
        logline.color = new Color(0.8f, 0.86f, 0.88f);

        Button start = CosmicRaysUIFactory.CreateButton(
            TitlePanel,
            "BeginPrologueButton",
            "BEGIN PROLOGUE",
            () => beginPrologue(),
            60f);
        RectTransform startRect = (RectTransform)start.transform;
        startRect.anchorMin = new Vector2(0.082f, 0.27f);
        startRect.anchorMax = new Vector2(0.29f, 0.335f);
        startRect.offsetMin = Vector2.zero;
        startRect.offsetMax = Vector2.zero;
        StyleButton(start, new Color(0.42f, 0.18f, 0.055f, 1f));

        Text metadata = CosmicRaysUIFactory.CreateAnchoredLabel(
            TitlePanel,
            "Metadata",
            "GREENBELT, MARYLAND  /  EARTH\n04 OCT 2028  /  19:42",
            new Vector2(0.7f, 0.11f),
            new Vector2(0.955f, 0.19f),
            18,
            TextAnchor.MiddleRight);
        metadata.color = new Color(0.54f, 0.68f, 0.75f);

        Text chapter = CosmicRaysUIFactory.CreateAnchoredLabel(
            TitlePanel,
            "Chapter",
            "PROLOGUE 00\nTHE PROMISE",
            new Vector2(0.7f, 0.72f),
            new Vector2(0.955f, 0.8f),
            18,
            TextAnchor.MiddleRight);
        chapter.color = new Color(0.54f, 0.68f, 0.75f);
    }

    private void BuildDialogue(RectTransform root, Action advanceDialogue)
    {
        DialoguePanel = CreateAnchoredPanel(
            root,
            "Dialogue Panel",
            new Vector2(0.052f, 0.045f),
            new Vector2(0.948f, 0.405f),
            new Color(0.008f, 0.028f, 0.045f, 0.94f));
        CreateAnchoredImage(
            DialoguePanel,
            "Dialogue Accent",
            new Vector2(0f, 0f),
            new Vector2(0.0045f, 1f),
            Amber);
        CreateAnchoredImage(
            DialoguePanel,
            "Dialogue Rule",
            new Vector2(0.035f, 0.86f),
            new Vector2(0.965f, 0.865f),
            new Color(0.4f, 0.8f, 0.9f, 0.18f));

        SpeakerLabel = CosmicRaysUIFactory.CreateAnchoredLabel(
            DialoguePanel,
            "Speaker",
            string.Empty,
            new Vector2(0.04f, 0.84f),
            new Vector2(0.35f, 0.96f),
            22,
            TextAnchor.MiddleLeft);
        SpeakerLabel.color = Amber;

        Text channel = CosmicRaysUIFactory.CreateAnchoredLabel(
            DialoguePanel,
            "Channel",
            "MEMORY RECORD // 01",
            new Vector2(0.55f, 0.84f),
            new Vector2(0.96f, 0.96f),
            15,
            TextAnchor.MiddleRight);
        channel.color = new Color(0.42f, 0.66f, 0.75f);

        DialogueLabel = CosmicRaysUIFactory.CreateAnchoredLabel(
            DialoguePanel,
            "Dialogue",
            string.Empty,
            new Vector2(0.04f, 0.52f),
            new Vector2(0.96f, 0.84f),
            28,
            TextAnchor.MiddleLeft);
        DialogueLabel.color = new Color(0.9f, 0.94f, 0.95f);

        ChoiceContainer = CreateVerticalContainer(
            DialoguePanel,
            "Dialogue Choices",
            new Vector2(0.04f, 0.2f),
            new Vector2(0.96f, 0.52f));

        ContinueButton = CosmicRaysUIFactory.CreateButton(
            DialoguePanel,
            "ContinueButton",
            "CONTINUE",
            () => advanceDialogue(),
            52f);
        RectTransform continueRect = (RectTransform)ContinueButton.transform;
        continueRect.anchorMin = new Vector2(0.04f, 0.055f);
        continueRect.anchorMax = new Vector2(0.24f, 0.16f);
        continueRect.offsetMin = Vector2.zero;
        continueRect.offsetMax = Vector2.zero;
        StyleButton(ContinueButton, new Color(0.08f, 0.3f, 0.38f, 1f));
    }

    private void BuildPuzzle(
        RectTransform root,
        Action runTest,
        Action resetPuzzle,
        Action unlockFile,
        Action exitTerminal)
    {
        PuzzlePanel = CosmicRaysUIFactory.CreatePanel(root, "Magnetic Calibration");
        PuzzlePanel.GetComponent<Image>().color = new Color(0.004f, 0.018f, 0.03f, 0.975f);
        RectTransform glow = CosmicRaysUIFactory.CreateGradientPanel(
            PuzzlePanel,
            "Calibration Glow",
            false,
            new Color(0.02f, 0.12f, 0.16f, 0.32f));
        glow.anchorMin = new Vector2(0f, 0.35f);
        glow.anchorMax = new Vector2(1f, 1f);

        Text title = CosmicRaysUIFactory.CreateAnchoredLabel(
            PuzzlePanel,
            "Title",
            "MAGNETIC FIELD CALIBRATION",
            new Vector2(0.075f, 0.895f),
            new Vector2(0.78f, 0.965f),
            40,
            TextAnchor.MiddleLeft);
        title.color = Cyan;

        Text tag = CosmicRaysUIFactory.CreateAnchoredLabel(
            PuzzlePanel,
            "Tag",
            "SPA-077  /  EXPERIMENTAL SYSTEM",
            new Vector2(0.075f, 0.85f),
            new Vector2(0.7f, 0.895f),
            17,
            TextAnchor.MiddleLeft);
        tag.color = Magenta;

        Text instructions = CosmicRaysUIFactory.CreateAnchoredLabel(
            PuzzlePanel,
            "Instructions",
            "Rotate the coils to carry one charged particle from the middle-left source to the middle-right collector.\nThe magenta B2 column is the protected research core. Select it for details; route around it.",
            new Vector2(0.075f, 0.77f),
            new Vector2(0.7f, 0.845f),
            19,
            TextAnchor.MiddleLeft);
        instructions.color = new Color(0.7f, 0.8f, 0.84f);

        RectTransform gridFrame = CreateAnchoredPanel(
            PuzzlePanel,
            "Field Grid Frame",
            new Vector2(0.075f, 0.315f),
            new Vector2(0.57f, 0.755f),
            new Color(0.015f, 0.052f, 0.072f, 0.82f));
        CosmicRaysUIFactory.CreateEdgeLine(gridFrame, new Color(0.32f, 0.85f, 0.92f, 0.75f), 0.005f);

        var gridObject = new GameObject(
            "Field Grid",
            typeof(RectTransform),
            typeof(GridLayoutGroup));
        PuzzleGrid = gridObject.GetComponent<RectTransform>();
        PuzzleGrid.SetParent(gridFrame, false);
        PuzzleGrid.anchorMin = new Vector2(0.05f, 0.09f);
        PuzzleGrid.anchorMax = new Vector2(0.95f, 0.91f);
        PuzzleGrid.offsetMin = Vector2.zero;
        PuzzleGrid.offsetMax = Vector2.zero;
        GridLayoutGroup layout = gridObject.GetComponent<GridLayoutGroup>();
        layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layout.constraintCount = 3;
        layout.cellSize = new Vector2(216f, 86f);
        layout.spacing = new Vector2(10f, 10f);
        layout.childAlignment = TextAnchor.MiddleCenter;

        Text source = CosmicRaysUIFactory.CreateAnchoredLabel(
            gridFrame,
            "Source",
            "SOURCE",
            new Vector2(0.005f, 0.44f),
            new Vector2(0.09f, 0.55f),
            13,
            TextAnchor.MiddleRight);
        source.color = Cyan;
        Text collector = CosmicRaysUIFactory.CreateAnchoredLabel(
            gridFrame,
            "Collector",
            "COLLECTOR",
            new Vector2(0.91f, 0.44f),
            new Vector2(0.997f, 0.55f),
            13,
            TextAnchor.MiddleLeft);
        collector.color = Cyan;

        RectTransform telemetry = CreateAnchoredPanel(
            PuzzlePanel,
            "Telemetry",
            new Vector2(0.605f, 0.315f),
            new Vector2(0.925f, 0.755f),
            new Color(0.01f, 0.038f, 0.055f, 0.9f));
        CosmicRaysUIFactory.CreateEdgeLine(telemetry, new Color(0.9f, 0.34f, 0.62f, 0.9f), 0.007f);
        Text telemetryHeader = CosmicRaysUIFactory.CreateAnchoredLabel(
            telemetry,
            "Telemetry Header",
            "ROUTING TELEMETRY",
            new Vector2(0.08f, 0.83f),
            new Vector2(0.92f, 0.94f),
            18,
            TextAnchor.MiddleLeft);
        telemetryHeader.color = Cyan;
        Text state = CosmicRaysUIFactory.CreateAnchoredLabel(
            telemetry,
            "State",
            "FIELD STANDBY\nSOURCE: CHARGED\nCOLLECTOR: READY",
            new Vector2(0.08f, 0.57f),
            new Vector2(0.92f, 0.82f),
            17,
            TextAnchor.MiddleLeft);
        state.color = new Color(0.54f, 0.68f, 0.74f);
        Text coreWarning = CosmicRaysUIFactory.CreateAnchoredLabel(
            telemetry,
            "Core Warning",
            "B2  //  PROTECTED CORE\nNO PARTICLE MAY CROSS",
            new Vector2(0.08f, 0.38f),
            new Vector2(0.92f, 0.54f),
            16,
            TextAnchor.MiddleLeft);
        coreWarning.color = Magenta;

        PuzzleFeedback = CosmicRaysUIFactory.CreateAnchoredLabel(
            telemetry,
            "Feedback",
            "Route not tested. Select any coil.",
            new Vector2(0.08f, 0.07f),
            new Vector2(0.92f, 0.36f),
            18,
            TextAnchor.MiddleLeft);
        PuzzleFeedback.color = Amber;

        Text footer = CosmicRaysUIFactory.CreateAnchoredLabel(
            PuzzlePanel,
            "Footer",
            "COIL GRID 3 x 3  /  CLOCKWISE ROTATION  /  B2 IS FIXED",
            new Vector2(0.075f, 0.245f),
            new Vector2(0.58f, 0.3f),
            15,
            TextAnchor.MiddleLeft);
        footer.color = new Color(0.42f, 0.56f, 0.62f);

        RunTestButton = CreateAnchoredButton(
            PuzzlePanel,
            "RunTestButton",
            "RUN PARTICLE TEST",
            () => runTest(),
            new Vector2(0.605f, 0.225f),
            new Vector2(0.755f, 0.29f),
            Amber);
        ResetPuzzleButton = CreateAnchoredButton(
            PuzzlePanel,
            "ResetButton",
            "RESET COILS",
            () => resetPuzzle(),
            new Vector2(0.765f, 0.225f),
            new Vector2(0.925f, 0.29f),
            new Color(0.18f, 0.38f, 0.46f, 1f));
        UnlockFileButton = CreateAnchoredButton(
            PuzzlePanel,
            "UnlockFileButton",
            "UNLOCK SPA-077 FILE",
            () => unlockFile(),
            new Vector2(0.605f, 0.145f),
            new Vector2(0.775f, 0.21f),
            Cyan);
        CreateAnchoredButton(
            PuzzlePanel,
            "ExitTerminalButton",
            "EXIT TERMINAL",
            () => exitTerminal(),
            new Vector2(0.785f, 0.145f),
            new Vector2(0.925f, 0.21f),
            new Color(0.18f, 0.38f, 0.46f, 1f));
    }

    private void BuildMissionFile(RectTransform root, Action closeFile)
    {
        MissionFilePanel = CosmicRaysUIFactory.CreatePanel(root, "SPA-077 Mission File");
        MissionFilePanel.GetComponent<Image>().color = new Color(0.008f, 0.025f, 0.035f, 0.985f);
        RectTransform glow = CosmicRaysUIFactory.CreateGradientPanel(
            MissionFilePanel,
            "File Glow",
            false,
            new Color(0.025f, 0.1f, 0.11f, 0.45f));
        glow.anchorMin = new Vector2(0f, 0.4f);
        glow.anchorMax = new Vector2(1f, 1f);

        Text topTag = CosmicRaysUIFactory.CreateAnchoredLabel(
            MissionFilePanel,
            "Top Tag",
            "DECLASSIFIED  /  EYES ONLY  /  SPA-077",
            new Vector2(0.075f, 0.915f),
            new Vector2(0.75f, 0.96f),
            16,
            TextAnchor.MiddleLeft);
        topTag.color = new Color(0.55f, 0.7f, 0.76f);

        Text title = CosmicRaysUIFactory.CreateAnchoredLabel(
            MissionFilePanel,
            "Title",
            "SPA-077",
            new Vector2(0.075f, 0.81f),
            new Vector2(0.55f, 0.92f),
            62,
            TextAnchor.MiddleLeft);
        title.color = Cyan;

        Text metadata = CosmicRaysUIFactory.CreateAnchoredLabel(
            MissionFilePanel,
            "Metadata",
            "SOLAR PARTICLE ABSORBER\nLAUNCH: 07 DEC 2028  /  DURATION: 10 YEARS\nLEAD: DR. ALICE RAHMAN",
            new Vector2(0.555f, 0.805f),
            new Vector2(0.93f, 0.91f),
            18,
            TextAnchor.MiddleLeft);
        metadata.color = new Color(0.68f, 0.8f, 0.84f);

        CosmicRaysUIFactory.CreateDivider(
            MissionFilePanel,
            new Color(0.36f, 0.8f, 0.9f, 0.35f),
            2f).rectTransform.anchorMin = new Vector2(0.075f, 0.79f);
        Image divider = MissionFilePanel.Find("Divider").GetComponent<Image>();
        RectTransform dividerRect = divider.rectTransform;
        dividerRect.anchorMax = new Vector2(0.925f, 0.792f);
        dividerRect.offsetMin = Vector2.zero;
        dividerRect.offsetMax = Vector2.zero;

        Text observedTag = CosmicRaysUIFactory.CreateAnchoredLabel(
            MissionFilePanel,
            "Observed Tag",
            "OBSERVED SCIENCE",
            new Vector2(0.075f, 0.735f),
            new Vector2(0.4f, 0.785f),
            16,
            TextAnchor.MiddleLeft);
        observedTag.color = Cyan;

        Text observed = CosmicRaysUIFactory.CreateAnchoredLabel(
            MissionFilePanel,
            "Objective",
            "Detect and study solar energetic particles released during major solar events. Measure their energy, charge, direction, and effects on spacecraft systems.",
            new Vector2(0.075f, 0.635f),
            new Vector2(0.925f, 0.73f),
            22,
            TextAnchor.MiddleLeft);
        observed.color = new Color(0.78f, 0.85f, 0.87f);

        Text experimentalTag = CosmicRaysUIFactory.CreateAnchoredLabel(
            MissionFilePanel,
            "Experimental Tag",
            "EXPERIMENTAL MODULE",
            new Vector2(0.075f, 0.575f),
            new Vector2(0.4f, 0.625f),
            16,
            TextAnchor.MiddleLeft);
        experimentalTag.color = Magenta;

        Text restricted = CosmicRaysUIFactory.CreateAnchoredLabel(
            MissionFilePanel,
            "Restricted Module",
            "Test limited particle capture and magnetic confinement. Results may support the theoretical SEP-MPS-269 propulsion study. No working engine has been demonstrated.",
            new Vector2(0.075f, 0.475f),
            new Vector2(0.925f, 0.57f),
            22,
            TextAnchor.MiddleLeft);
        restricted.color = new Color(0.78f, 0.85f, 0.87f);

        Text clue = CosmicRaysUIFactory.CreateAnchoredLabel(
            MissionFilePanel,
            "Clue",
            "AREEBA: IF YOU FOUND THIS, CHECK BENEATH MY DESK.",
            new Vector2(0.075f, 0.39f),
            new Vector2(0.925f, 0.45f),
            22,
            TextAnchor.MiddleLeft);
        clue.color = Amber;

        Text signature = CosmicRaysUIFactory.CreateAnchoredLabel(
            MissionFilePanel,
            "Signature",
            "SPA-077 CONTROL  /  AUTHENTICATED",
            new Vector2(0.075f, 0.31f),
            new Vector2(0.6f, 0.35f),
            15,
            TextAnchor.MiddleLeft);
        signature.color = new Color(0.4f, 0.6f, 0.66f);

        Button close = CreateAnchoredButton(
            MissionFilePanel,
            "CloseFileButton",
            "CLOSE FILE",
            () => closeFile(),
            new Vector2(0.075f, 0.11f),
            new Vector2(0.255f, 0.175f),
            new Color(0.16f, 0.38f, 0.48f, 1f));
        close.GetComponentInChildren<Text>().fontSize = 20;
    }

    private void BuildNote(RectTransform root, Action foldNote)
    {
        NotePanel = CosmicRaysUIFactory.CreatePanel(root, "Alice's Note");
        NotePanel.GetComponent<Image>().color = new Color(0.005f, 0.014f, 0.02f, 0.94f);
        RectTransform paper = CreateAnchoredPanel(
            NotePanel,
            "Paper",
            new Vector2(0.285f, 0.16f),
            new Vector2(0.715f, 0.85f),
            new Color(0.12f, 0.075f, 0.042f, 0.99f));
        CosmicRaysUIFactory.CreateEdgeLine(paper, new Color(1f, 0.67f, 0.25f, 0.9f), 0.006f);

        Text tag = CosmicRaysUIFactory.CreateAnchoredLabel(
            paper,
            "Tag",
            "HANDWRITTEN  /  RECOVERED",
            new Vector2(0.08f, 0.88f),
            new Vector2(0.92f, 0.96f),
            14,
            TextAnchor.MiddleLeft);
        tag.color = new Color(0.66f, 0.52f, 0.36f);

        Text title = CosmicRaysUIFactory.CreateAnchoredLabel(
            paper,
            "Title",
            "TO AREEBA",
            new Vector2(0.08f, 0.755f),
            new Vector2(0.92f, 0.87f),
            46,
            TextAnchor.MiddleLeft);
        title.color = Amber;

        Text body = CosmicRaysUIFactory.CreateAnchoredLabel(
            paper,
            "Note",
            "My brave girl,\n\nIf I cannot come home on your birthday, watch the sky. SPA-077 carries more than a mission. It carries a path back to you.\n\nTrust the signal. Remember Enceladus.\n\nMom",
            new Vector2(0.08f, 0.31f),
            new Vector2(0.92f, 0.745f),
            25,
            TextAnchor.MiddleLeft);
        body.color = new Color(0.96f, 0.87f, 0.72f);

        Button fold = CreateAnchoredButton(
            paper,
            "FoldNoteButton",
            "FOLD THE NOTE",
            () => foldNote(),
            new Vector2(0.08f, 0.12f),
            new Vector2(0.4f, 0.22f),
            new Color(0.45f, 0.22f, 0.08f, 1f));
        fold.GetComponentInChildren<Text>().fontSize = 20;
    }

    private void BuildEnding(RectTransform root, Action beginMission, Action replay)
    {
        EndingPanel = CosmicRaysUIFactory.CreatePanel(root, "Enceladus Signal");
        EndingPanel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.18f);
        RectTransform fade = CosmicRaysUIFactory.CreateGradientPanel(
            EndingPanel,
            "Ending Lower Gradient",
            false,
            new Color(0.004f, 0.011f, 0.022f, 0.97f));
        fade.anchorMin = new Vector2(0f, 0f);
        fade.anchorMax = new Vector2(0.62f, 0.76f);

        Text timeJump = CosmicRaysUIFactory.CreateAnchoredLabel(
            EndingPanel,
            "Time Jump",
            "10 YEARS LATER  /  07 DEC 2038",
            new Vector2(0.06f, 0.865f),
            new Vector2(0.52f, 0.915f),
            18,
            TextAnchor.MiddleLeft);
        timeJump.color = Amber;

        Text title = CosmicRaysUIFactory.CreateAnchoredLabel(
            EndingPanel,
            "Title",
            "SIGNAL DETECTED:\nENCELADUS",
            new Vector2(0.058f, 0.63f),
            new Vector2(0.58f, 0.84f),
            58,
            TextAnchor.MiddleLeft);
        title.color = Cyan;
        title.lineSpacing = 0.88f;

        EndingMessage = CosmicRaysUIFactory.CreateAnchoredLabel(
            EndingPanel,
            "Message",
            "The transmission carries one word in Alice's voice: Areeba.",
            new Vector2(0.06f, 0.55f),
            new Vector2(0.54f, 0.62f),
            23,
            TextAnchor.MiddleLeft);
        EndingMessage.color = new Color(0.82f, 0.88f, 0.9f);

        Button mission = CreateAnchoredButton(
            EndingPanel,
            "BeginMissionButton",
            "BEGIN ENCELADUS MISSION",
            () => beginMission(),
            new Vector2(0.06f, 0.245f),
            new Vector2(0.31f, 0.31f),
            Cyan);
        StyleButton(mission, new Color(0.08f, 0.32f, 0.42f, 1f));

        CreateAnchoredButton(
            EndingPanel,
            "ReplayButton",
            "REPLAY PROLOGUE",
            () => replay(),
            new Vector2(0.06f, 0.165f),
            new Vector2(0.22f, 0.225f),
            new Color(0.16f, 0.35f, 0.43f, 1f));

        Text beacon = CosmicRaysUIFactory.CreateAnchoredLabel(
            EndingPanel,
            "Beacon",
            "RELAY 07  //  SIGNAL LOCKED\nSOURCE: ENCELADUS SOUTH POLAR PLUME",
            new Vector2(0.62f, 0.72f),
            new Vector2(0.94f, 0.8f),
            16,
            TextAnchor.MiddleRight);
        beacon.color = new Color(0.48f, 0.74f, 0.82f);
    }

    private void BuildFade(RectTransform root)
    {
        RectTransform fade = CosmicRaysUIFactory.CreatePanel(root, "Fade", false);
        FadeImage = fade.GetComponent<Image>();
        FadeImage.color = Color.black;
        FadeImage.raycastTarget = false;
        fade.SetAsLastSibling();
    }

    private static RectTransform CreateAnchoredPanel(
        Transform parent,
        string name,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Color color)
    {
        RectTransform panel = CosmicRaysUIFactory.CreatePanel(parent, name);
        panel.anchorMin = anchorMin;
        panel.anchorMax = anchorMax;
        panel.offsetMin = Vector2.zero;
        panel.offsetMax = Vector2.zero;
        panel.GetComponent<Image>().color = color;
        return panel;
    }

    private static RectTransform CreateVerticalContainer(
        Transform parent,
        string name,
        Vector2 anchorMin,
        Vector2 anchorMax)
    {
        var containerObject = new GameObject(
            name,
            typeof(RectTransform),
            typeof(VerticalLayoutGroup));
        RectTransform container = containerObject.GetComponent<RectTransform>();
        container.SetParent(parent, false);
        container.anchorMin = anchorMin;
        container.anchorMax = anchorMax;
        container.offsetMin = Vector2.zero;
        container.offsetMax = Vector2.zero;
        VerticalLayoutGroup layout = containerObject.GetComponent<VerticalLayoutGroup>();
        layout.spacing = 7f;
        layout.padding = new RectOffset(2, 2, 2, 2);
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        return container;
    }

    private static Image CreateAnchoredImage(
        Transform parent,
        string name,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Color color)
    {
        return CosmicRaysUIFactory.CreateAnchoredImage(parent, name, anchorMin, anchorMax, color);
    }

    private static Button CreateAnchoredButton(
        Transform parent,
        string name,
        string label,
        UnityEngine.Events.UnityAction action,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Color accent)
    {
        Button button = CosmicRaysUIFactory.CreateButton(parent, name, label, action, 52f);
        RectTransform rect = (RectTransform)button.transform;
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        StyleButton(button, accent);
        return button;
    }

    private static void CreateCenteredImage(
        RectTransform parent,
        string name,
        float size,
        Color color)
    {
        var imageObject = new GameObject(name, typeof(RectTransform), typeof(Image));
        RectTransform rect = imageObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(size, size);
        Image image = imageObject.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
    }

    private static void CreateRing(
        RectTransform parent,
        float size,
        Color color)
    {
        var ringObject = new GameObject("Ring", typeof(RectTransform), typeof(Image));
        RectTransform rect = ringObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(size, size);
        Image image = ringObject.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;

        var outlineObject = new GameObject("Outline", typeof(RectTransform), typeof(Image));
        RectTransform outlineRect = outlineObject.GetComponent<RectTransform>();
        outlineRect.SetParent(rect, false);
        outlineRect.anchorMin = Vector2.zero;
        outlineRect.anchorMax = Vector2.one;
        outlineRect.offsetMin = new Vector2(1f, 1f);
        outlineRect.offsetMax = new Vector2(-1f, -1f);
        Image outline = outlineObject.GetComponent<Image>();
        outline.color = new Color(0.01f, 0.03f, 0.05f, 0.55f);
        outline.raycastTarget = false;
    }
}
