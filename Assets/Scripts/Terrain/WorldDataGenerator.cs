using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using JetBrains.Annotations;
using UnityEngine.Profiling;
using Unity.VisualScripting;

class WorldDataGenerator : MonoBehaviour
{

    [SerializeField] private NPCSO testNPCSO;

    [Header("World Generation Settings")]
    [SerializeField] public int maxChunkSize;
    [SerializeField] public int initialWorldSize;
    [SerializeField] public int fullWorldSizeChunks;
    [SerializeField] public int LODlevels;
    [SerializeField] public float quadSize;

    /// <summary>
    /// How many world units a tile is wide and high.
    /// </summary>
    [HideInInspector] public float tileSize;

    /// <summary>
    /// How many tiles there are on one side of a the grid of tiles in a chunk.
    /// </summary>
    [HideInInspector] public int chunkTilesSideLength { get; private set; }

    [Header("basic entities")]
    [SerializeField] private List<BasicEntity> basicEntities = new List<BasicEntity>(); // entities that will be rendered around
    private List<NPC> NPCs = new List<NPC>(); // entities that will be rendered around
    List<IRenderAround> renderArounds = new List<IRenderAround>();

    public ChunkTree chunkTree { get; private set; }

    public List<Chunk> chunkGenerationQueue = new List<Chunk>();
    public List<bool> generateMeshQueue = new List<bool>();

    public List<Chunk> upscaleQueue = new List<Chunk>();

    public static WorldDataGenerator instance { get; private set; }



    public event EventHandler<EventArgs> OnBeforeDestroy;


    bool testSpawnedNPC = false;

    private void Awake()
    {
        int pow = (int)Mathf.Pow(2, LODlevels - 1);
        tileSize = quadSize * pow;

        chunkTilesSideLength = (int)((maxChunkSize-1) / pow);

        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogError("There should only be one instance of WorldDataGenerator");
        }
        CheckValuesValid();
    }

    private void OnDestroy() {
        OnBeforeDestroy?.Invoke(this, EventArgs.Empty);
    }


    private void CheckValuesValid()
    {
        // check so values are valid
        if ((maxChunkSize - 1) % Mathf.Pow(2, LODlevels) != 0)
        {
            Debug.LogError("maxChunkSize-1 must be a multiple of 2^LODlevels");
        }
        int p = 2;
        bool pow2 = false;
        while (p <= fullWorldSizeChunks)
        {
            p *= 2;
            if (p == fullWorldSizeChunks)
            {
                pow2 = true;
                break;
            }
        }
        if (!pow2)
        {
            Debug.LogError("fullWorldSizeChunks must be a power of 2");
        }
    }

    public void AddRenderAround(IRenderAround renderAround)
    {
        renderArounds.Add(renderAround);
    }

    public void Start()
    {
        chunkTree = new ChunkTree(new Vector2Int(0, 0), new Vector2Int(fullWorldSizeChunks, fullWorldSizeChunks), fullWorldSizeChunks);

        foreach (BasicEntity basicEntity in basicEntities)
        {
            renderArounds.Add(basicEntity);
        }

        // generate initial world
        for (int i = -initialWorldSize; i < initialWorldSize; i++)
        {
            for (int j = -initialWorldSize; j < initialWorldSize; j++)
            {
                PromptChunkGeneration(new Vector2Int(i + fullWorldSizeChunks / 2, j + fullWorldSizeChunks / 2), generateMesh: true);
            }
        }

        StartCoroutine(UpdateRenderArounds());

        StartCoroutine(UpscaleChunksCoroutine());
    }

    private void TestSpawnNPC() {
        Chunk chunk = chunkTree.CreateOrGetChunk(new Vector2Int(fullWorldSizeChunks / 2, fullWorldSizeChunks / 2), allowCreation: false);
        ChunkTile tile = chunk.tiles.tiles[chunk.tiles.sideLength / 2, chunk.tiles.sideLength / 2];
        NPC npc = NpcManager.instance.SpawnNPC(testNPCSO, tile);
        testSpawnedNPC = true;
    }


    private void Update()
    {
        GenerateChunksQueue();
        // test
        if (testSpawnedNPC == false) {
            TestSpawnNPC();
        }
    }

    public Vector2 GetChunkWorldPostion(Vector2Int chunkCoordinates, bool offset = true) {
        float o = offset ? 0.5f : 0f;
        return new Vector2((chunkCoordinates.x - fullWorldSizeChunks / 2 + o) * (maxChunkSize - 1) * quadSize, (chunkCoordinates.y - fullWorldSizeChunks / 2 + o) * (maxChunkSize - 1) * quadSize);
    }

    public Vector2Int GetChunkCoordinates(Vector2 worldPosition) {
        return new Vector2Int(Mathf.FloorToInt(worldPosition.x / (maxChunkSize + 1) / quadSize + fullWorldSizeChunks / 2), Mathf.FloorToInt(worldPosition.y / (maxChunkSize + 1) / quadSize + fullWorldSizeChunks / 2));
    }

    private void PromptChunkGeneration(Vector2Int chunkCoordinates, bool generateMesh = false)
    {
        // check if the chunk is within the bounds of the world
        if (chunkCoordinates.x < 0 || chunkCoordinates.y < 0 || chunkCoordinates.x >= fullWorldSizeChunks || chunkCoordinates.y >= fullWorldSizeChunks)
        {
            return;
        }

        Chunk chunk = chunkTree.CreateOrGetChunk(chunkCoordinates, allowCreation: true);
        if (chunk.generated == false && chunk.generationPrompted == false)
        {
            chunk.generationPrompted = true;
            EnqueueChunk(chunk, generateMesh);
        }
    }

    private void PromptRenderArounds()
    {
        foreach (IRenderAround renderAround in renderArounds)
        {
            // calculate chunk coordinates
            Vector2 centerPosition = renderAround.getCenterPosition();
            Vector2Int chunkCoordinates = GetChunkCoordinates(centerPosition);

            int sideLength = renderAround.getRenderDistanceChunks() * 2 + -1; // only uneven numbers
            for (int i = 0; i < sideLength; i++)
            {
                for (int j = 0; j < sideLength; j++)
                {
                    // Create a chunk if it doesn't exist
                    PromptChunkGeneration(chunkCoordinates + new Vector2Int(i - (int)Mathf.Floor(sideLength / 2), j - (int)Mathf.Floor(sideLength / 2)), generateMesh: true);
                }
            }
        }
    }

    private IEnumerator UpdateRenderArounds()
    {
        while (true)
        {
            PromptRenderArounds();
            yield return new WaitForSeconds(1f);
        }
    }

    // ----- Generation Queue -----

    public void EnqueueChunk(Chunk chunk, bool generateMesh)
    {
        chunkGenerationQueue.Add(chunk);
        generateMeshQueue.Add(generateMesh);
    }

    public void GenerateChunksQueue()
    {
        if (chunkGenerationQueue.Count == 0) return;

        GenerateChunkJobParallel[] generateChunkJobs = new GenerateChunkJobParallel[chunkGenerationQueue.Count];
        NativeArray<JobHandle> chunkJobHandles = new NativeArray<JobHandle>(chunkGenerationQueue.Count, Allocator.TempJob);

        for (int i = 0; i < chunkGenerationQueue.Count; i++)
        {
            Chunk chunk = chunkGenerationQueue[i];
            chunk.generated = true;

            generateChunkJobs[i] = chunk.GetGenerateChunkJobParallel();


            JobHandle jobHandle = generateChunkJobs[i].Schedule(generateChunkJobs[i].GetJobLength(),1); // Schedule the job
            chunkJobHandles[i] = jobHandle;

            upscaleQueue.Add(chunk);

            // Debug.Log("index: " + i);
        }


        // Wait for all jobs to finish
        JobHandle.CompleteAll(chunkJobHandles);
        chunkJobHandles.Dispose();

        // set the data back to the chunks
        for (int i = 0; i < chunkGenerationQueue.Count; i++)
        {
            Chunk chunk = chunkGenerationQueue[i];
            chunk.chunkDataArray = generateChunkJobs[i].chunkDataArray;

            chunk.tiles = chunk.chunkDataArray.CreateChunkTiles(); // create the tiles from the data array. Do this only for the lowest LOD
        }


        // generate meshes
        for (int i = 0; i < chunkGenerationQueue.Count; i++)
        {
            bool generateMesh = generateMeshQueue[i];

            if (generateMesh)
            {
                Chunk chunk = chunkGenerationQueue[i];
                Vector2Int chunkCoordinates = chunk.chunkPosition;

                Vector2 worldPosition = GetChunkWorldPostion(chunk.chunkPosition);
                SquareMeshObject sqr = MeshGenerator.instance.CreateSquareMeshGameObject(worldPosition, maxChunkSize, maxChunkSize, chunkCoordinates);
                chunk.GenerateMesh(sqr, generateMeshCollider: true);

            }
        }

        for (int i = 0; i < chunkGenerationQueue.Count; i++) {
            TerrainGenerator.Instance.GenerateTerrainObjects(chunkGenerationQueue[i]);
        }


        chunkGenerationQueue.Clear();
        generateMeshQueue.Clear();
    }


    private IEnumerator UpscaleChunksCoroutine() {
        while (true) {
            yield return new WaitForSeconds(0.3f);
            if (upscaleQueue.Count == 0) continue;

            NativeArray<JobHandle> chunkJobHandles = new NativeArray<JobHandle>(upscaleQueue.Count, Allocator.TempJob);
            GenerateChunkJobParallel[] upscaleJobs = new GenerateChunkJobParallel[upscaleQueue.Count];

            // float time = Time.realtimeSinceStartup;

            Profiler.BeginSample("UpscaleDataArrays");

            for (int i = 0; i < upscaleQueue.Count; i++) {
                if (upscaleQueue[i].chunkDataArray.currentLOD == 0) continue;

                Chunk chunk = upscaleQueue[i];

                chunk.chunkDataArray.UpscaleArraysLOD(); // upscale the data arrays. new memory cannot be allocated in jobs, so we need to do it here

                // get and schedule the jobs
                upscaleJobs[i] = chunk.GetGenerateChunkJobParallel(upscale: true);
                JobHandle jobHandle = upscaleJobs[i].Schedule(upscaleJobs[i].GetJobLength(),1);
                chunkJobHandles[i] = jobHandle;
            }
            Profiler.EndSample();

            Profiler.BeginSample("CompleteUpscaleJobs");

            // wait for jobs to finnish
            JobHandle.CompleteAll(chunkJobHandles);
            chunkJobHandles.Dispose();

            Profiler.EndSample();

            // set back the data and upscale the mesh
            for (int i = 0; i < upscaleQueue.Count; i++) {
                if (upscaleQueue[i].chunkDataArray.currentLOD == 0) continue;

                Chunk chunk = upscaleQueue[i];
                chunk.chunkDataArray = upscaleJobs[i].chunkDataArray;
                chunk.UpscaleLOD(updateMesh: true, needToUpscaleDataArray: false);
            }

            // Debug.Log("Time to upscale chunks: " + (Time.realtimeSinceStartup - time) * 1000 + "ms");

        }
    }


}
