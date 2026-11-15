using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>Creates reusable UGUI controls used by the runtime-generated COSMIC RAYS screens.</summary>
public static class CosmicRaysUIFactory
{
    private static readonly Color PanelColor = new Color(0.015f, 0.04f, 0.09f, 0.96f);
    private static readonly Color ButtonColor = new Color(0.08f, 0.25f, 0.38f, 1f);
    private static Font _uiFont;
    private static Sprite _verticalFadeSprite;

    /// <summary>Creates a screen-space canvas with a safe-area content root and EventSystem.</summary>
    public static RectTransform CreateCanvasRoot(string name)
    {
        var canvasObject = new GameObject(
            name,
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster));

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.pixelPerfect = false;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        EnsureEventSystem();

        var safeAreaObject = new GameObject("SafeArea", typeof(RectTransform), typeof(SafeAreaFitter));
        RectTransform safeArea = safeAreaObject.GetComponent<RectTransform>();
        safeArea.SetParent(canvasObject.transform, false);
        safeArea.anchorMin = Vector2.zero;
        safeArea.anchorMax = Vector2.one;
        safeArea.offsetMin = Vector2.zero;
        safeArea.offsetMax = Vector2.zero;
        safeAreaObject.GetComponent<SafeAreaFitter>().ApplyNow();
        return safeArea;
    }

    /// <summary>Creates a full-stretch panel under a parent transform.</summary>
    public static RectTransform CreatePanel(Transform parent, string name, bool blocksRaycasts = true)
    {
        var panelObject = new GameObject(name, typeof(RectTransform), typeof(Image));
        RectTransform panel = panelObject.GetComponent<RectTransform>();
        panel.SetParent(parent, false);
        panel.anchorMin = Vector2.zero;
        panel.anchorMax = Vector2.one;
        panel.offsetMin = Vector2.zero;
        panel.offsetMax = Vector2.zero;

        Image background = panelObject.GetComponent<Image>();
        background.color = PanelColor;
        background.raycastTarget = blocksRaycasts;
        return panel;
    }

    /// <summary>Creates a full-stretch panel that fades from opaque at the bottom to clear at the top.</summary>
    public static RectTransform CreateGradientPanel(
        Transform parent,
        string name,
        bool blocksRaycasts,
        Color tint)
    {
        RectTransform panel = CreatePanel(parent, name, blocksRaycasts);
        Image image = panel.GetComponent<Image>();
        image.sprite = GetVerticalFadeSprite();
        image.type = Image.Type.Simple;
        image.color = tint;
        image.preserveAspect = false;
        return panel;
    }

    /// <summary>Creates a thin horizontal divider that participates in vertical layouts.</summary>
    public static Image CreateDivider(Transform parent, Color color, float height = 2f)
    {
        var dividerObject = new GameObject("Divider", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        RectTransform rect = dividerObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        LayoutElement layout = dividerObject.GetComponent<LayoutElement>();
        layout.minHeight = height;
        layout.preferredHeight = height;
        Image image = dividerObject.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    /// <summary>Creates a thin vertical accent line inside a control.</summary>
    public static Image CreateEdgeLine(Transform parent, Color color, float widthFraction = 0.006f)
    {
        Image line = CreateAnchoredImage(
            parent,
            "Accent Edge",
            new Vector2(0f, 0f),
            new Vector2(Mathf.Clamp01(widthFraction), 1f),
            color);
        line.raycastTarget = false;
        return line;
    }

    /// <summary>Creates a vertically arranged container within the standard screen margins.</summary>
    public static RectTransform CreateColumn(Transform parent, string name)
    {
        var columnObject = new GameObject(name, typeof(RectTransform), typeof(VerticalLayoutGroup));
        RectTransform column = columnObject.GetComponent<RectTransform>();
        column.SetParent(parent, false);
        column.anchorMin = new Vector2(0.08f, 0.06f);
        column.anchorMax = new Vector2(0.92f, 0.94f);
        column.offsetMin = Vector2.zero;
        column.offsetMax = Vector2.zero;

        VerticalLayoutGroup layout = columnObject.GetComponent<VerticalLayoutGroup>();
        layout.spacing = 12f;
        layout.padding = new RectOffset(12, 12, 12, 12);
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        return column;
    }

    /// <summary>Creates a scalable UGUI text label with a layout element.</summary>
    public static Text CreateLabel(
        Transform parent,
        string name,
        string value,
        int fontSize,
        float preferredHeight,
        TextAnchor alignment = TextAnchor.MiddleCenter)
    {
        var labelObject = new GameObject(name, typeof(RectTransform), typeof(Text), typeof(LayoutElement));
        RectTransform rect = labelObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);

        Text label = labelObject.GetComponent<Text>();
        label.font = GetUiFont();
        label.text = value;
        label.fontSize = fontSize;
        label.color = Color.white;
        label.alignment = alignment;
        label.horizontalOverflow = HorizontalWrapMode.Wrap;
        label.verticalOverflow = VerticalWrapMode.Overflow;
        label.raycastTarget = false;
        label.supportRichText = false;

        if (fontSize >= 30)
        {
            Shadow shadow = labelObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.75f);
            shadow.effectDistance = new Vector2(1.5f, -1.5f);
            shadow.useGraphicAlpha = true;
        }

        LayoutElement layout = labelObject.GetComponent<LayoutElement>();
        layout.minHeight = Mathf.Min(44f, preferredHeight);
        layout.preferredHeight = preferredHeight;
        return label;
    }

    /// <summary>Creates a selectable UGUI button and attaches its click callback.</summary>
    public static Button CreateButton(
        Transform parent,
        string name,
        string label,
        UnityEngine.Events.UnityAction onClick,
        float preferredHeight = 62f)
    {
        var buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);

        Image image = buttonObject.GetComponent<Image>();
        image.color = Color.white;

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(CosmicRaysAudio.PlayClick);
        if (onClick != null)
        {
            button.onClick.AddListener(onClick);
        }
        button.navigation = new Navigation { mode = Navigation.Mode.Automatic };

        ColorBlock colors = button.colors;
        colors.normalColor = ButtonColor;
        colors.highlightedColor = new Color(0.16f, 0.46f, 0.62f, 1f);
        colors.selectedColor = new Color(0.22f, 0.58f, 0.74f, 1f);
        colors.pressedColor = new Color(0.04f, 0.15f, 0.22f, 1f);
        colors.disabledColor = new Color(0.15f, 0.19f, 0.22f, 0.72f);
        colors.fadeDuration = 0.08f;
        button.colors = colors;

        Outline outline = buttonObject.AddComponent<Outline>();
        outline.effectColor = new Color(0.25f, 0.86f, 0.96f, 0.38f);
        outline.effectDistance = new Vector2(1.2f, -1.2f);
        outline.useGraphicAlpha = true;

        LayoutElement layout = buttonObject.GetComponent<LayoutElement>();
        layout.minHeight = 52f;
        layout.preferredHeight = preferredHeight;

        CreateEdgeLine(buttonObject.transform, new Color(1f, 1f, 1f, 0.32f), 0.004f);

        Text buttonLabel = CreateLabel(buttonObject.transform, "Label", label, 25, preferredHeight - 4f);
        Shadow labelShadow = buttonLabel.gameObject.AddComponent<Shadow>();
        labelShadow.effectColor = new Color(0f, 0f, 0f, 0.65f);
        labelShadow.effectDistance = new Vector2(1f, -1f);
        RectTransform labelRect = buttonLabel.rectTransform;
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(14f, 0f);
        labelRect.offsetMax = new Vector2(-14f, 0f);
        return button;
    }

    /// <summary>Creates a normalized horizontal slider with a generated track and handle.</summary>
    public static Slider CreateSlider(Transform parent, string name, float initialValue)
    {
        var sliderObject = new GameObject(name, typeof(RectTransform), typeof(Slider), typeof(LayoutElement));
        RectTransform sliderRect = sliderObject.GetComponent<RectTransform>();
        sliderRect.SetParent(parent, false);

        LayoutElement layout = sliderObject.GetComponent<LayoutElement>();
        layout.minHeight = 48f;
        layout.preferredHeight = 56f;

        var backgroundObject = new GameObject("Background", typeof(RectTransform), typeof(Image));
        RectTransform backgroundRect = backgroundObject.GetComponent<RectTransform>();
        backgroundRect.SetParent(sliderObject.transform, false);
        Stretch(backgroundRect, new Vector2(0f, 0.35f), new Vector2(1f, 0.65f));
        backgroundObject.GetComponent<Image>().color = new Color(0.12f, 0.17f, 0.22f, 1f);

        var fillAreaObject = new GameObject("Fill Area", typeof(RectTransform));
        RectTransform fillArea = fillAreaObject.GetComponent<RectTransform>();
        fillArea.SetParent(sliderObject.transform, false);
        Stretch(fillArea, new Vector2(0f, 0.35f), new Vector2(1f, 0.65f));

        var fillObject = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        RectTransform fillRect = fillObject.GetComponent<RectTransform>();
        fillRect.SetParent(fillArea, false);
        Stretch(fillRect, Vector2.zero, Vector2.one);
        fillObject.GetComponent<Image>().color = new Color(0.18f, 0.62f, 0.78f, 1f);

        var handleAreaObject = new GameObject("Handle Slide Area", typeof(RectTransform));
        RectTransform handleArea = handleAreaObject.GetComponent<RectTransform>();
        handleArea.SetParent(sliderObject.transform, false);
        Stretch(handleArea, new Vector2(0f, 0.1f), new Vector2(1f, 0.9f));
        handleArea.offsetMin = new Vector2(10f, 0f);
        handleArea.offsetMax = new Vector2(-10f, 0f);

        var handleObject = new GameObject("Handle", typeof(RectTransform), typeof(Image));
        RectTransform handleRect = handleObject.GetComponent<RectTransform>();
        handleRect.SetParent(handleArea, false);
        handleRect.anchorMin = new Vector2(0.5f, 0f);
        handleRect.anchorMax = new Vector2(0.5f, 1f);
        handleRect.pivot = new Vector2(0.5f, 0.5f);
        handleRect.sizeDelta = new Vector2(24f, 0f);
        Image handleImage = handleObject.GetComponent<Image>();
        handleImage.color = Color.white;

        Slider slider = sliderObject.GetComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = Mathf.Clamp01(initialValue);
        slider.fillRect = fillRect;
        slider.handleRect = handleRect;
        slider.targetGraphic = handleImage;
        slider.direction = Slider.Direction.LeftToRight;
        slider.navigation = new Navigation { mode = Navigation.Mode.Automatic };
        return slider;
    }

    /// <summary>Creates a scroll view and returns its vertically laid-out content transform.</summary>
    public static RectTransform CreateScrollContent(Transform parent, string name)
    {
        var scrollObject = new GameObject(name, typeof(RectTransform), typeof(ScrollRect), typeof(LayoutElement));
        RectTransform scrollRect = scrollObject.GetComponent<RectTransform>();
        scrollRect.SetParent(parent, false);
        scrollRect.anchorMin = new Vector2(0.08f, 0.16f);
        scrollRect.anchorMax = new Vector2(0.92f, 0.78f);
        scrollRect.offsetMin = Vector2.zero;
        scrollRect.offsetMax = Vector2.zero;
        LayoutElement scrollLayout = scrollObject.GetComponent<LayoutElement>();
        scrollLayout.minHeight = 220f;
        scrollLayout.preferredHeight = 450f;
        scrollLayout.flexibleHeight = 1f;

        var viewportObject = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(RectMask2D));
        RectTransform viewport = viewportObject.GetComponent<RectTransform>();
        viewport.SetParent(scrollRect, false);
        Stretch(viewport, Vector2.zero, Vector2.one);
        viewportObject.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.08f);
        viewportObject.GetComponent<Image>().raycastTarget = true;

        var contentObject = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        RectTransform content = contentObject.GetComponent<RectTransform>();
        content.SetParent(viewport, false);
        content.anchorMin = new Vector2(0f, 1f);
        content.anchorMax = new Vector2(1f, 1f);
        content.pivot = new Vector2(0.5f, 1f);
        content.anchoredPosition = Vector2.zero;
        content.sizeDelta = Vector2.zero;

        VerticalLayoutGroup layout = contentObject.GetComponent<VerticalLayoutGroup>();
        layout.spacing = 8f;
        layout.padding = new RectOffset(10, 10, 10, 10);
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        ContentSizeFitter fitter = contentObject.GetComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        ScrollRect scroll = scrollObject.GetComponent<ScrollRect>();
        scroll.viewport = viewport;
        scroll.content = content;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        return content;
    }

    /// <summary>Creates a text label positioned by normalized anchors.</summary>
    public static Text CreateAnchoredLabel(
        Transform parent,
        string name,
        string value,
        Vector2 anchorMin,
        Vector2 anchorMax,
        int fontSize,
        TextAnchor alignment)
    {
        var labelObject = new GameObject(name, typeof(RectTransform), typeof(Text));
        RectTransform rect = labelObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Text label = labelObject.GetComponent<Text>();
        label.font = GetUiFont();
        label.text = value;
        label.fontSize = fontSize;
        label.color = Color.white;
        label.alignment = alignment;
        label.horizontalOverflow = HorizontalWrapMode.Wrap;
        label.verticalOverflow = VerticalWrapMode.Overflow;
        label.raycastTarget = false;
        label.supportRichText = false;

        if (fontSize >= 30)
        {
            Shadow shadow = labelObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.75f);
            shadow.effectDistance = new Vector2(1.5f, -1.5f);
            shadow.useGraphicAlpha = true;
        }

        return label;
    }

    /// <summary>Creates a non-interactive image positioned by normalized anchors.</summary>
    public static Image CreateAnchoredImage(
        Transform parent,
        string name,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Color color)
    {
        var imageObject = new GameObject(name, typeof(RectTransform), typeof(Image));
        RectTransform rect = imageObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        Image image = imageObject.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    /// <summary>Sets the EventSystem selection when a selectable control is available.</summary>
    public static void Focus(Selectable selectable)
    {
        if (selectable != null && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(selectable.gameObject);
        }
    }

    /// <summary>Sets the EventSystem selection to the provided button.</summary>
    public static void Focus(Button button)
    {
        Focus((Selectable)button);
    }

    private static void EnsureEventSystem()
    {
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null)
        {
            var eventSystemObject = new GameObject("EventSystem", typeof(EventSystem));
            eventSystem = eventSystemObject.GetComponent<EventSystem>();
        }

        if (eventSystem.GetComponent<BaseInputModule>() != null)
        {
            return;
        }

        Type inputSystemModule = Type.GetType(
            "UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem",
            false);

        if (inputSystemModule != null && typeof(BaseInputModule).IsAssignableFrom(inputSystemModule))
        {
            Component module = eventSystem.gameObject.AddComponent(inputSystemModule);
            MethodInfo assignDefaultActions = inputSystemModule.GetMethod(
                "AssignDefaultActions",
                BindingFlags.Instance | BindingFlags.Public);
            if (assignDefaultActions != null)
            {
                assignDefaultActions.Invoke(module, null);
            }
            else
            {
                Debug.LogWarning(
                    "InputSystemUIInputModule was added without default actions. Configure its UI actions in the generated EventSystem.");
            }
        }
        else
        {
            eventSystem.gameObject.AddComponent<StandaloneInputModule>();
        }
    }

    private static Font GetUiFont()
    {
        if (_uiFont == null)
        {
            _uiFont = Resources.Load<Font>("CosmicRays/Fonts/Rajdhani-SemiBold");
            if (_uiFont == null)
            {
                _uiFont = Resources.Load<Font>("CosmicRays/Fonts/Rajdhani-Regular");
            }
            if (_uiFont == null)
            {
                _uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }
            if (_uiFont == null)
            {
                _uiFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }
            if (_uiFont == null)
            {
                Debug.LogError("No built-in UGUI font was available. Assign a font to the generated Text components.");
            }
        }
        return _uiFont;
    }

    private static Sprite GetVerticalFadeSprite()
    {
        if (_verticalFadeSprite != null)
        {
            return _verticalFadeSprite;
        }

        const int width = 4;
        const int height = 256;
        var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.name = "COSMIC RAYS Vertical Fade";
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        var pixels = new Color32[width * height];
        for (int y = 0; y < height; y++)
        {
            float alpha = 1f - y / (float)(height - 1);
            alpha = Mathf.SmoothStep(0f, 1f, alpha);
            byte value = (byte)Mathf.RoundToInt(alpha * 255f);
            for (int x = 0; x < width; x++)
            {
                pixels[y * width + x] = new Color32(255, 255, 255, value);
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply(false, true);
        _verticalFadeSprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, width, height),
            new Vector2(0.5f, 0.5f),
            100f,
            0,
            SpriteMeshType.FullRect);
        _verticalFadeSprite.name = "COSMIC RAYS Vertical Fade";
        return _verticalFadeSprite;
    }

    private static void Stretch(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
