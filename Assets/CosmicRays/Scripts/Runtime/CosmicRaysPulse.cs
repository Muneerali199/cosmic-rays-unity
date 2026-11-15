using UnityEngine;

/// <summary>Pulses a light so terminals, beacons, and reactor cores feel alive.</summary>
public sealed class CosmicRaysPulse : MonoBehaviour
{
    public Light target;
    public float baseIntensity = 1f;
    public float amplitude = 0.25f;
    public float speed = 2f;
    public float phase;

    private void Awake()
    {
        if (target == null)
        {
            target = GetComponent<Light>();
        }
    }

    private void Update()
    {
        if (target == null)
        {
            return;
        }

        target.intensity = baseIntensity + Mathf.Sin(Time.time * speed + phase) * amplitude;
    }
}
