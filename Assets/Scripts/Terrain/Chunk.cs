
using System.Collections;
using System.Data;
using TMPro;
using Unity.Burst;
using Unity.Collections;
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
    public bool generationPrompted = false;

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

    public GenerateChunkJob GetGenerateChunkJob() {
        GenerateChunkJob job = new GenerateChunkJob {
            chunkDataArray = chunkDataArray
        };

        return job;
    }

    public UpscaleLODJob GetUpscaleLODJob() {
        UpscaleLODJob job = new UpscaleLODJob {
            chunkDataArray = chunkDataArray
        };

        return job;
    }

    public void GenerateMesh(SquareMeshObject squareMeshObject, bool generateMeshCollider = true) {
        if (!generated) {
            Debug.Log("This chunk has not been generated and its mesh can not be created!");
            return;
        }

        this.squareMeshObject = squareMeshObject;
        SetMeshHeights();

        int LOD = WorldDataGenerator.instance.LODlevels-1;
        // TerrainGenerator.Instance.GenerateChunkTextures(squareMeshObject, LOD: LOD);

        squareMeshObject.SetLOD(LOD);

        if (generateMeshCollider) {
            squareMeshObject.AddMeshCollider();
        }

    }

    
    public float[,] CreateHeightArray() {
        int s = Mathf.RoundToInt(Mathf.Sqrt(chunkDataArray.heightArray.Length));
        float[,] heightArray = new float[s, s];

        for (int i = 0; i < s; i++) {
            for (int j = 0; j < s; j++) {
                int index = chunkDataArray.GetIndex(i, j, s);
                heightArray[i, j] = chunkDataArray.heightArray[index];
            }
        }

        return heightArray;
    }

    public void SetMeshHeights() {
        squareMeshObject.SetMeshHeights(CreateHeightArray(), chunkDataArray.currentLOD, true);
    }

    public void UpscaleLOD(bool updateMesh = false, bool needToUpscaleDataArray = true) {
        if (needToUpscaleDataArray) chunkDataArray.UpscaleLOD();
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
    public int maxSize;
    public int maxLODs;
    public int currentLOD;
    public Vector2Int chunkPosition;

    public NativeArray<float> inlandnessArray;
    public NativeArray<float> humidityArray;
    public NativeArray<float> heatArray;
    public NativeArray<float> heightArray;

public ChunkDataArray(int size, int maxLODs, Vector2Int chunkPosition)
{
    this.maxSize = size;
    this.maxLODs = maxLODs;
    this.chunkPosition = chunkPosition;
    currentLOD = maxLODs - 1;

    int s = currentLOD == 0 ? maxSize : (int)(maxSize / Mathf.Pow(2, currentLOD)) + 1;

    inlandnessArray = new NativeArray<float>(s * s, Allocator.Persistent);
    humidityArray = new NativeArray<float>(s * s, Allocator.Persistent);
    heatArray = new NativeArray<float>(s * s, Allocator.Persistent);
    heightArray = new NativeArray<float>(s * s, Allocator.Persistent);
}

    public void Dispose()
    {
        if (inlandnessArray.IsCreated) inlandnessArray.Dispose();
        if (humidityArray.IsCreated) humidityArray.Dispose();
        if (heatArray.IsCreated) heatArray.Dispose();
        if (heightArray.IsCreated) heightArray.Dispose();
    }

    // Add a function to get the index in the flattened array
    public int GetIndex(int x, int y, int size) {
        return x + y * size;
    }

    public int GetArraySizeLOD() {
        if (currentLOD == 0) return maxSize;
        return (int)(maxSize / Mathf.Pow(2, currentLOD)) + 1;
    }

    public void SetValues(bool upscaling = false) {
        int s = GetArraySizeLOD();
        Vector2 origin = WorldDataGenerator.instance.GetChunkWorldPostion(chunkPosition, offset: false);
        float quadSize = WorldDataGenerator.instance.quadSize;
        float LODmultiplier = Mathf.Pow(2, currentLOD); // Adjust for LOD

        for (int i = 0; i < s; i++) {
            int js = s; // Default sampling rate

            if (upscaling && i % 2 == 0) {
                js *= 2; // Double the sampling rate for every other row when upscaling
            }

            for (int j = 0; j < js; j++) {
                Vector2 pos = new Vector2(i, j / (upscaling && i % 2 == 0 ? 2.0f : 1.0f)) * quadSize * LODmultiplier + origin;

                int xIndex = i;
                int yIndex = j;
                if (upscaling && i % 2 == 0) {
                    yIndex = j / 2;
                }

                int index = GetIndex(xIndex, yIndex, s);

                float inlandness = TerrainGenerator.Instance.GetInlandness(pos);
                float humidity = TerrainGenerator.Instance.GetHumidity(pos);
                float heat = TerrainGenerator.Instance.GetHeat(pos);
                float height = TerrainGenerator.Instance.GetHeight(pos, inlandness, humidity, heat);

                inlandnessArray[index] = inlandness;
                humidityArray[index] = humidity;
                heatArray[index] = heat;
                heightArray[index] = height;
            }
        }
    }

    public void UpscaleLOD() {
        if (currentLOD > 0) {
            currentLOD--;

            int newSize = GetArraySizeLOD();
            NativeArray<float> newInlandnessArray = new NativeArray<float>(newSize * newSize, Allocator.Persistent);
            NativeArray<float> newHumidityArray = new NativeArray<float>(newSize * newSize, Allocator.Persistent);
            NativeArray<float> newHeatArray = new NativeArray<float>(newSize * newSize, Allocator.Persistent);
            NativeArray<float> newHeightArray = new NativeArray<float>(newSize * newSize, Allocator.Persistent);

            int oldSize = newSize / 2 + 1;

            for (int i = 0; i < oldSize; i++) {
                for (int j = 0; j < oldSize; j++) {
                    int oldIndex = GetIndex(i, j, oldSize);
                    int newIndex1 = GetIndex(i * 2, j * 2, newSize);
                    int newIndex2 = GetIndex(i * 2 + 1, j * 2, newSize);
                    int newIndex3 = GetIndex(i * 2, j * 2 + 1, newSize);
                    int newIndex4 = GetIndex(i * 2 + 1, j * 2 + 1, newSize);

                    float oldInlandness = inlandnessArray[oldIndex];
                    float oldHumidity = humidityArray[oldIndex];
                    float oldHeat = heatArray[oldIndex];
                    float oldHeight = heightArray[oldIndex];

                    newInlandnessArray[newIndex1] = oldInlandness;
                    newHumidityArray[newIndex1] = oldHumidity;
                    newHeatArray[newIndex1] = oldHeat;
                    newHeightArray[newIndex1] = oldHeight;

                    newInlandnessArray[newIndex2] = oldInlandness;
                    newHumidityArray[newIndex2] = oldHumidity;
                    newHeatArray[newIndex2] = oldHeat;
                    newHeightArray[newIndex2] = oldHeight;

                    newInlandnessArray[newIndex3] = oldInlandness;
                    newHumidityArray[newIndex3] = oldHumidity;
                    newHeatArray[newIndex3] = oldHeat;
                    newHeightArray[newIndex3] = oldHeight;

                    newInlandnessArray[newIndex4] = oldInlandness;
                    newHumidityArray[newIndex4] = oldHumidity;
                    newHeatArray[newIndex4] = oldHeat;
                    newHeightArray[newIndex4] = oldHeight;
                }
            }

            inlandnessArray.Dispose();
            humidityArray.Dispose();
            heatArray.Dispose();
            heightArray.Dispose();

            inlandnessArray = newInlandnessArray;
            humidityArray = newHumidityArray;
            heatArray = newHeatArray;
            heightArray = newHeightArray;

            SetValues(upscaling: true);
        }
    }
}


[BurstCompile]
public struct GenerateChunkJob : IJob
{
    public ChunkDataArray chunkDataArray;
    public void Execute()
    {
        chunkDataArray.SetValues();
    }
}

[BurstCompile]
public struct UpscaleLODJob : IJob
{
    public ChunkDataArray chunkDataArray;
    public void Execute()
    {
        chunkDataArray.UpscaleLOD();
    }
}