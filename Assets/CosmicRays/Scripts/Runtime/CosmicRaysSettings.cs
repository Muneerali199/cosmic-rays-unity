using UnityEngine;

/// <summary>Persists master volume and platform-supported graphics preferences.</summary>
public static class CosmicRaysSettings
{
    private const string MasterVolumeKey = "CosmicRays.MasterVolume";
    private const string QualityKey = "CosmicRays.Quality";
    private const string ResolutionWidthKey = "CosmicRays.ResolutionWidth";
    private const string ResolutionHeightKey = "CosmicRays.ResolutionHeight";
    private const string FullscreenKey = "CosmicRays.Fullscreen";
    private static bool _initialized;

    /// <summary>Applies saved user settings once during startup.</summary>
    public static void Initialize()
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;
        AudioListener.volume = Mathf.Clamp01(PlayerPrefs.GetFloat(MasterVolumeKey, 1f));

        int qualityIndex = PlayerPrefs.GetInt(QualityKey, QualitySettings.GetQualityLevel());
        qualityIndex = Mathf.Clamp(qualityIndex, 0, Mathf.Max(0, QualitySettings.names.Length - 1));
        if (QualitySettings.names.Length > 0)
        {
            QualitySettings.SetQualityLevel(qualityIndex, true);
        }

        Screen.fullScreen = PlayerPrefs.GetInt(FullscreenKey, Screen.fullScreen ? 1 : 0) != 0;
        int savedWidth = PlayerPrefs.GetInt(ResolutionWidthKey, 0);
        int savedHeight = PlayerPrefs.GetInt(ResolutionHeightKey, 0);
        if (savedWidth > 0 && savedHeight > 0)
        {
            Screen.SetResolution(savedWidth, savedHeight, Screen.fullScreen);
        }
    }

    /// <summary>Sets and persists master volume in the range from zero to one.</summary>
    public static void SetMasterVolume(float value)
    {
        AudioListener.volume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(MasterVolumeKey, AudioListener.volume);
        PlayerPrefs.Save();
    }

    /// <summary>Current quality-level label or an unavailable message on unsupported platforms.</summary>
    public static string QualityLabel
    {
        get
        {
            string[] names = QualitySettings.names;
            int index = QualitySettings.GetQualityLevel();
            return names.Length == 0 ? "Unavailable" : names[Mathf.Clamp(index, 0, names.Length - 1)];
        }
    }

    /// <summary>Cycles to the next quality level when quality settings are available.</summary>
    public static void CycleQuality()
    {
        int count = QualitySettings.names.Length;
        if (count == 0)
        {
            return;
        }

        int next = (QualitySettings.GetQualityLevel() + 1) % count;
        QualitySettings.SetQualityLevel(next, true);
        PlayerPrefs.SetInt(QualityKey, next);
        PlayerPrefs.Save();
    }

    /// <summary>Current screen resolution label.</summary>
    public static string ResolutionLabel
    {
        get
        {
            return Screen.width + " x " + Screen.height;
        }
    }

    /// <summary>Cycles through available resolutions and persists the selection.</summary>
    public static void CycleResolution()
    {
        Resolution[] resolutions = Screen.resolutions;
        if (resolutions == null || resolutions.Length == 0)
        {
            return;
        }

        int current = FindCurrentResolution(resolutions);
        int next = current < 0 ? 0 : (current + 1) % resolutions.Length;
        Resolution target = resolutions[next];
        Screen.SetResolution(target.width, target.height, Screen.fullScreen);
        PlayerPrefs.SetInt(ResolutionWidthKey, target.width);
        PlayerPrefs.SetInt(ResolutionHeightKey, target.height);
        PlayerPrefs.Save();
    }

    /// <summary>Toggles and persists fullscreen mode.</summary>
    public static void ToggleFullscreen()
    {
        Screen.fullScreen = !Screen.fullScreen;
        PlayerPrefs.SetInt(FullscreenKey, Screen.fullScreen ? 1 : 0);
        PlayerPrefs.Save();
    }

    /// <summary>Current fullscreen status label.</summary>
    public static string FullscreenLabel
    {
        get { return "Fullscreen: " + (Screen.fullScreen ? "On" : "Off"); }
    }

    private static int FindCurrentResolution(Resolution[] resolutions)
    {
        for (int i = 0; i < resolutions.Length; i++)
        {
            if (resolutions[i].width == Screen.width && resolutions[i].height == Screen.height)
            {
                return i;
            }
        }
        return -1;
    }
}
