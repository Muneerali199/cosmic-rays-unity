using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Tracks completion state for the configured mission objective definitions.</summary>
public sealed class MissionProgress
{
    private readonly List<MissionObjectiveDefinition> _objectives = new List<MissionObjectiveDefinition>();
    private readonly HashSet<string> _completedIds = new HashSet<string>(StringComparer.Ordinal);
    private readonly HashSet<string> _unresolvedCompletedIds = new HashSet<string>(StringComparer.Ordinal);

    /// <summary>Raised after an objective transitions from pending to complete.</summary>
    public event Action Changed;

    /// <summary>Creates objective state from definitions and saved completed objective IDs.</summary>
    public MissionProgress(
        IEnumerable<MissionObjectiveDefinition> objectives,
        IEnumerable<string> completedIds)
    {
        if (objectives != null)
        {
            foreach (MissionObjectiveDefinition objective in objectives)
            {
                if (objective != null && !string.IsNullOrWhiteSpace(objective.objectiveId))
                {
                    if (HasObjective(objective.objectiveId))
                    {
                        Debug.LogError("Duplicate COSMIC RAYS mission objective ID: " + objective.objectiveId);
                    }
                    else
                    {
                        _objectives.Add(objective);
                    }
                }
            }
        }

        _objectives.Sort((left, right) =>
        {
            int monthCompare = left.missionMonth.CompareTo(right.missionMonth);
            return monthCompare != 0
                ? monthCompare
                : string.Compare(left.objectiveId, right.objectiveId, StringComparison.Ordinal);
        });

        if (completedIds == null)
        {
            return;
        }

        foreach (string id in completedIds)
        {
            if (HasObjective(id))
            {
                _completedIds.Add(id);
            }
            else if (!string.IsNullOrWhiteSpace(id))
            {
                Debug.LogWarning(
                    "Saved completed objective has no loaded definition and will be preserved: " + id);
                _unresolvedCompletedIds.Add(id);
            }
        }
    }

    /// <summary>Returns the configured objectives sorted by mission month and ID.</summary>
    public IReadOnlyList<MissionObjectiveDefinition> Objectives { get { return _objectives; } }
    /// <summary>Returns the number of completed configured objectives.</summary>
    public int CompletedCount { get { return _completedIds.Count; } }
    /// <summary>Returns whether at least one objective is configured.</summary>
    public bool HasObjectives { get { return _objectives.Count > 0; } }
    /// <summary>Returns whether every configured objective is complete.</summary>
    public bool IsComplete { get { return HasObjectives && _completedIds.Count == _objectives.Count; } }

    /// <summary>Checks completion state for a configured objective ID.</summary>
    public bool IsObjectiveComplete(string objectiveId)
    {
        return !string.IsNullOrWhiteSpace(objectiveId) && _completedIds.Contains(objectiveId);
    }

    /// <summary>Marks an objective complete once; returns false for unknown or completed IDs.</summary>
    public bool Complete(string objectiveId)
    {
        if (!HasObjective(objectiveId) || !_completedIds.Add(objectiveId))
        {
            return false;
        }

        Changed?.Invoke();
        return true;
    }

    /// <summary>Returns completed objective IDs as sorted plain save data.</summary>
    public List<string> CaptureCompletedIds()
    {
        var result = new List<string>(_completedIds);
        foreach (string id in _unresolvedCompletedIds)
        {
            if (!_completedIds.Contains(id))
            {
                result.Add(id);
            }
        }
        result.Sort(StringComparer.Ordinal);
        return result;
    }

    private bool HasObjective(string objectiveId)
    {
        if (string.IsNullOrWhiteSpace(objectiveId))
        {
            return false;
        }

        for (int i = 0; i < _objectives.Count; i++)
        {
            if (string.Equals(_objectives[i].objectiveId, objectiveId, StringComparison.Ordinal))
            {
                return true;
            }
        }
        return false;
    }
}
