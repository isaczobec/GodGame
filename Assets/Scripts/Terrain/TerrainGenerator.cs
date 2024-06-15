using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

/// <summary>
/// A class containing information about a chunk that has been generated. Returned by GenerateTerrain()
/// </summary
public class ChunkGenerationInfo {
    public ChunkGenerationInfo(Vector2[] heights) {
        this.heights = heights;
    }
    public Vector2[] heights {get; private set;}

}

public class TerrainGenerator: MonoBehaviour
{



    private int seed = 0;
    [SerializeField] private float multiplyRandomNumbersWith = 10000f;
    [SerializeField] private float addRandomNumbersWith = 10000f; // add this to the random numbers to avoid negative numbers, which give symmetric noise

    [Header("WORLD GENERATION SETTINGS")]

    [Header("Perlin generator settings")]
    [SerializeField] private PerlinGenerator inlandnessPerlinGenerator; // a perlin generator for the inlandness of the terrain, ie how elevated and mountainous it is
    [SerializeField] private float inlandnessHeightMultiplier = 300f;

    [SerializeField] private PerlinGenerator humidityPerlinGenerator; // a perlin generator for the humidity of the terrain. for instance used for biome generation
    [SerializeField] private PerlinGenerator heatPerlinGenerator; // a perlin generator for the heat of the terrain. for instance used for biome generation

    // all biomes that can be generated

    [SerializeField] private BiomeSO[] biomeSOs;
    private List<Biome> biomes = new List<Biome>();



    // -------------------------

    public void SetSeed(int seed) {
        this.seed = seed;
    }

    void Start()
    {
        AddBiomesFromSO();

        InitializePerlinGenerators(seed);
    }

    private void AddBiomesFromSO()
    {
        foreach (BiomeSO biomeSO in biomeSOs)
        {
            biomes.Add(biomeSO.biome);
        }
    }

    private void InitializePerlinGenerators(int seed) {

        // set pseudo random seed and init the perlin generators
        Random.InitState(seed);
        inlandnessPerlinGenerator.SetOrigin(GetPseudoRandomVector2());
        humidityPerlinGenerator.SetOrigin(GetPseudoRandomVector2());
        heatPerlinGenerator.SetOrigin(GetPseudoRandomVector2());

        // plainnessPerlinGenerator.SetOrigin(GetPseudoRandomVector2());
        // bumpinessPerlinGenerator.SetOrigin(GetPseudoRandomVector2());

        foreach (Biome biome in biomes) {
            biome.InitializePerlinGenerators(GetPseudoRandomVector2(), GetPseudoRandomVector2(), GetPseudoRandomVector2());
        }

    }

    private Vector2 GetPseudoRandomVector2() {
        Vector2 rand = new Vector2(Random.Range(1f,2f), Random.Range(1f,2f)) * multiplyRandomNumbersWith; // random between 1 and 2 because sampling negative numbers gives symmetric noise
        rand += new Vector2(addRandomNumbersWith, addRandomNumbersWith);
        return rand;
    }



    public void GenerateChunk(
        SquareMeshObject squareMeshObject,
        bool setTextures = true
        ) {
        
        Profiler.BeginSample("WorldGeneration/GenerateTerrain/GenerateChunk/SetVertexHeights");
        // set the height of the verticies
        for (int i = 0; i < squareMeshObject.squareMesh.vertices.Length; i++) {
            Vector3 vertex = squareMeshObject.squareMesh.vertices[i];
            float height = GetHeight(new Vector2(vertex.x, vertex.z));
            vertex.y = height;
            squareMeshObject.squareMesh.vertices[i] = vertex;
        }
        Profiler.EndSample();


        Profiler.BeginSample("WorldGeneration/GenerateTerrain/GenerateChunk/SetTextures");
        // set the textures
        if (setTextures) {
            // SetTexture(squareMeshObject, inlandnessPerlinGenerator, squareMeshObject.inlandnessTexture);
            // SetTexture(squareMeshObject, humidityPerlinGenerator, squareMeshObject.humidityTexture);
            // SetTexture(squareMeshObject, heatPerlinGenerator, squareMeshObject.heatTexture);

            // SetBiomeInterpolatedTexture(squareMeshObject, squareMeshObject.plainnessTexture, BiomeTextureType.Plainness);
            // SetBiomeInterpolatedTexture(squareMeshObject, squareMeshObject.bumpinessTexture, BiomeTextureType.Bumpiness);
            // SetBiomeInterpolatedTexture(squareMeshObject, squareMeshObject.steepnessTexture, BiomeTextureType.Steepness);

            SetAllTextures(squareMeshObject, squareMeshObject.inlandnessHumidityHeatTexture, squareMeshObject.plainnessBumpinessSteepnessTexture, squareMeshObject.plainnessBumpinessSteepnessTexture.height, squareMeshObject.plainnessBumpinessSteepnessTexture.width);
            SetInlandnessHumidityHeatTexture(squareMeshObject, squareMeshObject.inlandnessHumidityHeatTexture);

            // SetBiomeInterpolatedTexture(squareMeshObject, squareMeshObject.colorTexture, BiomeTextureType.Color);
            // SetBiomeInterpolatedTexture(squareMeshObject, null, BiomeTextureType.BiomeMask, squareMeshObject.biomeMaskTextures.textureSize, squareMeshObject.biomeMaskTextures.textureSize);
            
        }
        Profiler.EndSample();

    }

    private void SetTexture(SquareMeshObject squareMeshObject, PerlinGenerator perlinGenerator, Texture2D texture) {    
    float sampleXFrom = squareMeshObject.squareMesh.vertices[0].x;
    float sampleZFrom = squareMeshObject.squareMesh.vertices[0].z;

    for (int y = 0; y < texture.height; y++) {
        for (int x = 0; x < texture.width; x++) {
            float sampleX = sampleXFrom + (x) / (float)(texture.width) * squareMeshObject.squareMesh.sizeX * squareMeshObject.squareMesh.quadSize;
            float sampleZ = sampleZFrom + (y) / (float)(texture.height) * squareMeshObject.squareMesh.sizeZ * squareMeshObject.squareMesh.quadSize;

            float height = perlinGenerator.SampleNosie(new Vector2(sampleX, sampleZ), clamp: true, runThroughCurve: false, multiplyWithHeightMultiplier: false);
            texture.SetPixel(x, y, new Color(height, height, height, 1f));
        }
    }
    texture.Apply();
    }   


    private void SetInlandnessHumidityHeatTexture(SquareMeshObject squareMeshObject, Texture2D texture) {    
    float sampleXFrom = squareMeshObject.squareMesh.vertices[0].x;
    float sampleZFrom = squareMeshObject.squareMesh.vertices[0].z;

    for (int y = 0; y < texture.height; y++) {
        for (int x = 0; x < texture.width; x++) {
            float sampleX = sampleXFrom + (x) / (float)(texture.width) * squareMeshObject.squareMesh.sizeX * squareMeshObject.squareMesh.quadSize;
            float sampleZ = sampleZFrom + (y) / (float)(texture.height) * squareMeshObject.squareMesh.sizeZ * squareMeshObject.squareMesh.quadSize;

            float inlandness = inlandnessPerlinGenerator.SampleNosie(new Vector2(sampleX, sampleZ), clamp: true, runThroughCurve: false, multiplyWithHeightMultiplier: false);
            float humidity = humidityPerlinGenerator.SampleNosie(new Vector2(sampleX, sampleZ), clamp: true, runThroughCurve: false, multiplyWithHeightMultiplier: false);
            float heat = heatPerlinGenerator.SampleNosie(new Vector2(sampleX, sampleZ), clamp: true, runThroughCurve: false, multiplyWithHeightMultiplier: false);
            texture.SetPixel(x, y, new Color(inlandness, humidity, heat, 0f));
        }
    }
    texture.Apply();
    }   

    public enum BiomeTextureType {
        Inlandness,
        Plainness,
        Bumpiness,
        Color,
        BiomeMask,
        Steepness,

        PlainnessBumpinessSteepness,
        InlandnessHeatHumidity,
    }

    /// <summary>
    /// Sets all textures for a squareMeshObject. Make sure all textures have the same size.
    /// </summary>
    private void SetAllTextures(SquareMeshObject squareMeshObject, Texture2D inlHumHeatTexture, Texture2D plainBumpSteepTexture, int textureHeight = 10, int textureWidth = 10) { // THE INTERPOLATEBIOMES IS BEING RAN MULTIPLE TIMES, FIX THIS

        // get the initial sample positions
        float sampleXFrom = squareMeshObject.squareMesh.vertices[0].x;
        float sampleZFrom = squareMeshObject.squareMesh.vertices[0].z;

        int tH = textureHeight;
        int tW = textureWidth;

        for (int y = 0; y < tH; y++) {
            for (int x = 0; x < tW; x++) {
                float sampleX = sampleXFrom + x * squareMeshObject.squareMesh.quadSize * squareMeshObject.squareMesh.sizeX / tW;
                float sampleZ = sampleZFrom + y * squareMeshObject.squareMesh.quadSize * squareMeshObject.squareMesh.sizeZ / tH;

                float inlandness = inlandnessPerlinGenerator.SampleNosie(new Vector2(sampleX, sampleZ), clamp: true);
                float humidity = humidityPerlinGenerator.SampleNosie(new Vector2(sampleX, sampleZ), clamp: true);
                float heat = heatPerlinGenerator.SampleNosie(new Vector2(sampleX, sampleZ), clamp: true);


                
                List<BiomeInterpolationInfo> interpolateBiomes = GetBiomesOnPoint(inlandness, heat, humidity);


                inlHumHeatTexture.SetPixel(x, y, new Color(inlandness, humidity, heat, 0f));

                float r = 0f;
                float g = 0f;
                float b = 0f;
                float a = 0f;
                foreach (BiomeInterpolationInfo interpolateBiome in interpolateBiomes) {

                            r += interpolateBiome.biome.plainnessPerlinGenerator.SampleNosie(new Vector2(sampleX, sampleZ), clamp: true, runThroughCurve: false, multiplyWithHeightMultiplier: false) * interpolateBiome.weight;
                            g += interpolateBiome.biome.bumpinessPerlinGenerator.SampleNosie(new Vector2(sampleX, sampleZ), clamp: true, runThroughCurve: false, multiplyWithHeightMultiplier: false) * interpolateBiome.weight;
                            b += interpolateBiome.biome.GetHeightGradient(new Vector2(sampleX, sampleZ), inlandness, inlandnessHeightMultiplier,raw:false).magnitude * interpolateBiome.weight; // feels a little bit stupid that this has to be interpolated between biomes, but kinda makes sense

                            squareMeshObject.biomeMaskTextures.SetBiomeMask(interpolateBiome.biome.biomeMaskIndex, x, y, interpolateBiome.weight);

                }
                plainBumpSteepTexture.SetPixel(x, y, new Color(r, g, b, a));
            }
        }

        plainBumpSteepTexture?.Apply();
        inlHumHeatTexture?.Apply();
        squareMeshObject.biomeMaskTextures.ApplyAllBiomeMaskChanges();
    }

    // this is now only used for biome mask textures




    private void SetPixelValueBiomeInterpolationPerlin(float px, float py, float sampleX, float sampleZ, List<BiomeInterpolationInfo> biL, Texture2D texture, BiomeTextureType biomeTextureType) {
        float runningSum = 0;
        foreach (BiomeInterpolationInfo bi in biL) {
            PerlinGenerator perlinGenerator = null;
            switch (biomeTextureType) {
                case BiomeTextureType.Plainness:
                    perlinGenerator = bi.biome.plainnessPerlinGenerator;
                    break;
                case BiomeTextureType.Bumpiness:
                    perlinGenerator = bi.biome.bumpinessPerlinGenerator;
                    break;
            }
            float val = perlinGenerator.SampleNosie(new Vector2(sampleX, sampleZ), clamp: true, runThroughCurve: false, multiplyWithHeightMultiplier: false);
            runningSum += val * bi.weight;
        }
        texture.SetPixel((int)px, (int)py, new Color(runningSum, runningSum, runningSum, 1f));
    }



    // Interpolates between the biomes on a given point
    private List<BiomeInterpolationInfo> GetBiomesOnPoint(float inlandness, float heat, float humidity, bool smoothInterpolation = true) {
    List<BiomeInterpolationInfo> interpolateBiomes = new List<BiomeInterpolationInfo>();

    Vector3 location = new Vector3(inlandness, heat, humidity);
    float totalWeight = 0f;

    foreach (Biome biome in biomes) {
        float padding = biome.linearPadding / 2;
        float weight = 1f;

        // Calculate the weight based on distance to biome center
        Vector3 biomeCenter = (biome.bound0 + biome.bound1) / 2;
        Vector3 biomeExtents = (biome.bound1 - biome.bound0) / 2;

        for (int i = 0; i < 3; i++) {
            float distanceToCenter = Mathf.Abs(location[i] - biomeCenter[i]);
            float biomeSizeWithPadding = biomeExtents[i] + padding;
            weight *= Mathf.Clamp01(1 - (distanceToCenter / biomeSizeWithPadding));
        }

        if (smoothInterpolation) weight = SmoothingFunction(weight);
        totalWeight += weight;
        interpolateBiomes.Add(new BiomeInterpolationInfo() { biome = biome, weight = weight });
    }

    // Normalize the weights if totalWeight > 0 to ensure they sum to 1
    if (!smoothInterpolation && totalWeight > 0) {
        for (int i = 0; i < interpolateBiomes.Count; i++) {
            interpolateBiomes[i].weight /= totalWeight;
        }
    }
    if (smoothInterpolation) {
        for (int i = 0; i < interpolateBiomes.Count; i++) {
            interpolateBiomes[i].weight /= totalWeight;
        }
    }

    return interpolateBiomes;
}


private float SmoothingFunction(float x) {
    return x * x * (3 - 2 * x);
 }





    // ----------- Actual generation code --------------

    /// <summary>
    /// Generates the height of the terrain at a given position
    /// </summary>
    private float GetHeight(Vector2 position) {

        float inlandnessHeight = inlandnessPerlinGenerator.SampleNosie(position, clamp: true);
        float humidity = humidityPerlinGenerator.SampleNosie(position, clamp: true);
        float heat = heatPerlinGenerator.SampleNosie(position, clamp: true);

        List<BiomeInterpolationInfo> interpolateBiomes = GetBiomesOnPoint(inlandnessHeight, heat, humidity);

        float heightSum = 0;
        foreach (BiomeInterpolationInfo interpolateBiome in interpolateBiomes) {
            heightSum += interpolateBiome.biome.GetHeight(position, inlandnessHeight, inlandnessHeightMultiplier) * interpolateBiome.weight;
        }

        return heightSum;

    }
}

