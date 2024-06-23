
using System.Collections;
using Unity.Jobs;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.Profiling;

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

        chunkDataArray = new ChunkDataArray(WorldDataGenerator.instance.maxChunkSize, WorldDataGenerator.instance.LODlevels, chunkPosition: chunkPosition);
        this.squareMeshObject = squareMeshObject;

        this.visible = visible;


    }

    public void GenerateChunk() {
        if (generated) return;
        generated = true;

        chunkDataArray.SetValues(); // generate the arrays of data for this chunk

    }

    public void GenerateMesh(SquareMeshObject squareMeshObject, bool generateMeshCollider = true) {
        if (!generated) {
            Debug.Log("This chunk has not been generated and its mesh can not be created!");
            return;
        }

        this.squareMeshObject = squareMeshObject;
        SetMeshHeights();

        int LOD = WorldDataGenerator.instance.LODlevels-1;
        TerrainGenerator.Instance.GenerateChunkTextures(squareMeshObject, LOD: LOD);

        squareMeshObject.SetLOD(LOD);

        if (generateMeshCollider) {
            squareMeshObject.AddMeshCollider();
        }

    }

    

    public void SetMeshHeights() {
        squareMeshObject.SetMeshHeights(chunkDataArray.heightArray, chunkDataArray.currentLOD, true);
    }

    public void UpscaleLOD(bool updateMesh = false) {
        chunkDataArray.UpscaleLOD();
        if (updateMesh) {

        SetMeshHeights();
        squareMeshObject.SetLOD(chunkDataArray.currentLOD);
        }
    }

    public IEnumerator tC() {
        while (chunkDataArray.currentLOD > 0) {
            yield return new WaitForSeconds(1f);
            UpscaleLOD(updateMesh: true);
        }
    }


}





/// <summary>
/// A class containing arrays of information about all the tiles/nodes/vertices in a chunk, used for ie pathfinding and other gameplay mechanics
/// </summary>
public struct ChunkDataArray {
    

    private int maxSize;
    private int maxLODs;
    public int currentLOD;

    private Vector2Int chunkPosition;

    // arrays
    public float[,] inlandnessArray;
    public float[,] humidityArray;
    public float[,] heatArray;
    public float[,] heightArray;

    public ChunkDataArray(int size, int maxLODs, Vector2Int chunkPosition) {
        this.maxSize = size;
        this.maxLODs = maxLODs;
        this.chunkPosition = chunkPosition;
        currentLOD = maxLODs - 1;

        inlandnessArray = null;
        humidityArray = null;
        heatArray = null;
        heightArray = null;

        InitializeArrays();
    }

    /// <summary>
    /// Returns the size of the array in the current LOD.
    /// </summary>
    public int GetArraySizeLOD() {
        if (currentLOD == 0) return maxSize;
        return (int)(maxSize / Mathf.Pow(2, currentLOD)) + 1;
    }

    /// <summary>
    /// Upscale the LOD of this chunk one step (x 2res).
    /// </summary> 
    public void UpscaleLOD() {
        if (currentLOD > 0) {
            currentLOD--;

            int newSize = GetArraySizeLOD();

            inlandnessArray = UpscaleArray(inlandnessArray, newSize);
            humidityArray = UpscaleArray(humidityArray, newSize);
            heatArray = UpscaleArray(heatArray, newSize);
            heightArray = UpscaleArray(heightArray, newSize);

            SetValues(upscaling: true);
        }
    }

    /// <summary>
    /// Upscale the LOD of this array one step. (x2 res)
    /// </summary>
    private float[,] UpscaleArray(float[,] array, int newLen) {
        float[,] newVals = new float[newLen, newLen];

        for (int i = 0; i < array.GetLength(0); i++) {
            for (int j = 0; j < array.GetLength(1); j++) {
                newVals[i * 2, j * 2] = array[i, j];
            }
        }

        return newVals;

    }

    

    public void InitializeArrays() {
        int s = GetArraySizeLOD();
        inlandnessArray = new float[s, s];
        humidityArray = new float[s, s];
        heatArray = new float[s, s];
        heightArray = new float[s, s];
    }

    public void SetValues(bool upscaling = false) {
    int s = GetArraySizeLOD();
    Vector2 origin = WorldDataGenerator.instance.GetChunkWorldPostion(chunkPosition, offset: false);
    float quadSize = WorldDataGenerator.instance.quadSize;
    float LODmultiplier = Mathf.Pow(2, currentLOD); // Adjust for LOD

    // Loop through all points to sample
    for (int i = 0; i < s; i++) {
        int js = s; // Default sampling rate

        // Adjust sampling rate if upscaling
        if (upscaling && i % 2 == 0) {
            js *= 2; // Double the sampling rate for every other row when upscaling
        }

        for (int j = 0; j < js; j++) {
            // Calculate the world position of the point
            Vector2 pos = new Vector2(i, j / (upscaling && i % 2 == 0 ? 2.0f : 1.0f)) * quadSize * LODmultiplier + origin;

            // Calculate the index of the point in the arrays to set
            int xIndex = i;
            int yIndex = j;
            if (upscaling && i % 2 == 0) {
                yIndex = j / 2; // Correctly map the yIndex for the upscaled grid
            }

            Profiler.BeginSample("WorldGeneration/GenerateTerrain/GenerateChunkTextures/SetVertexHeights/SamplePerlinNoise");

            // Set the values of the arrays
            float inlandness = TerrainGenerator.Instance.GetInlandness(pos);
            float humidity = TerrainGenerator.Instance.GetHumidity(pos);
            float heat = TerrainGenerator.Instance.GetHeat(pos);

            Profiler.EndSample();

            Profiler.BeginSample("WorldGeneration/GenerateTerrain/GenerateChunkTextures/SetVertexHeights/CalculateHeights");
            float height = TerrainGenerator.Instance.GetHeight(pos, inlandness, humidity, heat);
            Profiler.EndSample();

            inlandnessArray[xIndex, yIndex] = inlandness;
            humidityArray[xIndex, yIndex] = humidity;
            heatArray[xIndex, yIndex] = heat;
            heightArray[xIndex, yIndex] = height;
        }
    }
}

    public struct GenerateChunkJob : IJob
    {
        public ChunkDataArray chunkDataArray;
        public void Execute()
        {
            chunkDataArray.SetValues();
        }
    }

}