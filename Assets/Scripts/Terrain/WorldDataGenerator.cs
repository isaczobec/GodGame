using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class WorldDataGenerator: MonoBehaviour {


    [Header("World Generation Settings")]
    [SerializeField] public int maxChunkSize;
    [SerializeField] public int initialWorldSize;
    [SerializeField] public int fullWorldSizeChunks;
    [SerializeField] public int LODlevels;
    [SerializeField] public float quadSize;


    [Header("basic entities")]
    [SerializeField] private List<BasicEntity> basicEntities = new List<BasicEntity>(); // entities that will be rendered around
    List<IRenderAround> renderArounds = new List<IRenderAround>();



    public ChunkTree chunkTree {get; private set;}


    public static WorldDataGenerator instance {get; private set;}

    public void Awake()
    {
        if (instance == null)
        {instance = this;}
        else
        {Debug.LogError("There should only be one instance of WorldDataGenerator");}
        CheckValuesValid();
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

    public void Start() {
        chunkTree = new ChunkTree(new Vector2Int(0,0), new Vector2Int(fullWorldSizeChunks, fullWorldSizeChunks), fullWorldSizeChunks);


        foreach (BasicEntity basicEntity in basicEntities) {
            renderArounds.Add(basicEntity);
        }

        // generate initial world
        for (int i = -initialWorldSize; i < initialWorldSize; i++) {
            for (int j = -initialWorldSize; j < initialWorldSize; j++) {
                PromptChunkGeneration(new Vector2Int(i + fullWorldSizeChunks/2, j + fullWorldSizeChunks/2), generateMesh: true);
            }
        }

        StartCoroutine(UpdateRenderArounds());
    }



    public Vector2 GetChunkWorldPostion(Vector2Int chunkCoordinates, bool offset = true) {
        float o = offset? 0.5f : 0f;
        return new Vector2((chunkCoordinates.x - fullWorldSizeChunks/2 + o) * (maxChunkSize-1) * quadSize, (chunkCoordinates.y - fullWorldSizeChunks/2 + o) * (maxChunkSize-1) * quadSize);
    }

    public Vector2Int GetChunkCoordinates(Vector2 worldPosition) {
        return new Vector2Int(Mathf.FloorToInt(worldPosition.x / (maxChunkSize+1) / quadSize + fullWorldSizeChunks/2), Mathf.FloorToInt(worldPosition.y / (maxChunkSize+1) / quadSize + fullWorldSizeChunks/2));
    }



    private void PromptChunkGeneration(Vector2Int chunkCoordinates, bool generateMesh = false) {

        // check if the chunk is within the bounds of the world
        if (chunkCoordinates.x < 0 || chunkCoordinates.y < 0 || chunkCoordinates.x >= fullWorldSizeChunks || chunkCoordinates.y >= fullWorldSizeChunks) {
            return;
        }

        Chunk chunk = chunkTree.CreateOrGetChunk(chunkCoordinates, allowCreation:true);
        if (chunk.generated == false)
        {
            chunk.GenerateChunk();
            if (generateMesh)
            {
                Vector2 worldPosition = GetChunkWorldPostion(chunkCoordinates); 
                SquareMeshObject sqr = MeshGenerator.instance.CreateSquareMeshGameObject(worldPosition, maxChunkSize, maxChunkSize, chunkCoordinates);
                chunk.GenerateMesh(sqr, generateMeshCollider: true);
            }
        }

    }

    private void PromptRenderArounds() {


        foreach (IRenderAround renderAround in renderArounds) {

            // calculate chunk coordinates
            Vector2 centerPosition = renderAround.getCenterPosition();
            Vector2Int chunkCoordinates = GetChunkCoordinates(centerPosition);

            int sideLength = renderAround.getRenderDistanceChunks() * 2 + -1; // only uneven numbers
            for (int i = 0; i < sideLength; i++) {
                for (int j = 0; j < sideLength; j++) {

                    // Create a chunk if it doesn't exist
                    PromptChunkGeneration(chunkCoordinates + new Vector2Int(i-(int)Mathf.Floor(sideLength/2), j-(int)Mathf.Floor(sideLength/2)), generateMesh: true);

                }
            }
        }

    }

    private IEnumerator UpdateRenderArounds() {
        while (true) {
            PromptRenderArounds();
            yield return new WaitForSeconds(1f);
        }
    }

}