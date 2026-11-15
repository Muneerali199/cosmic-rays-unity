using UnityEngine;

/// <summary>Rotates and optionally floats a decorative object.</summary>
public sealed class CosmicRaysSpin : MonoBehaviour
{
    public Vector3 axis = Vector3.up;
    public float degreesPerSecond = 8f;
    public float bobAmplitude;
    public float bobSpeed = 1f;

    private Vector3 _restPosition;
    private float _phase;

    private void Awake()
    {
        _restPosition = transform.localPosition;
        _phase = Random.value * Mathf.PI * 2f;
    }

    private void Update()
    {
        transform.Rotate(axis, degreesPerSecond * Time.deltaTime, Space.Self);

        if (bobAmplitude > 0.0001f)
        {
            Vector3 position = _restPosition;
            position.y += Mathf.Sin(Time.time * bobSpeed + _phase) * bobAmplitude;
            transform.localPosition = position;
        }
    }
}
