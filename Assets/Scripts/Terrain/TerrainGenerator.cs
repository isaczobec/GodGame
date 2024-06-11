using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    [SerializeField] private float multiplyRandomNumbersWith = 1000f;
    [SerializeField] private float addRandomNumbersWith = 1000f; // add this to the random numbers to avoid negative numbers, which give symmetric noise

    [Header("WORLD GENERATION SETTINGS")]

    [Header("Perlin generator settings")]
    [SerializeField] private PerlinGenerator inlandnessPerlinGenerator; // a perlin generator for the inlandness of the terrain, ie how elevated and mountainous it is
    // [SerializeField] private PerlinGenerator plainnessPerlinGenerator; // a perlin generator for the plainness of the terrain, ie how flat it is
    // [SerializeField] private PerlinGenerator bumpinessPerlinGenerator; // a perlin generator for the bumpiness of the terrain, ie how bumpy it is. Used in tandem with the plainness perlin generator, which is a multiplier with this one

    [SerializeField] private PerlinGenerator humidityPerlinGenerator; // a perlin generator for the humidity of the terrain. for instance used for biome generation
    [SerializeField] private PerlinGenerator heatPerlinGenerator; // a perlin generator for the heat of the terrain. for instance used for biome generation

    // all biomes that can be generated
    [SerializeField] private List<Biome> biomes = new List<Biome>();



    // -------------------------

    public void SetSeed(int seed) {
        this.seed = seed;
    }

    void Start()
    {
        InitializePerlinGenerators(seed);
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
            biome.InitializePerlinGenerators(GetPseudoRandomVector2(), GetPseudoRandomVector2());
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
        
        // set the height of the verticies
        for (int i = 0; i < squareMeshObject.squareMesh.vertices.Length; i++) {
            Vector3 vertex = squareMeshObject.squareMesh.vertices[i];
            float height = GetHeight(new Vector2(vertex.x, vertex.z));
            vertex.y = height;
            squareMeshObject.squareMesh.vertices[i] = vertex;
        }


        // set the textures
        if (setTextures) {
            SetTexture(squareMeshObject, inlandnessPerlinGenerator, squareMeshObject.inlandnessTexture);
            SetTexture(squareMeshObject, humidityPerlinGenerator, squareMeshObject.humidityTexture);
            SetTexture(squareMeshObject, heatPerlinGenerator, squareMeshObject.heatTexture);

            SetBiomeInterpolatedTexture(squareMeshObject, squareMeshObject.bumpinessTexture, BiomeTextureType.Bumpiness);
            SetBiomeInterpolatedTexture(squareMeshObject, squareMeshObject.plainnessTexture, BiomeTextureType.Plainness);
            SetBiomeInterpolatedTexture(squareMeshObject, squareMeshObject.colorTexture, BiomeTextureType.Color);
            SetBiomeInterpolatedTexture(squareMeshObject, null, BiomeTextureType.BiomeMask, squareMeshObject.biomeMaskTextures.textureSize, squareMeshObject.biomeMaskTextures.textureSize);
            
        }

    }

    private void SetTexture(SquareMeshObject squareMeshObject, PerlinGenerator perlinGenerator, Texture2D texture) {
    float sampleXFrom = squareMeshObject.squareMesh.vertices[0].x;
    float sampleZFrom = squareMeshObject.squareMesh.vertices[0].z;

    for (int y = 0; y < texture.height; y++) {
        for (int x = 0; x < texture.width; x++) {
            float sampleX = sampleXFrom + x / (float)(texture.width) * squareMeshObject.squareMesh.sizeX * squareMeshObject.squareMesh.quadSize;
            float sampleZ = sampleZFrom + y / (float)(texture.height) * squareMeshObject.squareMesh.sizeZ * squareMeshObject.squareMesh.quadSize;

            float height = perlinGenerator.SampleNosie(new Vector2(sampleX, sampleZ), clamp: true, runThroughCurve: false, multiplyWithHeightMultiplier: false);
            texture.SetPixel(x, y, new Color(height, height, height, 1f));
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
    }
    private void SetBiomeInterpolatedTexture(SquareMeshObject squareMeshObject, Texture2D texture, BiomeTextureType biomeTextureType, int textureHeight = 10, int textureWidth = 10) { // THE INTERPOLATEBIOMES IS BEING RAN MULTIPLE TIMES, FIX THIS

        // get the initial sample positions
        float sampleXFrom = squareMeshObject.squareMesh.vertices[0].x;
        float sampleZFrom = squareMeshObject.squareMesh.vertices[0].z;

        int tH = 0;
        int tW = 0;
        if (texture != null) {
            tH = texture.height;
            tW = texture.width;
        } else if (biomeTextureType == BiomeTextureType.BiomeMask) {
            tH = textureHeight;
            tW = textureWidth;
        } 

        

        for (int y = 0; y < tH; y++) {
            for (int x = 0; x < tW; x++) {
                float sampleX = sampleXFrom + x * squareMeshObject.squareMesh.quadSize * squareMeshObject.squareMesh.sizeX / tW;
                float sampleZ = sampleZFrom + y * squareMeshObject.squareMesh.quadSize * squareMeshObject.squareMesh.sizeZ / tH;

                float inlandness = inlandnessPerlinGenerator.SampleNosie(new Vector2(sampleX, sampleZ), clamp: true);
                float humidity = humidityPerlinGenerator.SampleNosie(new Vector2(sampleX, sampleZ), clamp: true);
                float heat = heatPerlinGenerator.SampleNosie(new Vector2(sampleX, sampleZ), clamp: true);
                
                List<BiomeInterpolationInfo> interpolateBiomes = GetBiomesOnPoint(inlandness, heat, humidity);

                switch (biomeTextureType) {

                    case BiomeTextureType.Inlandness:
                        foreach (BiomeInterpolationInfo interpolateBiome in interpolateBiomes) {
                            float val = interpolateBiome.biome.EvaluateInlandness(new Vector2(sampleX, sampleZ), inlandness);
                            texture.SetPixel(x, y, new Color(val, val, val, 1f));
                        }
                        break;

                    case BiomeTextureType.Plainness:
                        SetPixelValueBiomeInterpolationPerlin(x, y, sampleX, sampleZ, interpolateBiomes, texture, biomeTextureType);
                        break;

                    case BiomeTextureType.Bumpiness:
                        SetPixelValueBiomeInterpolationPerlin(x, y, sampleX, sampleZ, interpolateBiomes, texture, biomeTextureType);
                        break;

                    case BiomeTextureType.BiomeMask:

                        foreach (BiomeInterpolationInfo interpolateBiome in interpolateBiomes) {
                            squareMeshObject.biomeMaskTextures.SetBiomeMask(interpolateBiome.biome.biomeMaskIndex, x, y, interpolateBiome.weight);
                        }

                        break;

                    case BiomeTextureType.Color:
                        Color runningColorSum = new Color(0, 0, 0, 0);
                        foreach (BiomeInterpolationInfo bi in interpolateBiomes) {
                            runningColorSum += bi.biome.color * bi.weight;
                        } 
                        texture.SetPixel(x, y, runningColorSum);
                        break;

                }

            }
        }
        if (texture != null) { // if we arent rendering the biome mask
            texture.Apply();
        } else if (biomeTextureType == BiomeTextureType.BiomeMask) {
            squareMeshObject.biomeMaskTextures.ApplyAllBiomeMaskChanges();
        }

    }

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
            heightSum += interpolateBiome.biome.GetHeight(position, inlandnessHeight) * interpolateBiome.weight;
        }

        return heightSum;

    }
}

