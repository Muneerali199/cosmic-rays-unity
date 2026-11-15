using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using UnityEngine;

/// <summary>Reads and writes validated, versioned local mission progress.</summary>
public static class CosmicRaysSaveSystem
{
    /// <summary>Current persisted save schema version.</summary>
    public const int CurrentVersion = 1;
    private const string SaveFileName = "cosmic_rays_save.json";
    private const int MaximumSaveEntries = 10000;

    /// <summary>Absolute path of the primary local save file.</summary>
    public static string SavePath
    {
        get { return Path.Combine(Application.persistentDataPath, SaveFileName); }
    }

    /// <summary>Whether a primary or backup save file exists.</summary>
    public static bool HasAnySave
    {
        get { return File.Exists(SavePath) || File.Exists(SavePath + ".bak"); }
    }

    /// <summary>Writes validated mission state using a temporary file and backup replacement.</summary>
    public static bool TrySave(CosmicRaysSaveData data, out string error)
    {
        error = null;
        if (data == null || data.resources == null)
        {
            error = "There is no valid mission state to save.";
            return false;
        }

        if (data.version > CurrentVersion)
        {
            error = "The supplied mission state uses a newer save version and cannot be written.";
            return false;
        }

        data.version = CurrentVersion;
        data.savedAtUtc = DateTime.UtcNow.ToString("o");
        if (data.inventory == null) data.inventory = new System.Collections.Generic.List<InventoryStackData>();
        if (data.completedObjectiveIds == null) data.completedObjectiveIds = new System.Collections.Generic.List<string>();
        if (!ValidateData(data, out error))
        {
            return false;
        }

        string path = SavePath;
        string tempPath = path + ".tmp";
        string backupPath = path + ".bak";
        if (!CanWriteOverExistingSaves(path, backupPath, out error))
        {
            return false;
        }

        bool backupIsValid = File.Exists(backupPath) && IsValidSaveFile(backupPath);
        bool primaryIsValid = File.Exists(path) && IsValidSaveFile(path);

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(tempPath, JsonUtility.ToJson(data, true));

            if (File.Exists(path))
            {
                if (primaryIsValid)
                {
                    if (File.Exists(backupPath))
                    {
                        File.Delete(backupPath);
                    }
                    File.Move(path, backupPath);
                    backupIsValid = true;
                }
                else
                {
                    File.Delete(path);
                }
            }

            File.Move(tempPath, path);
            return true;
        }
        catch (IOException exception)
        {
            RestoreBackupIfPrimaryIsMissing(path, backupPath, backupIsValid, exception);
            TryDeleteTempFile(tempPath);
            error = "Could not write the mission save: " + exception.Message;
            return false;
        }
        catch (UnauthorizedAccessException exception)
        {
            RestoreBackupIfPrimaryIsMissing(path, backupPath, backupIsValid, exception);
            TryDeleteTempFile(tempPath);
            error = "Permission was denied while writing the mission save: " + exception.Message;
            return false;
        }
        catch (SecurityException exception)
        {
            RestoreBackupIfPrimaryIsMissing(path, backupPath, backupIsValid, exception);
            TryDeleteTempFile(tempPath);
            error = "The mission save location is not accessible: " + exception.Message;
            return false;
        }
        catch (ArgumentException exception)
        {
            RestoreBackupIfPrimaryIsMissing(path, backupPath, backupIsValid, exception);
            TryDeleteTempFile(tempPath);
            error = "The mission save path is invalid: " + exception.Message;
            return false;
        }
    }

    /// <summary>Loads the primary save or a valid backup, without accepting newer schema versions.</summary>
    public static bool TryLoad(out CosmicRaysSaveData data, out string message)
    {
        data = null;
        message = null;
        string path = SavePath;
        string backupPath = path + ".bak";
        string primaryError = null;

        if (File.Exists(path))
        {
            bool isNewerVersion;
            if (TryReadFile(path, out data, out primaryError, out isNewerVersion))
            {
                return true;
            }
            if (isNewerVersion)
            {
                message = primaryError;
                return false;
            }
        }

        if (File.Exists(backupPath))
        {
            string backupError;
            bool isNewerVersion;
            if (TryReadFile(backupPath, out data, out backupError, out isNewerVersion))
            {
                message = "The main save could not be loaded. The backup was loaded; the primary file was left unchanged.";
                Debug.LogWarning(message + (string.IsNullOrEmpty(primaryError) ? string.Empty : " " + primaryError));
                return true;
            }
            if (isNewerVersion)
            {
                message = backupError;
                return false;
            }
            message = "Neither the main save nor its backup could be loaded. Primary: " +
                      (string.IsNullOrEmpty(primaryError) ? "not available" : primaryError) +
                      " Backup: " + backupError;
            return false;
        }

        message = primaryError;
        return false;
    }

    private static bool CanWriteOverExistingSaves(string path, string backupPath, out string error)
    {
        error = null;
        string[] candidates = { path, backupPath };
        for (int i = 0; i < candidates.Length; i++)
        {
            if (!File.Exists(candidates[i]))
            {
                continue;
            }

            try
            {
                CosmicRaysSaveData existing =
                    JsonUtility.FromJson<CosmicRaysSaveData>(File.ReadAllText(candidates[i]));
                if (existing != null && existing.version > CurrentVersion)
                {
                    error = "A newer COSMIC RAYS save exists. It was not overwritten.";
                    return false;
                }
            }
            catch (IOException exception)
            {
                error = "Could not check the existing save before writing: " + exception.Message;
                return false;
            }
            catch (UnauthorizedAccessException exception)
            {
                error = "Permission was denied while checking the existing save: " + exception.Message;
                return false;
            }
            catch (SecurityException exception)
            {
                error = "The existing save could not be checked: " + exception.Message;
                return false;
            }
            catch (ArgumentException exception)
            {
                error = "The existing save could not be parsed before writing: " + exception.Message;
                return false;
            }
        }
        return true;
    }

    private static bool TryReadFile(
        string path,
        out CosmicRaysSaveData data,
        out string error,
        out bool isNewerVersion)
    {
        data = null;
        error = null;
        isNewerVersion = false;

        try
        {
            string json = File.ReadAllText(path);
            data = JsonUtility.FromJson<CosmicRaysSaveData>(json);
            if (data == null)
            {
                error = "The save file is incomplete or invalid.";
                return false;
            }

            if (data.version > CurrentVersion)
            {
                isNewerVersion = true;
                error = "This save was created by a newer version of COSMIC RAYS. It was not changed.";
                data = null;
                return false;
            }
            if (data.resources == null)
            {
                error = "The save file is missing required resource data.";
                data = null;
                return false;
            }
            if (data.version < 1)
            {
                error = "The save file has an unsupported version.";
                data = null;
                return false;
            }

            if (!ValidateData(data, out error))
            {
                data = null;
                return false;
            }
            return true;
        }
        catch (IOException exception)
        {
            error = "Could not read the save file: " + exception.Message;
        }
        catch (UnauthorizedAccessException exception)
        {
            error = "Permission was denied while reading the save file: " + exception.Message;
        }
        catch (SecurityException exception)
        {
            error = "The save file is not accessible: " + exception.Message;
        }
        catch (ArgumentException exception)
        {
            error = "The save file could not be parsed: " + exception.Message;
        }

        return false;
    }

    private static bool IsValidSaveFile(string path)
    {
        CosmicRaysSaveData ignoredData;
        string ignoredError;
        bool isNewerVersion;
        return TryReadFile(path, out ignoredData, out ignoredError, out isNewerVersion);
    }

    /// <summary>Checks save data before it is written or applied to runtime systems.</summary>
    public static bool ValidateData(CosmicRaysSaveData data, out string error)
    {
        error = null;
        if (data == null || data.resources == null)
        {
            error = "The save is missing its required mission resource data.";
            return false;
        }

        if (data.version < 1 || data.version > CurrentVersion)
        {
            error = "The save has an unsupported version: " + data.version + ".";
            return false;
        }

        if (!IsValidResource(data.resources.oxygen) ||
            !IsValidResource(data.resources.water) ||
            !IsValidResource(data.resources.energy))
        {
            error = "The save contains an invalid survival resource value.";
            return false;
        }

        if (data.inventory == null || data.completedObjectiveIds == null)
        {
            error = "The save is missing inventory or mission progress data.";
            return false;
        }
        if (data.inventory.Count > MaximumSaveEntries ||
            data.completedObjectiveIds.Count > MaximumSaveEntries)
        {
            error = "The save contains more mission entries than this build supports.";
            return false;
        }

        var savedItemIds = new HashSet<string>(StringComparer.Ordinal);
        for (int i = 0; i < data.inventory.Count; i++)
        {
            InventoryStackData stack = data.inventory[i];
            if (stack == null || string.IsNullOrWhiteSpace(stack.itemId) || stack.quantity <= 0 ||
                !savedItemIds.Add(stack.itemId))
            {
                error = "The save contains an invalid or duplicate inventory stack.";
                return false;
            }
        }

        var completedIds = new HashSet<string>(StringComparer.Ordinal);
        for (int i = 0; i < data.completedObjectiveIds.Count; i++)
        {
            string objectiveId = data.completedObjectiveIds[i];
            if (string.IsNullOrWhiteSpace(objectiveId) || !completedIds.Add(objectiveId))
            {
                error = "The save contains an invalid or duplicate completed objective ID.";
                return false;
            }
        }

        return true;
    }

    private static bool IsValidResource(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value) && value >= 0f;
    }

    private static void RestoreBackupIfPrimaryIsMissing(
        string path,
        string backupPath,
        bool backupIsValid,
        Exception originalError)
    {
        if (File.Exists(path) || !backupIsValid || !File.Exists(backupPath))
        {
            return;
        }

        try
        {
            File.Move(backupPath, path);
        }
        catch (IOException restoreError)
        {
            Debug.LogError("Could not restore the previous save after a write failure. " +
                           originalError.Message + " Restore error: " + restoreError.Message);
        }
        catch (UnauthorizedAccessException restoreError)
        {
            Debug.LogError("Could not restore the previous save after a write failure. " +
                           originalError.Message + " Restore error: " + restoreError.Message);
        }
        catch (SecurityException restoreError)
        {
            Debug.LogError("Could not restore the previous save after a write failure. " +
                           originalError.Message + " Restore error: " + restoreError.Message);
        }
    }

    private static void TryDeleteTempFile(string tempPath)
    {
        if (!File.Exists(tempPath))
        {
            return;
        }

        try
        {
            File.Delete(tempPath);
        }
        catch (IOException exception)
        {
            Debug.LogWarning("Could not remove the temporary save file: " + exception.Message);
        }
        catch (UnauthorizedAccessException exception)
        {
            Debug.LogWarning("Could not remove the temporary save file: " + exception.Message);
        }
        catch (SecurityException exception)
        {
            Debug.LogWarning("Could not remove the temporary save file: " + exception.Message);
        }
    }
}
