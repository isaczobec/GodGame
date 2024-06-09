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

    private Vector2 origin;

    public float scale;
    public float inlandnessHeightMultiplier;
    public AnimationCurve inlandnessHeightCurve;

    public void SetOrigin(Vector2 origin) {
        this.origin = origin;
    }


    public float SampleNosie(Vector2 location, bool clamp = false, bool runThroughCurve = true, bool multiplyWithHeightMultiplier = true) {
        Vector2 sampleFromLocation = (location - origin) * scale;
        float noise = Mathf.PerlinNoise(sampleFromLocation.x, sampleFromLocation.y);

        if (clamp) {noise = Mathf.Clamp(noise, 0f, 1f);}
        if (runThroughCurve) {noise = inlandnessHeightCurve.Evaluate(noise);}
        if (multiplyWithHeightMultiplier) {noise *= inlandnessHeightMultiplier;}

        return noise;
    }
}
