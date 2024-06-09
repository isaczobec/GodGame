using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A class containing a square mesh object and the IRenderAround interfaces that belong to it


public class Chunk {
    public Vector2Int chunkPosition {get; private set;}
    public SquareMeshObject squareMeshObject {get; private set;}

    public Chunk(Vector2Int chunkPosition, SquareMeshObject squareMeshObject) {
        this.chunkPosition = chunkPosition;
        this.squareMeshObject = squareMeshObject;
    }

}

public class MeshGenerator : MonoBehaviour
{

    // ------------ MESH GENERATION SETTINGS ------------
    [Header("Mesh Generation Settings")]


    // variables for generating the mesh
    [SerializeField] private float quadSize = 1f;
    [SerializeField] private float chunkSize = 50f;



    // ------------ MESH GENERATION OBJECTS ------------

    [SerializeField] private Material baseMaterial;

    List<SquareMeshObject> squareMeshObjects = new List<SquareMeshObject>();
    List<Vector2Int> discoveredChunks = new List<Vector2Int>(); // chunk coordinates that have been discovered
    List<Chunk> chunks = new List<Chunk>();


    [SerializeField] private List<BasicEntity> basicEntities = new List<BasicEntity>(); // entities that will be rendered around
    List<IRenderAround> renderArounds = new List<IRenderAround>();

    [SerializeField] private TerrainGenerator terrainGenerator;



    void Start()
    {
        // add all the basic entities to the renderArounds list
        foreach (BasicEntity basicEntity in basicEntities) {
            renderArounds.Add(basicEntity);
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
    private SquareMeshObject CreateSquareMeshGameObject(Vector2 centerPosition, int xSize, int zSize) {
        // GameObject squareMeshObject = Instantiate(GameObject., Vector3.zero, Quaternion.identity, transform);

        GameObject squareMeshObject = new GameObject("SquareMeshObject");
        squareMeshObject.transform.parent = transform;

        SquareMeshObject meshObject = squareMeshObject.AddComponent<SquareMeshObject>();
        squareMeshObjects.Add(meshObject);
        meshObject.squareMesh = new VisibleSquareMesh(centerPosition, xSize, zSize, quadSize);
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
    private void GenerateTerrain(SquareMeshObject squareMeshObject, bool updateMesh = true) {

        terrainGenerator.GenerateChunk(squareMeshObject);

        if (updateMesh) {
            UpdateMesh(squareMeshObject);
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


    // ------------ CHUNK GENERATION ------------

    private void PromptChunkCoordinates(Vector2Int chunkCoordinates) {
        if (!discoveredChunks.Contains(chunkCoordinates)) {
            discoveredChunks.Add(chunkCoordinates);
            SquareMeshObject newMeshObject = CreateSquareMeshGameObject(new Vector2(chunkCoordinates.x * chunkSize, chunkCoordinates.y * chunkSize), (int)chunkSize, (int)chunkSize);
            GenerateTerrain(newMeshObject);
            chunks.Add(new Chunk(chunkCoordinates, newMeshObject));
        }
    }

    private void PromptRenderArounds() {
        foreach (IRenderAround renderAround in renderArounds) {

            // calculate chunk coordinates
            Vector2 centerPosition = renderAround.getCenterPosition();
            int cX = centerPosition.x < 0? (int)(centerPosition.x / chunkSize - 0.5f) : (int)(centerPosition.x / chunkSize + 0.5f);
            int cY = centerPosition.y < 0? (int)(centerPosition.y / chunkSize - 0.5f) : (int)(centerPosition.y / chunkSize + 0.5f);
            Vector2Int chunkCoordinates = new Vector2Int(cX, cY);

            int sideLength = renderAround.getRenderDistanceChunks() * 2 + -1; // only uneven numbers
            for (int i = 0; i < sideLength; i++) {
                for (int j = 0; j < sideLength; j++) {
                    PromptChunkCoordinates(chunkCoordinates + new Vector2Int(i-(int)Mathf.Floor(sideLength/2), j-(int)Mathf.Floor(sideLength/2)));
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



