using UnityEngine;

/// <summary>Generates the small amount of ambient and interface audio used by the prologue.</summary>
public static class CosmicRaysAudio
{
    private const int SampleRate = 44100;

    private static AudioSource _clickSource;
    private static AudioClip _clickClip;

    public static void PlayClick()
    {
        if (_clickSource == null)
        {
            var sourceObject = new GameObject("COSMIC RAYS Interface Audio");
            Object.DontDestroyOnLoad(sourceObject);
            _clickSource = sourceObject.AddComponent<AudioSource>();
            _clickSource.playOnAwake = false;
            _clickSource.spatialBlend = 0f;
            _clickSource.volume = 0.22f;
        }

        if (_clickClip == null)
        {
            _clickClip = CreateClickClip();
        }

        _clickSource.PlayOneShot(_clickClip);
    }

    public static AudioClip CreateRoomTone(bool space)
    {
        int lengthSeconds = 8;
        int sampleCount = SampleRate * lengthSeconds;
        float[] samples = new float[sampleCount];
        float noise = 0f;

        for (int index = 0; index < sampleCount; index++)
        {
            float time = index / (float)SampleRate;
            float value;

            if (space)
            {
                value =
                    Mathf.Sin(time * Mathf.PI * 2f * 41f) * 0.18f +
                    Mathf.Sin(time * Mathf.PI * 2f * 82f) * 0.07f +
                    Mathf.Sin(time * Mathf.PI * 2f * 123f) * 0.025f;
            }
            else
            {
                value =
                    Mathf.Sin(time * Mathf.PI * 2f * 55f) * 0.16f +
                    Mathf.Sin(time * Mathf.PI * 2f * 110f) * 0.06f +
                    Mathf.Sin(time * Mathf.PI * 2f * 220f) * 0.02f;
            }

            noise = Mathf.Lerp(noise, Random.Range(-1f, 1f), 0.015f);
            value += noise * 0.055f;
            samples[index] = value * 0.55f;
        }

        AudioClip clip = AudioClip.Create(
            space ? "Deep Space Drone" : "Laboratory Room Tone",
            sampleCount,
            1,
            SampleRate,
            false);
        clip.SetData(samples, 0);
        return clip;
    }

    private static AudioClip CreateClickClip()
    {
        int sampleCount = Mathf.RoundToInt(SampleRate * 0.075f);
        float[] samples = new float[sampleCount];

        for (int index = 0; index < sampleCount; index++)
        {
            float time = index / (float)SampleRate;
            float fade = 1f - index / (float)sampleCount;
            fade *= fade;
            samples[index] =
                (Mathf.Sin(time * Mathf.PI * 2f * 1180f) * 0.42f +
                 Mathf.Sin(time * Mathf.PI * 2f * 1760f) * 0.16f) * fade;
        }

        AudioClip clip = AudioClip.Create("Interface Click", sampleCount, 1, SampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
