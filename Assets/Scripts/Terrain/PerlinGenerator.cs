



using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A class to generate perlin noise
/// </summary>

[Serializable]
public class PerlinGenerator
{

    public PerlinGenerator(Vector2 origin, float scale)
    {
        this.origin = origin;
        this.scale = scale;
    }

    // ---- SETTINGS ----

    private Vector2 origin;

    public float scale;
    public float valMultiplier;
    public AnimationCurve valCurve;
    public PerlinFractalSettings[] fractalSettings;
    public bool constantValue = false;


    // ---- METHODS ----

    public void SetOrigin(Vector2 origin) {
        this.origin = origin;
    }


    public float SampleNosie(Vector2 location, bool clamp = false, bool runThroughCurve = true, bool multiplyWithHeightMultiplier = true, float multiplyScale = 1)
    {
        float noise = GetNoiseValue(location, multiplyScale);

        if (clamp) { noise = Mathf.Clamp(noise, 0f, 1f); }
        if (runThroughCurve) { noise = valCurve.Evaluate(noise); }
        if (multiplyWithHeightMultiplier) { noise *= valMultiplier; }

        return noise;
    }

    /// <summary>
    /// Returns the RAW noise value at a location
    /// </summary>
    private float GetNoiseValue(Vector2 location, float multiplyScale)
    {

        if (constantValue) return valCurve.Evaluate(1f);

        Vector2 sampleFromLocation = (location - origin) * scale * multiplyScale;
        float noise = Mathf.PerlinNoise(sampleFromLocation.x, sampleFromLocation.y);

        
        if (fractalSettings.Length > 0)
        {
            float ampSum = 1f;
            foreach (PerlinFractalSettings fractalSetting in fractalSettings)
            {
                noise += Mathf.PerlinNoise(sampleFromLocation.x * fractalSetting.frequencyMultiplier, sampleFromLocation.y * fractalSetting.frequencyMultiplier) * fractalSetting.amplitudeMultiplier;
                ampSum += fractalSetting.amplitudeMultiplier;
            }
            noise /= ampSum;
        }

        return noise;
    }


}


[Serializable]
public class PerlinFractalSettings {
    public float amplitudeMultiplier;
    public float frequencyMultiplier;
}
