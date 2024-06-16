
using UnityEngine;

/// <summary>
/// A class containing a square mesh object and the IRenderAround interfaces that belong to it
/// </summary>
public class Chunk {
    public Vector2Int chunkPosition {get; private set;}

    public ChunkDataArray chunkDataArray;
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

        this.chunkDataArray = new ChunkDataArray(WorldDataGenerator.instance.maxChunkSize, WorldDataGenerator.instance.LODlevels);
        this.squareMeshObject = squareMeshObject;

        this.visible = visible;
    }


    

}



/// <summary>
/// A class containing arrays of information about all the tiles/nodes/vertices in a chunk, used for ie pathfinding and other gameplay mechanics
/// </summary>
public class ChunkDataArray {
    

    private int size;
    private int maxLODs;
    private int CurrentLOD;

    // arrays
    public float[,] heights;

    public ChunkDataArray(int size, int maxLODs) {
        this.size = size;
        this.maxLODs = maxLODs;
        CurrentLOD = maxLODs - 1;

        InitializeArrays();
    }

    /// <summary>
    /// Returns the size of the array in the current LOD.
    /// </summary>
    public int GetArraySizeLOD() {
        return (int)(size / Mathf.Pow(2, CurrentLOD)) + 1;
    }

    public void InitializeArrays() {
        int s = GetArraySizeLOD();
        heights = new float[s, s];
    }

}