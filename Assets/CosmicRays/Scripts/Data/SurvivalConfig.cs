using System;
using UnityEngine;

/// <summary>Designer-authored limits and placeholder tuning for the three survival resources.</summary>
[CreateAssetMenu(fileName = "SurvivalConfig", menuName = "COSMIC RAYS/Survival Config")]
public sealed class SurvivalConfig : ScriptableObject
{
    /// <summary>Lowest oxygen value before oxygen is considered depleted.</summary>
    [Tooltip("Gameplay placeholder default. Configure to match the final mission design.")]
    [Min(0f)] public float minimumOxygen;
    /// <summary>Lowest water value before water is considered depleted.</summary>
    [Tooltip("Gameplay placeholder default. Configure to match the final mission design.")]
    [Min(0f)] public float minimumWater;
    /// <summary>Lowest energy value before energy is considered depleted.</summary>
    [Tooltip("Gameplay placeholder default. Configure to match the final mission design.")]
    [Min(0f)] public float minimumEnergy;
    /// <summary>Highest oxygen value.</summary>
    [Tooltip("Gameplay placeholder default. Configure to match the final mission design.")]
    [Min(1f)] public float maximumOxygen = 100f;
    /// <summary>Highest water value.</summary>
    [Tooltip("Gameplay placeholder default. Configure to match the final mission design.")]
    [Min(1f)] public float maximumWater = 100f;
    /// <summary>Highest energy value.</summary>
    [Tooltip("Gameplay placeholder default. Configure to match the final mission design.")]
    [Min(1f)] public float maximumEnergy = 100f;
    /// <summary>Starting oxygen for a new mission.</summary>
    [Tooltip("Gameplay placeholder default. Configure to match the final mission design.")]
    [Min(0f)] public float startingOxygen = 100f;
    /// <summary>Starting water for a new mission.</summary>
    [Tooltip("Gameplay placeholder default. Configure to match the final mission design.")]
    [Min(0f)] public float startingWater = 100f;
    /// <summary>Starting energy for a new mission.</summary>
    [Tooltip("Gameplay placeholder default. Configure to match the final mission design.")]
    [Min(0f)] public float startingEnergy = 100f;
    /// <summary>Oxygen consumption per unpaused gameplay second.</summary>
    [Tooltip("Gameplay placeholder default. Configure to match the final mission design.")]
    [Min(0f)] public float oxygenUsePerSecond = 0.02f;
    /// <summary>Water consumption per unpaused gameplay second.</summary>
    [Tooltip("Gameplay placeholder default. Configure to match the final mission design.")]
    [Min(0f)] public float waterUsePerSecond = 0.01f;
    /// <summary>Energy consumption per unpaused gameplay second.</summary>
    [Tooltip("Gameplay placeholder default. Configure to match the final mission design.")]
    [Min(0f)] public float energyUsePerSecond = 0.015f;
    /// <summary>Low-resource warning threshold as a fraction of the configured resource range.</summary>
    [Range(0f, 1f)] public float warningThreshold = 0.25f;
    /// <summary>Whether reaching any resource minimum ends the mission.</summary>
    public bool failMissionWhenResourceIsEmpty = true;
    /// <summary>Stable item IDs and quantities granted when a new mission starts.</summary>
    public StartingInventoryItem[] startingInventory = Array.Empty<StartingInventoryItem>();

    private void OnValidate()
    {
        minimumOxygen = NormalizeNonNegative(minimumOxygen);
        minimumWater = NormalizeNonNegative(minimumWater);
        minimumEnergy = NormalizeNonNegative(minimumEnergy);
        maximumOxygen = NormalizeAtLeast(maximumOxygen, 1f, 100f);
        maximumWater = NormalizeAtLeast(maximumWater, 1f, 100f);
        maximumEnergy = NormalizeAtLeast(maximumEnergy, 1f, 100f);
        minimumOxygen = Mathf.Max(0f, Mathf.Min(minimumOxygen, maximumOxygen - 1f));
        minimumWater = Mathf.Max(0f, Mathf.Min(minimumWater, maximumWater - 1f));
        minimumEnergy = Mathf.Max(0f, Mathf.Min(minimumEnergy, maximumEnergy - 1f));
        startingOxygen = Mathf.Clamp(NormalizeNonNegative(startingOxygen), minimumOxygen, maximumOxygen);
        startingWater = Mathf.Clamp(NormalizeNonNegative(startingWater), minimumWater, maximumWater);
        startingEnergy = Mathf.Clamp(NormalizeNonNegative(startingEnergy), minimumEnergy, maximumEnergy);
        oxygenUsePerSecond = NormalizeNonNegative(oxygenUsePerSecond);
        waterUsePerSecond = NormalizeNonNegative(waterUsePerSecond);
        energyUsePerSecond = NormalizeNonNegative(energyUsePerSecond);
        warningThreshold = float.IsNaN(warningThreshold) || float.IsInfinity(warningThreshold)
            ? 0.25f
            : Mathf.Clamp01(warningThreshold);

        if (startingInventory == null)
        {
            startingInventory = Array.Empty<StartingInventoryItem>();
        }

        for (int i = 0; i < startingInventory.Length; i++)
        {
            if (startingInventory[i] != null)
            {
                startingInventory[i].quantity = Mathf.Max(0, startingInventory[i].quantity);
            }
        }
    }

    /// <summary>Returns the configured minimum for a resource, or zero for an unsupported value.</summary>
    public float MinimumFor(CosmicResource resource)
    {
        switch (resource)
        {
            case CosmicResource.Oxygen: return minimumOxygen;
            case CosmicResource.Water: return minimumWater;
            case CosmicResource.Energy: return minimumEnergy;
            default: return 0f;
        }
    }

    /// <summary>Returns the configured maximum for a resource, or zero for an unsupported value.</summary>
    public float MaximumFor(CosmicResource resource)
    {
        switch (resource)
        {
            case CosmicResource.Oxygen: return maximumOxygen;
            case CosmicResource.Water: return maximumWater;
            case CosmicResource.Energy: return maximumEnergy;
            default: return 0f;
        }
    }

    /// <summary>Validates values at runtime and reports invalid configuration before mission startup.</summary>
    public bool Validate(out string error)
    {
        error = null;
        if (!IsFinite(minimumOxygen) || !IsFinite(minimumWater) || !IsFinite(minimumEnergy) ||
            !IsFinite(maximumOxygen) || !IsFinite(maximumWater) || !IsFinite(maximumEnergy) ||
            !IsFinite(startingOxygen) || !IsFinite(startingWater) || !IsFinite(startingEnergy) ||
            !IsFinite(oxygenUsePerSecond) || !IsFinite(waterUsePerSecond) ||
            !IsFinite(energyUsePerSecond) || !IsFinite(warningThreshold))
        {
            error = "Survival configuration contains a non-finite numeric value.";
            return false;
        }

        if (minimumOxygen < 0f || minimumWater < 0f || minimumEnergy < 0f ||
            maximumOxygen <= minimumOxygen || maximumWater <= minimumWater ||
            maximumEnergy <= minimumEnergy ||
            startingOxygen < minimumOxygen || startingOxygen > maximumOxygen ||
            startingWater < minimumWater || startingWater > maximumWater ||
            startingEnergy < minimumEnergy || startingEnergy > maximumEnergy ||
            oxygenUsePerSecond < 0f || waterUsePerSecond < 0f || energyUsePerSecond < 0f ||
            warningThreshold < 0f || warningThreshold > 1f)
        {
            error = "Survival configuration values are outside their supported ranges.";
            return false;
        }

        if (startingInventory == null)
        {
            error = "Survival configuration has no valid starting-inventory list.";
            return false;
        }

        for (int i = 0; i < startingInventory.Length; i++)
        {
            StartingInventoryItem item = startingInventory[i];
            if (item != null &&
                (item.quantity < 0 || (item.quantity > 0 && string.IsNullOrWhiteSpace(item.itemId))))
            {
                error = "A starting-inventory entry has an invalid quantity or no stable item ID.";
                return false;
            }
        }

        return true;
    }

    private static float NormalizeNonNegative(float value)
    {
        return float.IsNaN(value) || float.IsInfinity(value) ? 0f : Mathf.Max(0f, value);
    }

    private static float NormalizeAtLeast(float value, float minimum, float fallback)
    {
        return float.IsNaN(value) || float.IsInfinity(value) ? fallback : Mathf.Max(minimum, value);
    }

    private static bool IsFinite(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value);
    }
}

/// <summary>One initial inventory stack granted when a new mission is created.</summary>
[Serializable]
public sealed class StartingInventoryItem
{
    /// <summary>Stable item-definition ID used to create the starting stack.</summary>
    public string itemId;
    /// <summary>Number of copies granted when a new mission starts.</summary>
    [Min(0)] public int quantity;
}
