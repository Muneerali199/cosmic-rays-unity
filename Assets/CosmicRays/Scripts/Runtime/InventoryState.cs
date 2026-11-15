using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Mutable item stacks that apply recovery effects through a SurvivalStatus instance.</summary>
public sealed class InventoryState
{
    private readonly Dictionary<string, InventoryItemDefinition> _definitions;
    private readonly Dictionary<string, int> _quantities = new Dictionary<string, int>(StringComparer.Ordinal);

    /// <summary>Raised after an item stack changes.</summary>
    public event Action Changed;

    /// <summary>Creates inventory from available item definitions and saved stack data.</summary>
    public InventoryState(
        IEnumerable<InventoryItemDefinition> definitions,
        IEnumerable<InventoryStackData> savedInventory)
    {
        _definitions = new Dictionary<string, InventoryItemDefinition>(StringComparer.Ordinal);
        if (definitions != null)
        {
            foreach (InventoryItemDefinition definition in definitions)
            {
                if (definition == null || string.IsNullOrWhiteSpace(definition.itemId))
                {
                    continue;
                }

                if (!_definitions.ContainsKey(definition.itemId))
                {
                    _definitions.Add(definition.itemId, definition);
                }
                else
                {
                    Debug.LogError("Duplicate COSMIC RAYS inventory item ID: " + definition.itemId);
                }
            }
        }

        if (savedInventory == null)
        {
            return;
        }

        foreach (InventoryStackData stack in savedInventory)
        {
            if (stack == null || string.IsNullOrWhiteSpace(stack.itemId) || stack.quantity <= 0)
            {
                continue;
            }

            if (!_definitions.ContainsKey(stack.itemId))
            {
                Debug.LogWarning(
                    "Saved inventory item has no loaded definition and will be preserved but unavailable: " +
                    stack.itemId);
                long unresolvedTotal = (long)GetCount(stack.itemId) + stack.quantity;
                _quantities[stack.itemId] = (int)Math.Min(int.MaxValue, unresolvedTotal);
                continue;
            }

            int current = GetCount(stack.itemId);
            int maximum = Mathf.Max(1, _definitions[stack.itemId].maximumStack);
            int restoredQuantity = (int)Math.Min(int.MaxValue, (long)current + stack.quantity);
            if (restoredQuantity > maximum)
            {
                Debug.LogWarning(
                    "Saved inventory quantity exceeds the current stack limit and was preserved: " +
                    stack.itemId);
            }
            if (restoredQuantity > current)
            {
                _quantities[stack.itemId] = restoredQuantity;
            }
        }
    }

    /// <summary>Returns the current quantity for an item ID, or zero when absent.</summary>
    public int GetCount(string itemId)
    {
        int quantity;
        return !string.IsNullOrEmpty(itemId) && _quantities.TryGetValue(itemId, out quantity) ? quantity : 0;
    }

    /// <summary>Adds as many copies as fit and returns the number accepted.</summary>
    public int Add(InventoryItemDefinition definition, int quantity)
    {
        if (definition == null || string.IsNullOrWhiteSpace(definition.itemId) || quantity <= 0)
        {
            return 0;
        }

        InventoryItemDefinition registeredDefinition;
        if (_definitions.TryGetValue(definition.itemId, out registeredDefinition))
        {
            if (registeredDefinition != definition)
            {
                Debug.LogError(
                    "Conflicting COSMIC RAYS item definitions use the same ID: " + definition.itemId);
                return 0;
            }
            definition = registeredDefinition;
        }
        else
        {
            _definitions.Add(definition.itemId, definition);
        }

        int current = GetCount(definition.itemId);
        int maximum = Mathf.Max(1, definition.maximumStack);
        int added = Mathf.Min(quantity, Mathf.Max(0, maximum - current));
        if (added <= 0)
        {
            return 0;
        }

        _quantities[definition.itemId] = current + added;
        Changed?.Invoke();
        return added;
    }

    /// <summary>Uses one item and restores its configured resource when recovery is possible.</summary>
    public bool TryUse(string itemId, SurvivalStatus survival, out string message)
    {
        message = "That item cannot be used.";
        if (survival == null || string.IsNullOrWhiteSpace(itemId))
        {
            return false;
        }

        InventoryItemDefinition definition;
        if (!_definitions.TryGetValue(itemId, out definition) || GetCount(itemId) <= 0)
        {
            message = "That item is not in the inventory.";
            return false;
        }

        if (definition.restoresResource == CosmicResource.None || definition.restoreAmount <= 0f)
        {
            return false;
        }

        float restored = survival.Restore(definition.restoresResource, definition.restoreAmount);
        if (restored <= 0f)
        {
            message = "The matching resource cannot accept any more.";
            return false;
        }

        int remaining = GetCount(itemId) - 1;
        if (remaining == 0)
        {
            _quantities.Remove(itemId);
        }
        else
        {
            _quantities[itemId] = remaining;
        }

        Changed?.Invoke();
        message = definition.displayName + " used.";
        return true;
    }

    /// <summary>Returns inventory stacks as serializable save data.</summary>
    public List<InventoryStackData> Capture()
    {
        var result = new List<InventoryStackData>();
        foreach (KeyValuePair<string, int> pair in _quantities)
        {
            result.Add(new InventoryStackData { itemId = pair.Key, quantity = pair.Value });
        }
        result.Sort((left, right) => string.CompareOrdinal(left.itemId, right.itemId));
        return result;
    }

    /// <summary>Returns saved stacks whose definitions are not currently loaded.</summary>
    public List<InventoryStackData> GetUnresolvedStacks()
    {
        var result = new List<InventoryStackData>();
        foreach (KeyValuePair<string, int> pair in _quantities)
        {
            if (!_definitions.ContainsKey(pair.Key))
            {
                result.Add(new InventoryStackData { itemId = pair.Key, quantity = pair.Value });
            }
        }
        result.Sort((left, right) => string.CompareOrdinal(left.itemId, right.itemId));
        return result;
    }

    /// <summary>Returns item definitions that currently have a positive stack, sorted by name.</summary>
    public List<InventoryItemDefinition> GetDefinitionsWithItems()
    {
        var result = new List<InventoryItemDefinition>();
        foreach (InventoryItemDefinition definition in _definitions.Values)
        {
            if (GetCount(definition.itemId) > 0)
            {
                result.Add(definition);
            }
        }
        result.Sort((left, right) => string.Compare(left.displayName, right.displayName, StringComparison.Ordinal));
        return result;
    }
}
