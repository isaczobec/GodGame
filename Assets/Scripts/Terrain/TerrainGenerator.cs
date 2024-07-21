using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

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



    private int seed = 10;
    /// <summary>
    /// Random instance used for ie seeding the perlin generators
    /// </summary>
    private System.Random seedRandom;
    [SerializeField] private float multiplyRandomNumbersWith = 10000f;
    [SerializeField] private float addRandomNumbersWith = 10000f; // add this to the random numbers to avoid negative numbers, which give symmetric noise

    [Header("WORLD GENERATION SETTINGS")]

    [Header("Perlin generator settings")]
    [SerializeField] public PerlinGenerator inlandnessPerlinGenerator; // a perlin generator for the inlandness of the terrain, ie how elevated and mountainous it is
    [SerializeField] public float inlandnessHeightMultiplier = 30f;

    [SerializeField] public PerlinGenerator humidityPerlinGenerator; // a perlin generator for the humidity of the terrain. for instance used for biome generation
    [SerializeField] public PerlinGenerator heatPerlinGenerator; // a perlin generator for the heat of the terrain. for instance used for biome generation

    // all biomes that can be generated

    [SerializeField] private BiomeSO[] biomeSOs;
    public List<Biome> biomes = new List<Biome>();




    public static TerrainGenerator Instance {get; private set;}

    // -------------------------

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Debug.LogError("There should only be one instance of TerrainGenerator");
        }

        seedRandom = new System.Random(seed);
    }
    

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
            Biome biome = biomeSO.biome;
            biomes.Add(biome);
            biome.Initialize();

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
        Vector2 rand = new Vector2((float)seedRandom.NextDouble()+1, (float)seedRandom.NextDouble()+1) * multiplyRandomNumbersWith; // random between 1 and 2 because sampling negative numbers gives symmetric noise
        rand += new Vector2(addRandomNumbersWith, addRandomNumbersWith);
        return rand;
    }



    public void GenerateChunkTextures(
        SquareMeshObject squareMeshObject,
        int LOD = 0,
        bool setTextures = true
        ) {
        
        Profiler.BeginSample("WorldGeneration/GenerateTerrain/GenerateChunkTextures/SetTextures");
        // set the textures
        if (setTextures) {

            SetAllTextures(squareMeshObject, squareMeshObject.inlandnessHumidityHeatTexture, squareMeshObject.plainnessBumpinessSteepnessTexture, squareMeshObject.plainnessBumpinessSteepnessTexture.height, squareMeshObject.plainnessBumpinessSteepnessTexture.width);

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
    private void SetAllTextures(SquareMeshObject squareMeshObject, Texture2D inlHumHeatTexture, Texture2D plainBumpSteepTexture, int textureHeight = 10, int textureWidth = 10) {

        // Perhaps this could be optimised if it were to used the precomputed data in the chunkdataarray


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


public float SmoothingFunction(float x) {
    return x * x * (3 - 2 * x);
 }





    // ----------- Actual generation code --------------

    /// <summary>
    /// Generates the height of the terrain at a given position
    /// </summary>
    private float GetHeight(Vector2 position) {

        Profiler.BeginSample("WorldGeneration/GenerateTerrain/GenerateChunkTextures/SetVertexHeights/SamplePerlinNoise");
        float inlandness = inlandnessPerlinGenerator.SampleNosie(position, clamp: true);
        float humidity = humidityPerlinGenerator.SampleNosie(position, clamp: true);
        float heat = heatPerlinGenerator.SampleNosie(position, clamp: true);
        Profiler.EndSample();

        List<BiomeInterpolationInfo> interpolateBiomes = GetBiomesOnPoint(inlandness, heat, humidity);

        float heightSum = 0;
        foreach (BiomeInterpolationInfo interpolateBiome in interpolateBiomes) {
            
            heightSum += interpolateBiome.biome.GetHeight(position, inlandness, inlandnessHeightMultiplier) * interpolateBiome.weight;
        }

        return heightSum;

    }


    public float GetInlandness(Vector2 position) {
        return inlandnessPerlinGenerator.SampleNosie(position, clamp: true);
    }
    public float GetHumidity(Vector2 position) {
        return humidityPerlinGenerator.SampleNosie(position, clamp: true);
    }
    public float GetHeat(Vector2 position) {
        return heatPerlinGenerator.SampleNosie(position, clamp: true);
    }

    public float GetHeight(Vector2 position, float inlandness, float humidity, float heat) {

        Profiler.BeginSample("WorldGeneration/GenerateTerrain/GenerateChunkTextures/SetVertexHeights/InterpolateBiomes");

        List<BiomeInterpolationInfo> interpolateBiomes = GetBiomesOnPoint(inlandness, heat, humidity);

        Profiler.EndSample();


        Profiler.BeginSample("WorldGeneration/GenerateTerrain/GenerateChunkTextures/SetVertexHeights/CalculateHeight");

        float heightSum = 0;
        foreach (BiomeInterpolationInfo interpolateBiome in interpolateBiomes) {
            heightSum += interpolateBiome.biome.GetHeight(position, inlandness, inlandnessHeightMultiplier) * interpolateBiome.weight;
        }
        
        Profiler.EndSample();
        return heightSum;
    }


    public void GenerateTerrainObjects(Chunk chunk, bool generateGameObjects = true) {

        // get a random but deterministic number from the chunk coordinates
        System.Random posRandom = new System.Random(chunk.chunkPosition.GetHashCode());
        System.Random choiceRandom = new System.Random(chunk.chunkPosition.GetHashCode());
        System.Random visualRandom = new System.Random(chunk.chunkPosition.GetHashCode());
        System.Random clusterRandom = new System.Random(chunk.chunkPosition.GetHashCode());

        for (int c = 0; c < 10; c++)
        {

            Vector2Int rPos = GetRandomTilePosition(posRandom, chunk.tiles.sideLength);


            ChunkTile tile = chunk.tiles.tiles[rPos.x, rPos.y];

            if (tile.terrainObject != null) continue; // there is already an object here

            // get biome at the random position
            float inlandness = tile.inlandness;
            float humidity = tile.humidity;
            float heat = tile.heat;

            List<BiomeInterpolationInfo> interpolateBiomes = GetBiomesOnPoint(inlandness, heat, humidity);

            List<TerrainObject> possibleObjects = new List<TerrainObject>();
            List<float> objectChances = new List<float>();

            float totalRange = 0f;

            foreach (BiomeInterpolationInfo interpolateBiome in interpolateBiomes)
            {
                foreach (TerrainObjectSO tObjSO in interpolateBiome.biome.terrainObjectSOs)
                {
                    TerrainObject tObj = tObjSO.terrainObject;

                    if (tObj.biomeThreshold > interpolateBiome.weight) continue; // the biome threshold is not met, skip this object

                    // add up chances to spawn
                    possibleObjects.Add(tObj);
                    float range = tObj.spawnWeight * interpolateBiome.weight; // weight it with the biome weight
                    objectChances.Add(range);
                    totalRange += range;

                }
            }

            // try to spawn an object
            TerrainObject spawnedObject = TryGenerateTerrainObjectAt(chunk, generateGameObjects, choiceRandom, visualRandom, rPos, possibleObjects, objectChances, totalRange);

            // clustering objects
            if (spawnedObject != null) { // if we spawned an object
                if (spawnedObject.clusterableTerrainObjects.Length > 0) {
                // try to cluster objects

                    for (int i = 0; i <  spawnedObject.maxClusteredObjects; i++) {
                        ClusterableTerrainObject clusterableTerrainObject = spawnedObject.clusterableTerrainObjects[clusterRandom.Next(spawnedObject.clusterableTerrainObjects.Length)];
                        if (clusterRandom.NextDouble() < clusterableTerrainObject.chanceToCluster) {
                            // move a random distance and see if the position is within the bounds of the chunk
                            rPos += new Vector2Int(clusterRandom.Next(-clusterableTerrainObject.clusterDistanceTiles, clusterableTerrainObject.clusterDistanceTiles), clusterRandom.Next(-clusterableTerrainObject.clusterDistanceTiles, clusterableTerrainObject.clusterDistanceTiles));
                            if (rPos.x >= 1 && rPos.x < chunk.tiles.sideLength-1 && rPos.y >= 1 && rPos.y < chunk.tiles.sideLength-1) { // cannot spawn on chunk edges
                                // try to spawn the object
                                TryGenerateTerrainObjectAt(chunk, generateGameObjects, choiceRandom, visualRandom, rPos, new List<TerrainObject>() { clusterableTerrainObject.terrainObjectSO.terrainObject }, new List<float>() { 1f }, 1f, dontRollSpawnChance: true);
                            }
                        } else {
                            break;
                        }
                    }

                }
            }

        }


    }

    private TerrainObject TryGenerateTerrainObjectAt(Chunk chunk, bool generateGameObjects, System.Random choiceRandom, System.Random visualRandom, Vector2Int rPos, List<TerrainObject> possibleObjects, List<float> objectChances, float totalRange, bool dontRollSpawnChance = false)
    {
        TerrainObject t = null;
        float randomValue = (float)choiceRandom.NextDouble() * totalRange;
        for (int i = 0; i < objectChances.Count; i++)
        {
            if (randomValue < objectChances[i])
            {
                if ((float)choiceRandom.NextDouble() < possibleObjects[i].chanceToSpawn || dontRollSpawnChance)
                { // roll to see if we spawn this object

                    TerrainObject objToSpawn = possibleObjects[i];

                    // calculate the positions of the surrounding tiles. Make sure the object fits in the chunk
                    int xMultiplier = (rPos.x < chunk.tiles.sideLength / 2) ? 1 : -1;
                    int yMultiplier = (rPos.y < chunk.tiles.sideLength / 2) ? 1 : -1;

                    // check if we can create an object here
                    // check if another object is overlapped by this one
                    bool overlapping = false;
                    bool tooSteep = false;
                    for (int x = 0; x < objToSpawn.xSize; x++)
                    {
                        if (overlapping) break;
                        if (tooSteep) break;
                        for (int y = 0; y < objToSpawn.ySize; y++)
                        {

                            // check the tile
                            ChunkTile checkTile = chunk.tiles.tiles[rPos.x + x * xMultiplier, rPos.y + y * yMultiplier];

                            if (checkTile.terrainObject != null)
                            {
                                overlapping = true;
                                break;
                            }

                            // only check steepness in even tiles and on the edges
                            if ((y % 2 == 0 && x % 2 == 0) || x == 0 || y == 0 || x == objToSpawn.xSize - 1 || y == objToSpawn.ySize - 1)
                            {
                                float steep = checkTile.GetMaxSteepness();
                                if (steep > objToSpawn.steepnessLimit)
                                {
                                    tooSteep = true;
                                    break;
                                }
                            }


                        }
                    }

                    if (overlapping) break; // dont spawn if were overlapping another object
                    if (tooSteep) break; // dont spawn if the terrain is too steep

                    Vector2Int coordinates = rPos + chunk.chunkPosition * WorldDataGenerator.instance.chunkTilesSideLength; // calculate the coordinates of the object in the world

                    objToSpawn = new TerrainObject(objToSpawn, coordinates); // create a copied object so we dont modify the original

                    chunk.terrainObjects.Add(objToSpawn); // add the object to the chunk so it can easily be found by npcs

                    // spawn the object in all the tiles it sbould occupy
                    for (int x = 0; x < objToSpawn.xSize; x++)
                    {
                        for (int y = 0; y < objToSpawn.ySize; y++)
                        {
                            chunk.tiles.tiles[rPos.x + x * xMultiplier, rPos.y + y * yMultiplier].terrainObject = objToSpawn;
                        }
                    }

                    if (generateGameObjects)
                    {

                        // instantiate the object
                        int randomIndex = visualRandom.Next(objToSpawn.prefabs.Length);
                        GameObject prefab = objToSpawn.prefabs[randomIndex];
                        Vector3 pos1 = chunk.tiles.GetTileWorldPosition(rPos.x, rPos.y);
                        Vector3 pos2 = chunk.tiles.GetTileWorldPosition(rPos.x + (objToSpawn.xSize - 1) * xMultiplier, rPos.y + (objToSpawn.ySize - 1) * yMultiplier);

                        Vector3 pos = (pos1 + pos2) / 2; // center of the object

                        GameObject obj = Instantiate(prefab, pos, Quaternion.identity, transform); // parent it to the terrain generator object
                        objToSpawn.createdObject = obj;

                        // set random rotation and scale
                        if (objToSpawn.randomRotation)
                        {
                            float randomRotation = (float)visualRandom.NextDouble() * 360f;
                            obj.transform.Rotate(new Vector3(0, randomRotation, 0));
                        }

                        if (objToSpawn.randomScale)
                        {
                            float randomScale = (float)visualRandom.NextDouble() * objToSpawn.scaleRandomNess * 2 - objToSpawn.scaleRandomNess + 1;
                            float scale = objToSpawn.scale * randomScale;
                            obj.transform.localScale = new Vector3(scale, scale, scale);
                        }
                        else
                        {
                            obj.transform.localScale = new Vector3(objToSpawn.scale, objToSpawn.scale, objToSpawn.scale);
                        }

                    }

                    t = objToSpawn; // save the object to return it

                }
                break;
            }
            randomValue -= objectChances[i];
        }

        return t;
    }


    /// <summary>
    /// Returns a random tile position in the chunk. 
    /// </summary>
    /// <param name="random"></param>
    /// <param name="sideLength"></param>
    /// <returns></returns>
    private Vector2Int GetRandomTilePosition(System.Random random, int sideLength, bool allowEdge = false) {
        if (allowEdge) {
            return new Vector2Int(random.Next(sideLength), random.Next(sideLength));
        } else {
            return new Vector2Int(random.Next(1, sideLength-1), random.Next(1, sideLength-1));
        }
    }
}











public struct SetAllTexturesJob : IJob {
    public NativeArray<float> inlHumHeatRed;
    public NativeArray<float> inlHumHeatGreen;
    public NativeArray<float> inlHumHeatBlue;
    public NativeArray<float> plainBumpSteepRed;
    public NativeArray<float> plainBumpSteepGreen;
    public NativeArray<float> plainBumpSteepBlue;

    public int textureHeight;
    public int textureWidth;

    public float sampleXFrom;
    public float sampleZFrom;

    public float quadSize;
    public int sizeX;
    public int sizeZ;

    public float inlandnessHeightMultiplier;

    public SetAllTexturesJob(
        int textureHeight,
        int textureWidth,
        float sampleXFrom,
        float sampleZFrom,
        float quadSize,
        int sizeX,
        int sizeZ,
        float inlandnessHeightMultiplier
    ) {
        this.textureHeight = textureHeight;
        this.textureWidth = textureWidth;
        this.sampleXFrom = sampleXFrom;
        this.sampleZFrom = sampleZFrom;
        this.quadSize = quadSize;
        this.sizeX = sizeX;
        this.sizeZ = sizeZ;
        this.inlandnessHeightMultiplier = inlandnessHeightMultiplier;

        int texSize = textureHeight * textureWidth;
        inlHumHeatRed = new NativeArray<float>(texSize, Allocator.TempJob);
        inlHumHeatGreen = new NativeArray<float>(texSize, Allocator.TempJob);
        inlHumHeatBlue = new NativeArray<float>(texSize, Allocator.TempJob);
        plainBumpSteepRed = new NativeArray<float>(texSize, Allocator.TempJob);
        plainBumpSteepGreen = new NativeArray<float>(texSize, Allocator.TempJob);
        plainBumpSteepBlue = new NativeArray<float>(texSize, Allocator.TempJob);

    }


    public void Execute() {

        for (int i = 0; i < textureHeight * textureWidth; i++) {
            int x = i % textureWidth;
            int y = i / textureHeight;

            float sampleX = sampleXFrom + x * quadSize * sizeX / textureWidth;
            float sampleZ = sampleZFrom + y * quadSize * sizeZ / textureHeight;

            float inlandness = TerrainGenerator.Instance.inlandnessPerlinGenerator.SampleNosie(new Vector2(sampleX, sampleZ), clamp: true);
            float humidity = TerrainGenerator.Instance.humidityPerlinGenerator.SampleNosie(new Vector2(sampleX, sampleZ), clamp: true);
            float heat = TerrainGenerator.Instance.heatPerlinGenerator.SampleNosie(new Vector2(sampleX, sampleZ), clamp: true);


            
            List<BiomeInterpolationInfo> interpolateBiomes = GetBiomesOnPoint(inlandness, heat, humidity);


            inlHumHeatRed[i] = inlandness;
            inlHumHeatGreen[i] = humidity;
            inlHumHeatBlue[i] = heat;

            float r = 0f;
            float g = 0f;
            float b = 0f;
            foreach (BiomeInterpolationInfo interpolateBiome in interpolateBiomes) {

                        r += interpolateBiome.biome.plainnessPerlinGenerator.SampleNosie(new Vector2(sampleX, sampleZ), clamp: true, runThroughCurve: false, multiplyWithHeightMultiplier: false) * interpolateBiome.weight;
                        g += interpolateBiome.biome.bumpinessPerlinGenerator.SampleNosie(new Vector2(sampleX, sampleZ), clamp: true, runThroughCurve: false, multiplyWithHeightMultiplier: false) * interpolateBiome.weight;
                        b += interpolateBiome.biome.GetHeightGradient(new Vector2(sampleX, sampleZ), inlandness, inlandnessHeightMultiplier,raw:false).magnitude * interpolateBiome.weight; // feels a little bit stupid that this has to be interpolated between biomes, but kinda makes sense

                        // squareMeshObject.biomeMaskTextures.SetBiomeMask(interpolateBiome.biome.biomeMaskIndex, x, y, interpolateBiome.weight);

            }
            plainBumpSteepRed[i] = r;
            plainBumpSteepGreen[i] = g;
            plainBumpSteepBlue[i] = b;

            }
        }

            // Interpolates between the biomes on a given point
    private List<BiomeInterpolationInfo> GetBiomesOnPoint(float inlandness, float heat, float humidity, bool smoothInterpolation = true) {
    List<BiomeInterpolationInfo> interpolateBiomes = new List<BiomeInterpolationInfo>();

    Vector3 location = new Vector3(inlandness, heat, humidity);
    float totalWeight = 0f;

    foreach (Biome biome in TerrainGenerator.Instance.biomes) {
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

        if (smoothInterpolation) weight = TerrainGenerator.Instance.SmoothingFunction(weight);
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

}

