using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Builds the runtime settings screen and connects controls to saved settings.</summary>
public static class CosmicRaysSettingsUI
{
    /// <summary>Builds the settings controls and returns the Back button.</summary>
    public static Button Build(RectTransform panel, Action onBack)
    {
        if (panel == null)
        {
            throw new ArgumentNullException(nameof(panel));
        }
        if (onBack == null)
        {
            throw new ArgumentNullException(nameof(onBack));
        }

        RectTransform column = CosmicRaysUIFactory.CreateColumn(panel, "Settings Content");
        CosmicRaysUIFactory.CreateLabel(column, "Title", "SETTINGS", 38, 72f);

        CosmicRaysUIFactory.CreateLabel(column, "AudioLabel", "Master audio volume", 24, 48f);
        Slider volumeSlider = CosmicRaysUIFactory.CreateSlider(
            column,
            "MasterVolumeSlider",
            AudioListener.volume);
        volumeSlider.onValueChanged.AddListener(CosmicRaysSettings.SetMasterVolume);

        Button qualityButton = CosmicRaysUIFactory.CreateButton(
            column,
            "QualityButton",
            "Graphics quality: " + CosmicRaysSettings.QualityLabel,
            null);
        qualityButton.interactable = QualitySettings.names.Length > 1;
        if (!qualityButton.interactable)
        {
            qualityButton.GetComponentInChildren<Text>().text = "Graphics quality: unavailable";
        }
        qualityButton.onClick.AddListener(() =>
        {
            CosmicRaysSettings.CycleQuality();
            qualityButton.GetComponentInChildren<Text>().text =
                "Graphics quality: " + CosmicRaysSettings.QualityLabel;
        });

        Button resolutionButton = CosmicRaysUIFactory.CreateButton(
            column,
            "ResolutionButton",
            "Resolution: " + CosmicRaysSettings.ResolutionLabel + " (change)",
            null);
        Resolution[] availableResolutions = Screen.resolutions;
        resolutionButton.interactable = availableResolutions != null && availableResolutions.Length > 1;
        if (!resolutionButton.interactable)
        {
            resolutionButton.GetComponentInChildren<Text>().text = "Resolution: unavailable";
        }
        resolutionButton.onClick.AddListener(() =>
        {
            CosmicRaysSettings.CycleResolution();
            resolutionButton.GetComponentInChildren<Text>().text =
                "Resolution: " + CosmicRaysSettings.ResolutionLabel + " (change)";
        });

        Button fullscreenButton = CosmicRaysUIFactory.CreateButton(
            column,
            "FullscreenButton",
            CosmicRaysSettings.FullscreenLabel,
            null);
        fullscreenButton.onClick.AddListener(() =>
        {
            CosmicRaysSettings.ToggleFullscreen();
            fullscreenButton.GetComponentInChildren<Text>().text = CosmicRaysSettings.FullscreenLabel;
        });

        Button backButton = CosmicRaysUIFactory.CreateButton(column, "BackButton", "Back", () => onBack());
        CosmicRaysUIFactory.Focus(volumeSlider);
        return backButton;
    }
}
