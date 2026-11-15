using UnityEngine;

/// <summary>Resource categories that an inventory item can restore.</summary>
public enum CosmicResource
{
    /// <summary>Indicates that an item does not restore a survival resource.</summary>
    None,
    /// <summary>Oxygen restored by an item.</summary>
    Oxygen,
    /// <summary>Water restored by an item.</summary>
    Water,
    /// <summary>Energy restored by an item.</summary>
    Energy
}

/// <summary>Designer-authored inventory identity, display text, stack limit, and recovery effect.</summary>
[CreateAssetMenu(fileName = "InventoryItem", menuName = "COSMIC RAYS/Inventory Item")]
public sealed class InventoryItemDefinition : ScriptableObject
{
    /// <summary>Stable unique ID stored in save files.</summary>
    public string itemId;
    /// <summary>Player-facing item name.</summary>
    public string displayName;
    /// <summary>Player-facing explanation shown in inventory.</summary>
    [TextArea] public string description;
    /// <summary>Resource restored by consuming this item.</summary>
    public CosmicResource restoresResource;
    /// <summary>Placeholder recovery amount; tune for the final gameplay balance.</summary>
    [Min(0f)] public float restoreAmount;
    /// <summary>Maximum quantity allowed in one inventory stack.</summary>
    [Min(1)] public int maximumStack = 99;

    private void OnValidate()
    {
        restoreAmount = float.IsNaN(restoreAmount) || float.IsInfinity(restoreAmount)
            ? 0f
            : Mathf.Max(0f, restoreAmount);
        maximumStack = Mathf.Max(1, maximumStack);
    }
}
