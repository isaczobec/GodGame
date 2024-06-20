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

    public static MeshGenerator instance {get; private set;}


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



    private void Awake() {
        if (instance == null) {
            instance = this;
        } else {
            Debug.LogError("There should only be one instance of MeshGenerator");
        }
    }

    void Start()
    {
        
        worldDataGenerator = WorldDataGenerator.instance;
        chunkTreeRef = worldDataGenerator.chunkTree;

        maxChunkSize = worldDataGenerator.maxChunkSize;
        initialWorldSize = worldDataGenerator.initialWorldSize;
        fullWorldSizeChunks = worldDataGenerator.fullWorldSizeChunks;
        LODlevels = worldDataGenerator.LODlevels;
        quadSize = worldDataGenerator.quadSize;


    }



    /// <summary>
    /// Creates and returns a square mesh at the given position with the given size and an accompaying mesh filter
    /// </summary>
    public SquareMeshObject CreateSquareMeshGameObject(Vector2 centerPosition, int xSize, int zSize, Vector2Int chunkCoordinates) {
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



    public Vector2 GetChunkWorldPostion(Vector2Int chunkCoordinates) {
        return new Vector2((chunkCoordinates.x - fullWorldSizeChunks/2 + 0.5f) * (maxChunkSize-1) * quadSize, (chunkCoordinates.y - fullWorldSizeChunks/2 + 0.5f) * (maxChunkSize-1) * quadSize);
    }

    public Vector2Int GetChunkCoordinates(Vector2 worldPosition) {
        return new Vector2Int(Mathf.FloorToInt(worldPosition.x / (maxChunkSize+1) / quadSize + fullWorldSizeChunks/2), Mathf.FloorToInt(worldPosition.y / (maxChunkSize+1) / quadSize + fullWorldSizeChunks/2));
    }

    public Chunk GetChunk(Vector2Int chunkCoordinates) {
        return chunkTreeRef.CreateOrGetChunk(chunkCoordinates,allowCreation:false);
    }


}
