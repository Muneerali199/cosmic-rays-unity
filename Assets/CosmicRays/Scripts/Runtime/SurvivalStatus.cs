using System;
using UnityEngine;

/// <summary>Mutable runtime survival values and resource-change/depletion notifications.</summary>
public sealed class SurvivalStatus
{
    private readonly SurvivalConfig _config;
    private bool _oxygenDepletionReported;
    private bool _waterDepletionReported;
    private bool _energyDepletionReported;

    /// <summary>Raised after a resource value changes.</summary>
    public event Action Changed;
    /// <summary>Raised once when a resource reaches its configured minimum.</summary>
    public event Action<CosmicResource> Depleted;

    /// <summary>Current oxygen value.</summary>
    public float Oxygen { get; private set; }
    /// <summary>Current water value.</summary>
    public float Water { get; private set; }
    /// <summary>Current energy value.</summary>
    public float Energy { get; private set; }

    /// <summary>Creates runtime survival state from a config and optional saved snapshot.</summary>
    public SurvivalStatus(SurvivalConfig config, ResourceSnapshot savedValues)
    {
        if (config == null)
        {
            throw new ArgumentNullException(nameof(config));
        }

        string validationError;
        if (!config.Validate(out validationError))
        {
            throw new ArgumentException(validationError, nameof(config));
        }

        _config = config;
        Oxygen = ClampResource(
            savedValues != null ? savedValues.oxygen : config.startingOxygen,
            config.minimumOxygen,
            config.maximumOxygen);
        Water = ClampResource(
            savedValues != null ? savedValues.water : config.startingWater,
            config.minimumWater,
            config.maximumWater);
        Energy = ClampResource(
            savedValues != null ? savedValues.energy : config.startingEnergy,
            config.minimumEnergy,
            config.maximumEnergy);
        _oxygenDepletionReported = IsAtMinimum(CosmicResource.Oxygen, Oxygen);
        _waterDepletionReported = IsAtMinimum(CosmicResource.Water, Water);
        _energyDepletionReported = IsAtMinimum(CosmicResource.Energy, Energy);
    }

    /// <summary>True when any resource is at its configured minimum.</summary>
    public bool IsDepleted
    {
        get
        {
            return IsAtMinimum(CosmicResource.Oxygen, Oxygen) ||
                   IsAtMinimum(CosmicResource.Water, Water) ||
                   IsAtMinimum(CosmicResource.Energy, Energy);
        }
    }

    /// <summary>Gets the maximum value for a resource.</summary>
    public float MaximumFor(CosmicResource resource)
    {
        return _config.MaximumFor(resource);
    }

    /// <summary>Gets the minimum value for a resource.</summary>
    public float MinimumFor(CosmicResource resource)
    {
        return _config.MinimumFor(resource);
    }

    /// <summary>Gets the current value for a resource.</summary>
    public float CurrentFor(CosmicResource resource)
    {
        switch (resource)
        {
            case CosmicResource.Oxygen: return Oxygen;
            case CosmicResource.Water: return Water;
            case CosmicResource.Energy: return Energy;
            default: return 0f;
        }
    }

    /// <summary>Gets the low-resource threshold as a fraction of the configured range.</summary>
    public float WarningThreshold { get { return _config.warningThreshold; } }

    /// <summary>Consumes resources according to configured per-second rates.</summary>
    public void Tick(float elapsedSeconds)
    {
        if (elapsedSeconds <= 0f || float.IsNaN(elapsedSeconds) || float.IsInfinity(elapsedSeconds))
        {
            return;
        }

        float oldOxygen = Oxygen;
        float oldWater = Water;
        float oldEnergy = Energy;

        Oxygen = ClampResource(
            Oxygen - _config.oxygenUsePerSecond * elapsedSeconds,
            _config.minimumOxygen,
            _config.maximumOxygen);
        Water = ClampResource(
            Water - _config.waterUsePerSecond * elapsedSeconds,
            _config.minimumWater,
            _config.maximumWater);
        Energy = ClampResource(
            Energy - _config.energyUsePerSecond * elapsedSeconds,
            _config.minimumEnergy,
            _config.maximumEnergy);

        if (!Mathf.Approximately(oldOxygen, Oxygen) ||
            !Mathf.Approximately(oldWater, Water) ||
            !Mathf.Approximately(oldEnergy, Energy))
        {
            Changed?.Invoke();
        }

        ReportDepletion(CosmicResource.Oxygen, Oxygen, ref _oxygenDepletionReported);
        ReportDepletion(CosmicResource.Water, Water, ref _waterDepletionReported);
        ReportDepletion(CosmicResource.Energy, Energy, ref _energyDepletionReported);
    }

    /// <summary>Restores a resource up to its configured maximum and returns the actual amount restored.</summary>
    public float Restore(CosmicResource resource, float amount)
    {
        if (amount <= 0f || float.IsNaN(amount) || float.IsInfinity(amount))
        {
            return 0f;
        }

        float previous;
        switch (resource)
        {
            case CosmicResource.Oxygen:
                previous = Oxygen;
                Oxygen = Mathf.Min(_config.maximumOxygen, Oxygen + amount);
                if (!IsAtMinimum(resource, Oxygen)) _oxygenDepletionReported = false;
                break;
            case CosmicResource.Water:
                previous = Water;
                Water = Mathf.Min(_config.maximumWater, Water + amount);
                if (!IsAtMinimum(resource, Water)) _waterDepletionReported = false;
                break;
            case CosmicResource.Energy:
                previous = Energy;
                Energy = Mathf.Min(_config.maximumEnergy, Energy + amount);
                if (!IsAtMinimum(resource, Energy)) _energyDepletionReported = false;
                break;
            default:
                return 0f;
        }

        float restored = CurrentFor(resource) - previous;
        if (restored > 0f)
        {
            Changed?.Invoke();
        }
        return restored;
    }

    /// <summary>Creates plain save data for the current resource values.</summary>
    public ResourceSnapshot Capture()
    {
        return new ResourceSnapshot
        {
            oxygen = Oxygen,
            water = Water,
            energy = Energy
        };
    }

    private void ReportDepletion(CosmicResource resource, float value, ref bool alreadyReported)
    {
        if (IsAtMinimum(resource, value) && !alreadyReported)
        {
            alreadyReported = true;
            Depleted?.Invoke(resource);
        }
    }

    private bool IsAtMinimum(CosmicResource resource, float value)
    {
        return value <= MinimumFor(resource);
    }

    private static float ClampResource(float value, float minimum, float maximum)
    {
        if (float.IsNaN(value) || float.IsInfinity(value))
        {
            value = minimum;
        }
        return Mathf.Clamp(value, minimum, maximum);
    }
}
