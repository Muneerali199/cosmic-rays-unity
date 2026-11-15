using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Builds the complete generated scene set, with the story prologue first.</summary>
public static class CosmicRaysPlayableSetup
{
    private const string Root = "Assets/CosmicRays";
    private const string ScenesRoot = Root + "/Scenes";
    private const string PrologueScenePath = ScenesRoot + "/Prologue.unity";
    private const string MainMenuScenePath = ScenesRoot + "/MainMenu.unity";
    private const string GameplayScenePath = ScenesRoot + "/Gameplay.unity";
    private const string TemplateSampleScenePath = "Assets/Scenes/SampleScene.unity";

    /// <summary>Creates all generated data and scenes, then places Prologue first in Build Settings.</summary>
    [MenuItem("Tools/COSMIC RAYS/Build Playable Project")]
    public static void BuildPlayableProject()
    {
        CosmicRaysProjectSetup.SetupScenesAndData();
        EnsureFolder(Root);
        EnsureFolder(ScenesRoot);
        CreatePrologueSceneIfMissing();
        RegisterBuildScenes();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        ValidatePlayableProject();
        Debug.Log("COSMIC RAYS playable project setup finished.");
    }

    /// <summary>Checks the prologue, campaign scenes, and required runtime scripts.</summary>
    [MenuItem("Tools/COSMIC RAYS/Validate Playable Project")]
    public static void ValidatePlayableProject()
    {
        bool valid = true;
        valid &= CheckScene(PrologueScenePath);
        valid &= CheckScene(MainMenuScenePath);
        valid &= CheckScene(GameplayScenePath);
        valid &= CheckScript("Assets/CosmicRays/Scripts/Runtime/CosmicRaysPrologueController.cs");
        valid &= CheckScript("Assets/CosmicRays/Scripts/Runtime/MagneticRoutingPuzzle.cs");

        if (valid)
        {
            Debug.Log("COSMIC RAYS playable-project validation passed.");
        }
        else
        {
            Debug.LogError("COSMIC RAYS playable-project validation found missing scenes or scripts.");
        }
    }

    private static void CreatePrologueSceneIfMissing()
    {
        UnityEngine.Object existing = AssetDatabase.LoadMainAssetAtPath(PrologueScenePath);
        if (existing != null)
        {
            if (existing is SceneAsset)
            {
                Debug.Log("Preserved existing prologue scene: " + PrologueScenePath);
            }
            else
            {
                Debug.LogError("A non-scene asset occupies the prologue scene path: " + PrologueScenePath);
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
                Debug.LogError("Could not activate the new COSMIC RAYS prologue scene.");
                return;
            }

            var root = new GameObject("COSMIC RAYS Prologue");
            root.AddComponent<CosmicRaysPrologueController>();

            var cameraObject = new GameObject("Prologue Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 12f, 0f);
            cameraObject.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            Camera camera = cameraObject.GetComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 6.2f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.015f, 0.035f, 0.05f, 1f);

            var lightObject = new GameObject("Prologue Directional Light", typeof(Light));
            Light light = lightObject.GetComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            lightObject.transform.rotation = Quaternion.Euler(55f, -35f, 0f);

            if (!EditorSceneManager.SaveScene(scene, PrologueScenePath))
            {
                Debug.LogError("Unity did not save the generated prologue scene.");
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

    private static void RegisterBuildScenes()
    {
        EditorBuildSettingsScene[] existing = EditorBuildSettings.scenes;
        var ordered = new List<EditorBuildSettingsScene>();
        AddSceneIfAvailable(ordered, PrologueScenePath);
        AddSceneIfAvailable(ordered, MainMenuScenePath);
        AddSceneIfAvailable(ordered, GameplayScenePath);

        for (int index = 0; index < existing.Length; index++)
        {
            string path = existing[index].path;
            if (!IsCosmicRaysScene(path) &&
                !string.Equals(path, TemplateSampleScenePath, StringComparison.Ordinal))
            {
                ordered.Add(existing[index]);
            }
        }
        EditorBuildSettings.scenes = ordered.ToArray();
    }

    private static void AddSceneIfAvailable(List<EditorBuildSettingsScene> scenes, string path)
    {
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) != null)
        {
            scenes.Add(new EditorBuildSettingsScene(path, true));
        }
    }

    private static bool IsCosmicRaysScene(string path)
    {
        return string.Equals(path, PrologueScenePath, StringComparison.Ordinal) ||
               string.Equals(path, MainMenuScenePath, StringComparison.Ordinal) ||
               string.Equals(path, GameplayScenePath, StringComparison.Ordinal);
    }

    private static void EnsureFolder(string path)
    {
        string[] parts = path.Split('/');
        string current = parts[0];
        for (int index = 1; index < parts.Length; index++)
        {
            string next = current + "/" + parts[index];
            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(current, parts[index]);
            }
            current = next;
        }
    }

    private static bool CheckScene(string path)
    {
        SceneAsset scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
        if (scene == null)
        {
            Debug.LogError("Missing COSMIC RAYS scene: " + path);
            return false;
        }

        EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
        for (int index = 0; index < scenes.Length; index++)
        {
            if (scenes[index].enabled && string.Equals(scenes[index].path, path, StringComparison.Ordinal))
            {
                return true;
            }
        }

        Debug.LogError("COSMIC RAYS scene is not enabled in Build Settings: " + path);
        return false;
    }

    private static bool CheckScript(string path)
    {
        if (AssetDatabase.LoadAssetAtPath<MonoScript>(path) != null)
        {
            return true;
        }
        Debug.LogError("Missing COSMIC RAYS script: " + path);
        return false;
    }
}
