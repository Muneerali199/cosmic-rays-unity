using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Creates and validates the starter COSMIC RAYS data assets and placeholder scenes.</summary>
public static class CosmicRaysProjectSetup
{
    private const string Root = "Assets/CosmicRays";
    private const string ResourcesRoot = Root + "/Resources/CosmicRays";
    private const string MainMenuScenePath = Root + "/Scenes/MainMenu.unity";
    private const string GameplayScenePath = Root + "/Scenes/Gameplay.unity";

    /// <summary>Creates missing starter data/scenes and registers scenes without replacing existing assets.</summary>
    [MenuItem("Tools/COSMIC RAYS/Setup Scenes and Data")]
    public static void SetupScenesAndData()
    {
        EnsureFolder(Root);
        EnsureFolder(Root + "/Scenes");
        EnsureFolder(Root + "/Scripts");
        EnsureFolder(Root + "/Editor");
        EnsureFolder(ResourcesRoot);
        EnsureFolder(ResourcesRoot + "/Data");
        EnsureFolder(ResourcesRoot + "/Items");
        EnsureFolder(ResourcesRoot + "/Objectives");

        CreateDefaultData();
        CreateSceneIfMissing(MainMenuScenePath, CreateMainMenuScene);
        CreateSceneIfMissing(GameplayScenePath, CreateGameplayScene);
        AddScenesToBuildSettings();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("COSMIC RAYS setup finished. Existing scenes and data assets were preserved.");
        ValidateSetup();
    }

    /// <summary>Checks required starter assets, generated scenes, and Build Settings entries.</summary>
    [MenuItem("Tools/COSMIC RAYS/Validate Setup")]
    public static void ValidateSetup()
    {
        bool valid = true;
        valid &= CheckAsset<SurvivalConfig>(ResourcesRoot + "/Data/SurvivalConfig.asset");
        valid &= CheckAsset<InventoryItemDefinition>(ResourcesRoot + "/Items/OxygenCanister.asset");
        valid &= CheckAsset<InventoryItemDefinition>(ResourcesRoot + "/Items/WaterRation.asset");
        valid &= CheckAsset<InventoryItemDefinition>(ResourcesRoot + "/Items/EnergyCell.asset");
        string[] objectiveAssets =
        {
            "Month01_InspectBase",
            "Month02_LifeSupport",
            "Month03_EnergyHeating",
            "Month04_SEPTechnology",
            "Month05_HabitatResearch",
            "Month06_FinalAssessment"
        };
        for (int i = 0; i < objectiveAssets.Length; i++)
        {
            valid &= CheckAsset<MissionObjectiveDefinition>(
                ResourcesRoot + "/Objectives/" + objectiveAssets[i] + ".asset");
        }
        valid &= CheckScene(MainMenuScenePath);
        valid &= CheckScene(GameplayScenePath);

        if (valid)
        {
            Debug.Log("COSMIC RAYS setup validation passed. This checks generated assets and scene registration, not runtime behavior.");
        }
        else
        {
            Debug.LogError("COSMIC RAYS setup validation found missing assets or scenes.");
        }
    }

    private static void CreateDefaultData()
    {
        string configPath = ResourcesRoot + "/Data/SurvivalConfig.asset";
        if (AssetDatabase.LoadAssetAtPath<SurvivalConfig>(configPath) == null)
        {
            if (AssetDatabase.LoadMainAssetAtPath(configPath) != null)
            {
                Debug.LogError("A different asset already uses the setup path; it was preserved: " + configPath);
            }
            else
            {
                SurvivalConfig config = ScriptableObject.CreateInstance<SurvivalConfig>();
                config.maximumOxygen = 100f;
                config.maximumWater = 100f;
                config.maximumEnergy = 100f;
                config.startingOxygen = 100f;
                config.startingWater = 100f;
                config.startingEnergy = 100f;
                config.oxygenUsePerSecond = 0.02f;
                config.waterUsePerSecond = 0.01f;
                config.energyUsePerSecond = 0.015f;
                config.warningThreshold = 0.25f;
                config.failMissionWhenResourceIsEmpty = true;
                config.startingInventory = new[]
                {
                    new StartingInventoryItem { itemId = "oxygen_canister", quantity = 2 },
                    new StartingInventoryItem { itemId = "water_ration", quantity = 2 },
                    new StartingInventoryItem { itemId = "energy_cell", quantity = 2 }
                };
                AssetDatabase.CreateAsset(config, configPath);
            }
        }

        CreateItemIfMissing(
            "OxygenCanister",
            "oxygen_canister",
            "Oxygen Canister",
            "A supply canister used to restore oxygen.",
            CosmicResource.Oxygen,
            35f);
        CreateItemIfMissing(
            "WaterRation",
            "water_ration",
            "Water Ration",
            "A sealed water supply used to restore water.",
            CosmicResource.Water,
            30f);
        CreateItemIfMissing(
            "EnergyCell",
            "energy_cell",
            "Energy Cell",
            "A portable cell used to restore energy.",
            CosmicResource.Energy,
            30f);

        CreateObjectiveIfMissing(
            "Month01_InspectBase",
            "month_01_inspect_base",
            1,
            "Inspect the research base",
            "Inspect the base and test its atmosphere.");
        CreateObjectiveIfMissing(
            "Month02_LifeSupport",
            "month_02_life_support",
            2,
            "Establish life-support processing",
            "Establish oxygen, water, and air processing.");
        CreateObjectiveIfMissing(
            "Month03_EnergyHeating",
            "month_03_energy_heating",
            3,
            "Secure energy and heating",
            "Make energy supply and habitat heating reliable.");
        CreateObjectiveIfMissing(
            "Month04_SEPTechnology",
            "month_04_sep_technology",
            4,
            "Test SEP technology",
            "Install and test the experimental SEP technology.");
        CreateObjectiveIfMissing(
            "Month05_HabitatResearch",
            "month_05_habitat_research",
            5,
            "Test the habitat and conduct research",
            "Test the habitat and run the planned research.");
        CreateObjectiveIfMissing(
            "Month06_FinalAssessment",
            "month_06_final_assessment",
            6,
            "Complete the final assessment",
            "Complete the final mission assessment and prepare for return.");
    }

    private static void CreateItemIfMissing(
        string assetName,
        string itemId,
        string displayName,
        string description,
        CosmicResource resource,
        float restoreAmount)
    {
        string path = ResourcesRoot + "/Items/" + assetName + ".asset";
        if (AssetDatabase.LoadAssetAtPath<InventoryItemDefinition>(path) != null)
        {
            return;
        }
        if (AssetDatabase.LoadMainAssetAtPath(path) != null)
        {
            Debug.LogError("A different asset already uses the setup path; it was preserved: " + path);
            return;
        }

        InventoryItemDefinition item = ScriptableObject.CreateInstance<InventoryItemDefinition>();
        item.itemId = itemId;
        item.displayName = displayName;
        item.description = description;
        item.restoresResource = resource;
        item.restoreAmount = restoreAmount;
        item.maximumStack = 99;
        AssetDatabase.CreateAsset(item, path);
    }

    private static void CreateObjectiveIfMissing(
        string assetName,
        string objectiveId,
        int month,
        string title,
        string description)
    {
        string path = ResourcesRoot + "/Objectives/" + assetName + ".asset";
        if (AssetDatabase.LoadAssetAtPath<MissionObjectiveDefinition>(path) != null)
        {
            return;
        }
        if (AssetDatabase.LoadMainAssetAtPath(path) != null)
        {
            Debug.LogError("A different asset already uses the setup path; it was preserved: " + path);
            return;
        }

        MissionObjectiveDefinition objective = ScriptableObject.CreateInstance<MissionObjectiveDefinition>();
        objective.objectiveId = objectiveId;
        objective.missionMonth = month;
        objective.title = title;
        objective.description = description;
        AssetDatabase.CreateAsset(objective, path);
    }

    private static void CreateSceneIfMissing(string path, Action createContents)
    {
        UnityEngine.Object existingAsset = AssetDatabase.LoadMainAssetAtPath(path);
        if (existingAsset != null)
        {
            if (existingAsset is SceneAsset)
            {
                Debug.Log("Preserved existing scene: " + path);
            }
            else
            {
                Debug.LogError("A different asset already uses the generated scene path; it was preserved: " + path);
            }
            return;
        }

        Scene previousActiveScene = SceneManager.GetActiveScene();
        bool canRestorePrevious = previousActiveScene.IsValid() &&
                                  previousActiveScene.isLoaded &&
                                  !string.IsNullOrEmpty(previousActiveScene.path);
        NewSceneMode mode = !Application.isBatchMode && canRestorePrevious
            ? NewSceneMode.Additive
            : NewSceneMode.Single;
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, mode);
        try
        {
            if (mode == NewSceneMode.Additive && !SceneManager.SetActiveScene(scene))
            {
                Debug.LogError("Could not activate the new scene for setup: " + path);
                return;
            }
            createContents();
            if (!EditorSceneManager.SaveScene(scene, path))
            {
                Debug.LogError("Unity did not save the generated scene: " + path);
            }
        }
        finally
        {
            if (mode == NewSceneMode.Additive && scene.IsValid() && scene.isLoaded)
            {
                EditorSceneManager.CloseScene(scene, true);
            }
            if (canRestorePrevious && previousActiveScene.IsValid() && previousActiveScene.isLoaded)
            {
                SceneManager.SetActiveScene(previousActiveScene);
            }
        }
    }

    private static void CreateMainMenuScene()
    {
        var root = new GameObject("COSMIC RAYS Main Menu");
        root.AddComponent<MainMenuController>();
    }

    private static void CreateGameplayScene()
    {
        var root = new GameObject("COSMIC RAYS Gameplay Systems");
        root.AddComponent<CosmicRaysGameplayController>();

        var cameraObject = new GameObject("Gameplay Camera", typeof(Camera), typeof(AudioListener));
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(0f, 2f, -10f);
        cameraObject.transform.rotation = Quaternion.Euler(8f, 0f, 0f);
        Camera camera = cameraObject.GetComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.008f, 0.02f, 0.045f, 1f);

        var lightObject = new GameObject("Placeholder Directional Light", typeof(Light));
        Light light = lightObject.GetComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1f;
        lightObject.transform.rotation = Quaternion.Euler(40f, -30f, 0f);
    }

    private static void AddScenesToBuildSettings()
    {
        EditorBuildSettingsScene[] existing = EditorBuildSettings.scenes;
        var scenes = new List<EditorBuildSettingsScene>();
        AddBuildSceneIfMissing(scenes, MainMenuScenePath);
        AddBuildSceneIfMissing(scenes, GameplayScenePath);
        for (int i = 0; i < existing.Length; i++)
        {
            if (!string.Equals(existing[i].path, MainMenuScenePath, StringComparison.Ordinal) &&
                !string.Equals(existing[i].path, GameplayScenePath, StringComparison.Ordinal))
            {
                scenes.Add(existing[i]);
            }
        }
        EditorBuildSettings.scenes = scenes.ToArray();
    }

    private static void AddBuildSceneIfMissing(List<EditorBuildSettingsScene> scenes, string path)
    {
        for (int i = 0; i < scenes.Count; i++)
        {
            if (string.Equals(scenes[i].path, path, StringComparison.Ordinal))
            {
                if (!scenes[i].enabled)
                {
                    scenes[i] = new EditorBuildSettingsScene(path, true);
                }
                return;
            }
        }
        scenes.Add(new EditorBuildSettingsScene(path, true));
    }

    private static void EnsureFolder(string path)
    {
        string[] parts = path.Split('/');
        string current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(current, parts[i]);
            }
            current = next;
        }
    }

    private static bool CheckAsset<T>(string path) where T : UnityEngine.Object
    {
        if (AssetDatabase.LoadAssetAtPath<T>(path) != null)
        {
            return true;
        }
        Debug.LogError("Missing COSMIC RAYS setup asset: " + path);
        return false;
    }

    private static bool CheckScene(string path)
    {
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) == null)
        {
            Debug.LogError("Missing COSMIC RAYS scene: " + path);
            return false;
        }

        EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
        for (int i = 0; i < scenes.Length; i++)
        {
            if (scenes[i].enabled && string.Equals(scenes[i].path, path, StringComparison.Ordinal))
            {
                return true;
            }
        }

        Debug.LogError("Scene is not enabled in Build Settings: " + path);
        return false;
    }
}
