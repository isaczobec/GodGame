using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;


/// <summary>
/// A class containing a square mesh object and the IRenderAround interfaces that belong to it


public class Chunk {
    public Vector2Int chunkPosition {get; private set;}
    public SquareMeshObject squareMeshObject;

    public bool generated = false;
    public bool discovered = false;

    public bool visible {
    set {
            squareMeshObject?.SetVisibilityMultiplier(value? 1f : 0f);
        } 
    } 

    public Chunk(Vector2Int chunkPosition, SquareMeshObject squareMeshObject = null, bool visible = false) {
        this.chunkPosition = chunkPosition;
        this.squareMeshObject = squareMeshObject;

        this.visible = visible;
    }

}

public class MeshGenerator : MonoBehaviour
{

    // ------------ MESH GENERATION SETTINGS ------------
    [Header("Mesh Generation Settings")]


    // variables for generating the mesh
    [SerializeField] private float quadSize = 1f;
    [SerializeField] private float chunkSize = 50f;


    [SerializeField] private int initialWorldSize = 20;

    [SerializeField] private int fullWorldSizeChunks = 100;



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
    private Chunk[,] chunksArray;



    void Start()
    {

        InitializeChunkArray();

        // add all the basic entities to the renderArounds list
        foreach (BasicEntity basicEntity in basicEntities) {
            renderArounds.Add(basicEntity);
        }



        for (int i = -initialWorldSize; i < initialWorldSize; i++) {
            for (int j = -initialWorldSize; j < initialWorldSize; j++) {
                PromptChunkGeneration(new Vector2Int(i + fullWorldSizeChunks/2, j + fullWorldSizeChunks/2));
                
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
        meshObject.squareMesh = new VisibleSquareMesh(centerPosition, xSize, zSize, quadSize, chunkCoordinates);
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
    private void GenerateTerrain(SquareMeshObject squareMeshObject, bool updateMesh = true, bool generateMeshCollider = true) {

        Profiler.BeginSample("WorldGeneration/GenerateTerrain/GenerateChunk");
        terrainGenerator.GenerateChunk(squareMeshObject);
        Profiler.EndSample();

        Profiler.BeginSample("WorldGeneration/GenerateTerrain/UpdateMesh");
        if (updateMesh) {
            UpdateMesh(squareMeshObject);
        }
        Profiler.EndSample();

        if (generateMeshCollider) {
            squareMeshObject.AddMeshCollider();
        }
    }

    /// <summary>
    /// Generates the height of all squareMeshObjects using GenerateTerrain().
    /// </summary>
    private void GenerateAllHeights(bool updateMesh = true) {
        foreach (SquareMeshObject meshObject in squareMeshObjects) {
            GenerateTerrain(meshObject, false);
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

    private void InitializeChunkArray() {
        chunksArray = new Chunk[fullWorldSizeChunks, fullWorldSizeChunks];

        for (int i = 0; i < fullWorldSizeChunks; i++) {
            for (int j = 0; j < fullWorldSizeChunks; j++) {
                chunksArray[i, j] = new Chunk(new Vector2Int(i, j));
            }
        }
    }


    public Vector2 GetChunkWorldPostion(Vector2Int chunkCoordinates) {
        return new Vector2((chunkCoordinates.x - fullWorldSizeChunks/2 + 0.5f) * chunkSize * quadSize, (chunkCoordinates.y - fullWorldSizeChunks/2 + 0.5f) * chunkSize * quadSize);
    }

    public Vector2Int GetChunkCoordinates(Vector2 worldPosition) {
        return new Vector2Int(Mathf.FloorToInt(worldPosition.x / chunkSize / quadSize + fullWorldSizeChunks/2), Mathf.FloorToInt(worldPosition.y / chunkSize / quadSize + fullWorldSizeChunks/2));
    }

    public Chunk GetChunk(Vector2Int chunkCoordinates) {
        return chunksArray[chunkCoordinates.x, chunkCoordinates.y];
    }

    private void PromptChunkGeneration(Vector2Int chunkCoordinates) {

        // check if the chunk is within the bounds of the world
        if (chunkCoordinates.x < 0 || chunkCoordinates.y < 0 || chunkCoordinates.x >= fullWorldSizeChunks || chunkCoordinates.y >= fullWorldSizeChunks) {
            return;
        }

        if (chunksArray[chunkCoordinates.x, chunkCoordinates.y].discovered == false)
        {
            GenerateChunkAtCoordinates(chunkCoordinates);
        }
    }

    private void GenerateChunkAtCoordinates(Vector2Int chunkCoordinates)
    {
        Profiler.BeginSample("WorldGeneration/CreateSquareMeshObject");
        SquareMeshObject newMeshObject = CreateSquareMeshGameObject(GetChunkWorldPostion(chunkCoordinates), (int)chunkSize, (int)chunkSize, chunkCoordinates);
        Profiler.EndSample();

        Profiler.BeginSample("WorldGeneration/GenerateTerrain");
        GenerateTerrain(newMeshObject);
        Profiler.EndSample();
        chunksArray[chunkCoordinates.x, chunkCoordinates.y].squareMeshObject = newMeshObject;
        chunksArray[chunkCoordinates.x, chunkCoordinates.y].discovered = true;
        chunksArray[chunkCoordinates.x, chunkCoordinates.y].generated = true;

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
                    PromptChunkGeneration(chunkCoordinates + new Vector2Int(i-(int)Mathf.Floor(sideLength/2), j-(int)Mathf.Floor(sideLength/2)));

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



