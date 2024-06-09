using System;
using System.Collections;
using System.Collections.Generic;
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
    public float inlandnessHeightMultiplier = 100f;
    public AnimationCurve inlandnessHeightCurve;
    [Header("Plainness")]
    public PerlinGenerator plainnessPerlinGenerator;
    [Header("Bumpiness")]
    public PerlinGenerator bumpinessPerlinGenerator;

    [Header("Color")]
    public Color color;

    public void InitializePerlinGenerators(Vector2 pos1, Vector2 pos2) { 

        plainnessPerlinGenerator.SetOrigin(pos1);
        bumpinessPerlinGenerator.SetOrigin(pos2);

    }

    public float EvaluateInlandness(Vector2 position, float inlandness) {
        float inl = inlandnessHeightCurve.Evaluate(inlandness);
        inl *= inlandnessHeightMultiplier;
        return inl;
    }

    public float GetHeight(Vector2 position, float inlandness) {

        inlandness = EvaluateInlandness(position, inlandness);

        // get how plain the terrain is
        float plainness = plainnessPerlinGenerator.SampleNosie(position);
        float bumpiness = bumpinessPerlinGenerator.SampleNosie(position) * plainness;

        float height = inlandness + bumpiness;
        return height;

    }
}

/// <summary>
/// A return type of info on how to interpolate between biomes
public class BiomeInterpolationInfo {
    public Biome biome; // which biome
    public float weight; // its weight in the interpolation
}
