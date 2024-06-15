using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// A class containing information when and how a biome is generated
/// </summary>

[Serializable]
public class Biome
{
    // bounds for the inlandness, heat, humidity cube for this biome

    public string name;
    public int biomeMaskIndex;

    [Header("inlandness, heat, humidity cube")]
    public Vector3 bound0;
    public Vector3 bound1;

    public float linearPadding = 0.1f; // padding for the linear interpolation between biomes


    [Header("BIOME SETTINGS")]
    [Header("Inlandness")]
    public AnimationCurve inlandnessHeightCurve;
    [Header("Plainness")]
    public PerlinGenerator plainnessPerlinGenerator;
    [Header("Bumpiness")]
    public PerlinGenerator bumpinessPerlinGenerator;

    [Header("Color")]
    public Color color;


    [Header("Water")]

    public bool generateWater = true;
    public PerlinGenerator lakePerlinGenerator;
    public float waterThreshold = 0.5f;
    public float waterPlainnessSmoothness = 5f; // exponent for how quickly the water appears as plainness decreases
    public float waterDepthMultiplier = 20f;
    public AnimationCurve waterDepthProfile;

    public void InitializePerlinGenerators(Vector2 pos1, Vector2 pos2, Vector2 pos3) { 

        plainnessPerlinGenerator.SetOrigin(pos1);
        bumpinessPerlinGenerator.SetOrigin(pos2);
        lakePerlinGenerator.SetOrigin(pos3);

    }


    /// <summary>
    /// Evaluates the inlandness curve, returning a value between 0 and 1
    /// </summary>
    public float EvaluateInlandness(float inlandness) {

        // limit the inlandness to the bounds so the curve is smoothly evaluated
        inlandness -= bound0.x;
        inlandness /= (bound1.x - bound0.x);

        float inl = inlandnessHeightCurve.Evaluate(inlandness);
        return inl;
    }


    /// <summary>
    /// Returns the height at a given position
    /// </summary>
    public float GetHeight(Vector2 position, float inlandness, float inlandnessHeightMultiplier, bool normalized = false) {

        inlandness = EvaluateInlandness(inlandness);
        if (!normalized) inlandness *= inlandnessHeightMultiplier;

        // get how plain the terrain is
        float plainness = plainnessPerlinGenerator.SampleNosie(position, clamp: normalized, multiplyWithHeightMultiplier: !normalized);
        float bumpiness = bumpinessPerlinGenerator.SampleNosie(position, clamp: normalized, multiplyWithHeightMultiplier: !normalized);
        float waterValue = GetWaterValue(position);

        bumpiness *= plainness;
        bumpiness *= Mathf.Pow(1 - waterValue, waterPlainnessSmoothness);

        float waterDepth = waterValue * waterDepthMultiplier;

        float height = inlandness + bumpiness - waterDepth;


        return height;

    }

    /// <summary>
    /// Returns the water "amount" at a given position. Values in the range [0,1]
    /// </summary>
    public float GetWaterValue(Vector2 position) {

        if (!generateWater) return 0;

        float raw = lakePerlinGenerator.SampleNosie(position, clamp: true);
        raw = Mathf.Max(0,raw - waterThreshold);
        raw /= (1 - waterThreshold); // normalize

        raw = waterDepthProfile.Evaluate(raw);
        
        return raw;
    }

    // CAN BE OPTIMISED BY NOT CALCULATING THE FIRST HEIGHT, BUT INSTEAD PASSING IT AS AN ARGUMENT
    // COULD ALSO BE DONE ANALYTICALLY INSTEAD?

    /// <summary>
    /// Returns the gradient of the height at a given position
    /// </summary>
    public Vector2 GetHeightGradient(Vector2 position, float inlandness, float inlandnessHeightMultiplier, float h = 0.01f, bool raw = false) {

        float firstHeight = GetHeight(position, inlandness, inlandnessHeightMultiplier, normalized: raw);

        float xStep = GetHeight(position + new Vector2(h, 0), inlandness, inlandnessHeightMultiplier, normalized: raw);
        float yStep = GetHeight(position + new Vector2(0, h), inlandness, inlandnessHeightMultiplier, normalized: raw);

        float dx = (xStep - firstHeight) / h;
        float dy = (yStep - firstHeight) / h;

        return new Vector2(dx, dy);

    }
}

/// <summary>
/// A return type of info on how to interpolate between biomes
public class BiomeInterpolationInfo {
    public Biome biome; // which biome
    public float weight; // its weight in the interpolation
}
