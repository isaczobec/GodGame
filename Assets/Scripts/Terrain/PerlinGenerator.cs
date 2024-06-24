using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;

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
    public float valMultiplier;
    public AnimationCurve valCurve;
    public PerlinFractalSettings[] fractalSettings;
    public bool constantValue = false;


    private AnimationCurveInterpolator curveInterpolator;


    /// <summary>
    /// Set the origin of the noise generator. Used to call initialization function(s)
    /// </summary>
    /// <param name="origin"></param>
    public void SetOrigin(Vector2 origin)
    {
        this.origin = origin;

        Initialize();
    }


    private void Initialize()
    {
        
        curveInterpolator = new AnimationCurveInterpolator(valCurve, 20);
    }


    /// <summary>
    /// Sample noise at a location
    /// </summary>
    public float SampleNosie(Vector2 location, bool clamp = false, bool runThroughCurve = true, bool multiplyWithHeightMultiplier = true, float multiplyScale = 1)
    {
        float noise = GetNoiseValue(location, multiplyScale);

        if (clamp) noise = Mathf.Clamp(noise, 0f, 1f);
        if (runThroughCurve) noise = curveInterpolator.Sample(noise);
        if (multiplyWithHeightMultiplier) noise *= valMultiplier;

        return noise;
    }

    /// <summary>
    /// Get raw noise value from the perlin noise function
    /// </summary>
    /// <param name="location"></param>
    /// <param name="multiplyScale"></param>
    /// <returns></returns>
    private float GetNoiseValue(Vector2 location, float multiplyScale)
    {
        if (constantValue) return curveInterpolator.Sample(1f);

        Vector2 sampleFromLocation = (location - origin) * scale * multiplyScale;
        float noise = Mathf.PerlinNoise(sampleFromLocation.x, sampleFromLocation.y);

        // add fractal noise
        int fractalLength = fractalSettings.Length;
        if (fractalLength > 0)
        {
            float ampSum = 1f;
            for (int i = 0; i < fractalLength; i++)
            {
                PerlinFractalSettings fractalSetting = fractalSettings[i];
                noise += Mathf.PerlinNoise(sampleFromLocation.x * fractalSetting.frequencyMultiplier, sampleFromLocation.y * fractalSetting.frequencyMultiplier) * fractalSetting.amplitudeMultiplier;
                ampSum += fractalSetting.amplitudeMultiplier;
            }
            noise /= ampSum;
        }

        return noise;
    }


}

[Serializable]
public class PerlinFractalSettings
{
    public float amplitudeMultiplier;
    public float frequencyMultiplier;
}




    public class AnimationCurveInterpolator {

        public float[] xValues;
        public float[] yValues;

        public int steps;

        private float min;
        private float max;

        float diff;

        public AnimationCurveInterpolator(AnimationCurve curve, int steps) {
            this.steps = steps;

            xValues = new float[steps];
            yValues = new float[steps];

            min = curve.keys[0].time;
            max = curve.keys[curve.length - 1].time;

            diff = max - min;

            for (int i = 0; i < steps; i++) {
                float t = min + (float)i / (steps - 1) * diff;
                xValues[i] = t;
                yValues[i] = curve.Evaluate(t);
            }
        }

        public float Sample(float x) {
            if (x <= xValues[0]) return yValues[0];
            if (x >= xValues[steps - 1]) return yValues[steps - 1];

            float clamp01 = (x - min) / diff;
            int index = Mathf.FloorToInt(clamp01 * (steps - 1));

            // linearly interpolate between the two values
            float t = (clamp01 - (float)index / (steps - 1)) * (steps - 1);
            return Mathf.Lerp(yValues[index], yValues[index + 1], t);

        }

    }