using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;



public class MeshGenerator : MonoBehaviour
{

    // ------------ MESH GENERATION SETTINGS ------------
    [Header("Mesh Generation Settings")]


    // variables for generating the mesh

    // theese are gotten from the worldDataGenerator
    private float quadSize = 1f;
    private float maxChunkSize = 50f;
    private int initialWorldSize = 20;
    private int fullWorldSizeChunks = 256;
    private int LODlevels = 4;


    private WorldDataGenerator worldDataGenerator; // used to get data about the world


    // ------------ MESH GENERATION OBJECTS ------------

    [SerializeField] private Material baseMaterial;

    List<SquareMeshObject> squareMeshObjects = new List<SquareMeshObject>();


    [SerializeField] private List<BasicEntity> basicEntities = new List<BasicEntity>(); // entities that will be rendered around
    List<IRenderAround> renderArounds = new List<IRenderAround>();

    [SerializeField] private TerrainGenerator terrainGenerator;


    // member variables

    /// <summary>
    /// A 2D array of all chunks in the world
    /// </summary>
    // private Chunk[,] chunksArray;

    private ChunkTree chunkTreeRef;



    void Start()
    {
        
        worldDataGenerator = WorldDataGenerator.instance;
        chunkTreeRef = worldDataGenerator.chunkTree;

        maxChunkSize = worldDataGenerator.maxChunkSize;
        initialWorldSize = worldDataGenerator.initialWorldSize;
        fullWorldSizeChunks = worldDataGenerator.fullWorldSizeChunks;
        LODlevels = worldDataGenerator.LODlevels;
        quadSize = worldDataGenerator.quadSize;

        // InitializeChunkArray();

        // add all the basic entities to the renderArounds list
        foreach (BasicEntity basicEntity in basicEntities) {
            renderArounds.Add(basicEntity);
        }



        for (int i = -initialWorldSize; i < initialWorldSize; i++) {
            for (int j = -initialWorldSize; j < initialWorldSize; j++) {
                PromptChunkMeshGeneration(new Vector2Int(i + fullWorldSizeChunks/2, j + fullWorldSizeChunks/2));
                
            }
        }

        StartCoroutine(UpdateRenderArounds());

    }

    private void Update() {
        // squareMeshObjects[0].squareMesh.verticies[0].y += 2f * Time.deltaTime;
        // squareMeshObjects[0].UpdateMesh();
    }


    /// <summary>
    /// Creates and returns a square mesh at the given position with the given size and an accompaying mesh filter
    /// </summary>
    private SquareMeshObject CreateSquareMeshGameObject(Vector2 centerPosition, int xSize, int zSize, Vector2Int chunkCoordinates) {
        // GameObject squareMeshObject = Instantiate(GameObject., Vector3.zero, Quaternion.identity, transform);

        GameObject squareMeshObject = new GameObject("SquareMeshObject");
        squareMeshObject.transform.parent = transform;

        SquareMeshObject meshObject = squareMeshObject.AddComponent<SquareMeshObject>();
        squareMeshObjects.Add(meshObject);
        meshObject.squareMesh = new VisibleSquareMesh(centerPosition, xSize, zSize, quadSize, chunkCoordinates, levelsOfDetail: LODlevels);
        meshObject.Initialize(baseMaterial:baseMaterial);

        return meshObject;
    }

    /// <summary>
    /// Deletes a square mesh object from the list and destroys the game object
    /// </summary>
    private void DeleteSquareMeshGameObject(SquareMeshObject meshToDelete) {
        if (!squareMeshObjects.Contains(meshToDelete)) {
            Debug.LogError("Trying to delete a mesh that doesn't exist in the list");
            return;
        }
        squareMeshObjects.Remove(meshToDelete);
        Destroy(meshToDelete.gameObject);
    }

    /// <summary>
    /// Generates the height of a squareMeshObject using perlin noise. Will be made more elaborate in the future
    /// </summary>
    private void GenerateChunkTerrain(SquareMeshObject squareMeshObject, bool generateMeshCollider = true) {

        terrainGenerator.GenerateChunkTextures(squareMeshObject, LOD: LODlevels-1);

        squareMeshObject.SetLOD(LODlevels-1);

        if (generateMeshCollider) {
            squareMeshObject.AddMeshCollider();
        }
    }

    /// <summary>
    /// Generates the height of all squareMeshObjects using GenerateChunkTerrain().
    /// </summary>
    private void GenerateAllHeights(bool updateMesh = true) {
        foreach (SquareMeshObject meshObject in squareMeshObjects) {
            GenerateChunkTerrain(meshObject); // set minimum detail level
        }
        if(updateMesh) UpdateAllMeshes();
    }

    /// <summary>
    /// Updates all meshes in the squareMeshObjects list
    /// </summary>
    private void UpdateAllMeshes() {
        foreach (SquareMeshObject meshObject in squareMeshObjects) {
            meshObject.UpdateMesh();
        }
    }

    private void UpdateMesh(SquareMeshObject meshObject) {
        meshObject.UpdateMesh();
        
    }














    // ------------ CHUNKS ------------


    // ---- chunk array management ----

    // private void InitializeChunkArray() {
    //     chunksArray = new Chunk[fullWorldSizeChunks, fullWorldSizeChunks];

    //     for (int i = 0; i < fullWorldSizeChunks; i++) {
    //         for (int j = 0; j < fullWorldSizeChunks; j++) {
    //             chunksArray[i, j] = new Chunk(new Vector2Int(i, j));
    //         }
    //     }
    // }


    public Vector2 GetChunkWorldPostion(Vector2Int chunkCoordinates) {
        return new Vector2((chunkCoordinates.x - fullWorldSizeChunks/2 + 0.5f) * (maxChunkSize-1) * quadSize, (chunkCoordinates.y - fullWorldSizeChunks/2 + 0.5f) * (maxChunkSize-1) * quadSize);
    }

    public Vector2Int GetChunkCoordinates(Vector2 worldPosition) {
        return new Vector2Int(Mathf.FloorToInt(worldPosition.x / (maxChunkSize+1) / quadSize + fullWorldSizeChunks/2), Mathf.FloorToInt(worldPosition.y / (maxChunkSize+1) / quadSize + fullWorldSizeChunks/2));
    }

    public Chunk GetChunk(Vector2Int chunkCoordinates) {
        return chunkTreeRef.CreateOrGetChunk(chunkCoordinates,allowCreation:false);
    }

    private void PromptChunkMeshGeneration(Vector2Int chunkCoordinates) {

        // check if the chunk is within the bounds of the world
        if (chunkCoordinates.x < 0 || chunkCoordinates.y < 0 || chunkCoordinates.x >= fullWorldSizeChunks || chunkCoordinates.y >= fullWorldSizeChunks) {
            return;
        }

        Chunk chunk = chunkTreeRef.CreateOrGetChunk(chunkCoordinates, allowCreation:true);
        if (chunk.discovered == false)
        {
            GenerateChunkMesh(chunk);
        }
    }

    private void GenerateChunkMesh(Chunk chunk)
    {
        SquareMeshObject newMeshObject = CreateSquareMeshGameObject(GetChunkWorldPostion(chunk.chunkPosition), (int)maxChunkSize, (int)maxChunkSize, chunk.chunkPosition);

        chunk.chunkDataArray.SetValues(); // generate the arrays of data for this chunk
        chunk.squareMeshObject = newMeshObject;
        chunk.SetMeshHeights();
        GenerateChunkTerrain(newMeshObject);
        chunk.discovered = true;
        chunk.generated = true;


        StartCoroutine(chunk.tC());

    }

    private void PromptRenderArounds() {

        Profiler.BeginSample("WorldGeneration/PromptRenderArounds");

        foreach (IRenderAround renderAround in renderArounds) {

            // calculate chunk coordinates
            Vector2 centerPosition = renderAround.getCenterPosition();
            Vector2Int chunkCoordinates = GetChunkCoordinates(centerPosition);

            int sideLength = renderAround.getRenderDistanceChunks() * 2 + -1; // only uneven numbers
            for (int i = 0; i < sideLength; i++) {
                for (int j = 0; j < sideLength; j++) {

                    // Create a chunk if it doesn't exist
                    PromptChunkMeshGeneration(chunkCoordinates + new Vector2Int(i-(int)Mathf.Floor(sideLength/2), j-(int)Mathf.Floor(sideLength/2)));

                }
            }
        }

        Profiler.EndSample();
    }

    private IEnumerator UpdateRenderArounds() {
        while (true) {
            PromptRenderArounds();
            yield return new WaitForSeconds(1f);
        }
    }
}



