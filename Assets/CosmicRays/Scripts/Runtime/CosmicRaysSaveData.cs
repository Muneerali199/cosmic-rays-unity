using System;
using System.Collections.Generic;

/// <summary>Plain serializable snapshot of a mission, containing no Unity object references.</summary>
[Serializable]
public sealed class CosmicRaysSaveData
{
    /// <summary>Save schema version used to validate future compatibility.</summary>
    public int version = CosmicRaysSaveSystem.CurrentVersion;
    /// <summary>UTC timestamp of the most recent write.</summary>
    public string savedAtUtc;
    /// <summary>Player survival resource snapshot.</summary>
    public ResourceSnapshot resources = new ResourceSnapshot();
    /// <summary>Item ID and quantity stacks; contains no Unity object references.</summary>
    public List<InventoryStackData> inventory = new List<InventoryStackData>();
    /// <summary>Stable IDs of completed mission objectives.</summary>
    public List<string> completedObjectiveIds = new List<string>();
}

[Serializable]
public sealed class ResourceSnapshot
{
    /// <summary>Saved oxygen value.</summary>
    public float oxygen;
    /// <summary>Saved water value.</summary>
    public float water;
    /// <summary>Saved energy value.</summary>
    public float energy;
}

[Serializable]
public sealed class InventoryStackData
{
    /// <summary>Stable item-definition ID.</summary>
    public string itemId;
    /// <summary>Quantity in the stack.</summary>
    public int quantity;
}
