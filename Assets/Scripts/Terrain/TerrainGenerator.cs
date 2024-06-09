using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    [SerializeField] private PerlinGenerator plainnessPerlinGenerator; // a perlin generator for the plainness of the terrain, ie how flat it is
    [SerializeField] private PerlinGenerator bumpinessPerlinGenerator; // a perlin generator for the bumpiness of the terrain, ie how bumpy it is. Used in tandem with the plainness perlin generator, which is a multiplier with this one

    [SerializeField] private PerlinGenerator humidityPerlinGenerator; // a perlin generator for the humidity of the terrain. for instance used for biome generation
    [SerializeField] private PerlinGenerator heatPerlinGenerator; // a perlin generator for the heat of the terrain. for instance used for biome generation



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
        plainnessPerlinGenerator.SetOrigin(GetPseudoRandomVector2());
        bumpinessPerlinGenerator.SetOrigin(GetPseudoRandomVector2());
        humidityPerlinGenerator.SetOrigin(GetPseudoRandomVector2());
        heatPerlinGenerator.SetOrigin(GetPseudoRandomVector2());

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
            SetTexture(squareMeshObject, plainnessPerlinGenerator, squareMeshObject.plainnessTexture);
            SetTexture(squareMeshObject, bumpinessPerlinGenerator, squareMeshObject.bumpinessTexture);
            SetTexture(squareMeshObject, humidityPerlinGenerator, squareMeshObject.humidityTexture);
            SetTexture(squareMeshObject, heatPerlinGenerator, squareMeshObject.heatTexture);
        }

    }

    private void SetTexture(SquareMeshObject squareMeshObject, PerlinGenerator perlinGenerator, Texture2D texture) {

        // get the initial sample positions
        float sampleXFrom = squareMeshObject.squareMesh.vertices[0].x;
        float sampleZFrom = squareMeshObject.squareMesh.vertices[0].z;


        for (int y = 0; y < texture.height; y++) {
            for (int x = 0; x < texture.width; x++) {
                float sampleX = sampleXFrom + x * squareMeshObject.squareMesh.quadSize * squareMeshObject.squareMesh.sizeX / texture.width;
                float sampleZ = sampleZFrom + y * squareMeshObject.squareMesh.quadSize * squareMeshObject.squareMesh.sizeZ / texture.height;

                float height = perlinGenerator.SampleNosie(new Vector2(sampleX, sampleZ), clamp:true, runThroughCurve:false, multiplyWithHeightMultiplier:false);
                texture.SetPixel(x, y, new Color(height, height, height, 1f));
            }
        }
        texture.Apply();

    }



    // ----------- Actual generation code --------------

    /// <summary>
    /// Generates the height of the terrain at a given position
    /// </summary>
    private float GetHeight(Vector2 position) {

        float inlandnessHeight = inlandnessPerlinGenerator.SampleNosie(position);

        // get how plain the terrain is
        float plainness = plainnessPerlinGenerator.SampleNosie(position);
        float bumpiness = bumpinessPerlinGenerator.SampleNosie(position) * plainness;

        float height = inlandnessHeight + bumpiness;
        return height;

    }
}

