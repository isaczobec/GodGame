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

    [Header("inlandness, heat, humidity cube")]
    public Vector3 bottomLeftCornerBound;
    public Vector3 topRightCorner;

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

    public float GetHeight(Vector2 position, float inlandness) {

        inlandness *= inlandnessHeightMultiplier;

        // get how plain the terrain is
        float plainness = plainnessPerlinGenerator.SampleNosie(position);
        float bumpiness = bumpinessPerlinGenerator.SampleNosie(position) * plainness;

        float height = inlandness + bumpiness;
        return height;

    }
}
