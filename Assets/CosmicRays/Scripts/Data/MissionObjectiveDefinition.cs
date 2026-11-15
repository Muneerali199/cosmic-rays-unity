using UnityEngine;

/// <summary>Designer-authored title and description for one stable mission objective ID.</summary>
[CreateAssetMenu(fileName = "MissionObjective", menuName = "COSMIC RAYS/Mission Objective")]
public sealed class MissionObjectiveDefinition : ScriptableObject
{
    /// <summary>Stable unique ID stored in save data and passed to CompleteObjective.</summary>
    public string objectiveId;
    /// <summary>Mission month used to order the objective list.</summary>
    [Min(1)] public int missionMonth = 1;
    /// <summary>Player-facing objective title.</summary>
    public string title;
    /// <summary>Player-facing objective details.</summary>
    [TextArea] public string description;
}
