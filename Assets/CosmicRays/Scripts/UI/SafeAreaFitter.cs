using UnityEngine;

/// <summary>Applies the current display safe area to this RectTransform.</summary>
public sealed class SafeAreaFitter : MonoBehaviour
{
    private Rect _lastSafeArea;
    private int _lastScreenWidth;
    private int _lastScreenHeight;

    private void Awake()
    {
        ApplyNow();
    }

    private void Update()
    {
        if (_lastSafeArea != Screen.safeArea ||
            _lastScreenWidth != Screen.width ||
            _lastScreenHeight != Screen.height)
        {
            ApplyNow();
        }
    }

    /// <summary>Immediately updates the RectTransform anchors to match Screen.safeArea.</summary>
    public void ApplyNow()
    {
        Rect safeArea = Screen.safeArea;
        RectTransform rect = transform as RectTransform;
        if (rect == null || Screen.width <= 0 || Screen.height <= 0)
        {
            return;
        }

        rect.anchorMin = new Vector2(safeArea.xMin / Screen.width, safeArea.yMin / Screen.height);
        rect.anchorMax = new Vector2(safeArea.xMax / Screen.width, safeArea.yMax / Screen.height);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        _lastSafeArea = safeArea;
        _lastScreenWidth = Screen.width;
        _lastScreenHeight = Screen.height;
    }
}
