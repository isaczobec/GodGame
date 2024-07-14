
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using TMPro;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.Profiling;

public class Chunk {


    public Vector2Int chunkPosition {get; private set;}

    public ChunkDataArray chunkDataArray;
    public SquareMeshObject squareMeshObject;

    public bool generated = false;
    public bool discovered = false;
    public bool generationPrompted = false;

    public ChunkTiles tiles; // a class containing arrays of information about all the tiles in a chunk, used for ie pathfinding and other gameplay mechanics

    /// <summary>
    /// The node in the chunk tree that this chunk belongs to
    /// </summary>
    public ChunkTreeNode chunkTreeNode;
    


    /// <summary>
    /// A list of all the terrain objects that exist in this chunk.
    /// </summary>
    public List<TerrainObject> terrainObjects = new List<TerrainObject>();

    /// <summary>
    /// A list of all the NPCs that are currently in this chunk.
    /// </summary>
    public List<NPC> npcs = new List<NPC>();
    

    public bool visible {
    set {
            squareMeshObject?.SetVisibilityMultiplier(value? 1f : 0f);
        } 
    } 


    public Chunk(Vector2Int chunkPosition, SquareMeshObject squareMeshObject = null, bool visible = false, ChunkTreeNode chunkTreeNode = null) {
        this.chunkPosition = chunkPosition;

        chunkDataArray = new ChunkDataArray(WorldDataGenerator.instance.maxChunkSize, WorldDataGenerator.instance.LODlevels, chunkPosition: chunkPosition);
        this.squareMeshObject = squareMeshObject;

        this.visible = visible;

        WorldDataGenerator.instance.OnBeforeDestroy += Dispose;

        this.chunkTreeNode = chunkTreeNode;

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

    public GenerateChunkJobParallel GetGenerateChunkJobParallel(bool upscale = false) {
        GenerateChunkJobParallel job = new GenerateChunkJobParallel(chunkDataArray, upscale: upscale);
        return job;
    }

    public UpscaleLODJob GetUpscaleLODJob() {
        UpscaleLODJob job = new UpscaleLODJob {
            chunkDataArray = chunkDataArray
        };

        return job;
    }

    public SetAllTexturesJob GetSetAllTexturesJob(int textureSize) {

        SetAllTexturesJob job = new SetAllTexturesJob (
            textureHeight: textureSize,
            textureWidth: textureSize,
            sampleXFrom: squareMeshObject.squareMesh.vertices[0].x,
            sampleZFrom: squareMeshObject.squareMesh.vertices[0].z,
            quadSize: squareMeshObject.squareMesh.quadSize,
            sizeX: squareMeshObject.squareMesh.sizeX,
            sizeZ: squareMeshObject.squareMesh.sizeZ,
            inlandnessHeightMultiplier: TerrainGenerator.Instance.inlandnessHeightMultiplier
        );

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
        TerrainGenerator.Instance.GenerateChunkTextures(squareMeshObject, LOD: LOD);

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
        if (needToUpscaleDataArray) {
            chunkDataArray.UpscaleArraysLOD();
            chunkDataArray.SetValues(upscaling: true);
        } 
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

    private void Dispose(object sender, EventArgs e) {
        chunkDataArray.Dispose();
    }

    public Chunk GetRelativeChunk(Vector2Int direction) {
    // Calculate the target position by adding the direction to the current chunk position
    Vector2Int targetPosition = chunkPosition + direction;

    // Start at the current node
    ChunkTreeNode currentNode = chunkTreeNode;

    // Traverse up the tree until we find a node that contains the target position
    while (currentNode != null) {
        if (currentNode.topLeftPosition.x <= targetPosition.x && currentNode.bottomRightPosition.x > targetPosition.x &&
            currentNode.topLeftPosition.y <= targetPosition.y && currentNode.bottomRightPosition.y > targetPosition.y) {
            break;
        }
        currentNode = currentNode.parent;
    }

    // If we didn't find such a node, return null (this shouldn't happen if the tree is well-formed)
    if (currentNode == null) {
        return null;
    }

    // Traverse back down the tree to find the chunk at the target position
    while (currentNode.level > 1) {
        currentNode = currentNode.GetNodeFromPosition(targetPosition);
        if (currentNode == null) {
            return null;
        }
    }

    // If the chunk doesn't exist, return null
    if (currentNode.chunk == null) {
        return null;
    }

    // Return the chunk
    return currentNode.chunk;
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

    if (upscaling) {
        // Only calculate for new points introduced by upscaling
        for (int i = 0; i < s; i++) {
            for (int j = 0; j < s; j++) {
                if (i % 2 == 1 || j % 2 == 1) { // New points condition
                    Vector2 pos = new Vector2(i, j) * quadSize * LODmultiplier + origin;
                    int index = GetIndex(i, j, s);

                    // Recalculate values for new points
                    inlandnessArray[index] = TerrainGenerator.Instance.GetInlandness(pos);
                    humidityArray[index] = TerrainGenerator.Instance.GetHumidity(pos);
                    heatArray[index] = TerrainGenerator.Instance.GetHeat(pos);
                    heightArray[index] = TerrainGenerator.Instance.GetHeight(pos, inlandnessArray[index], humidityArray[index], heatArray[index]);
                }
                // Existing points are already copied in UpscaleArraysLOD method
            }
        }
    } else {
        // Original full calculation for non-upscaling scenario
        for (int i = 0; i < s; i++) {
            for (int j = 0; j < s; j++) {
                Vector2 pos = new Vector2(i, j) * quadSize * LODmultiplier + origin;
                int index = GetIndex(i, j, s);

                inlandnessArray[index] = TerrainGenerator.Instance.GetInlandness(pos);
                humidityArray[index] = TerrainGenerator.Instance.GetHumidity(pos);
                heatArray[index] = TerrainGenerator.Instance.GetHeat(pos);
                heightArray[index] = TerrainGenerator.Instance.GetHeight(pos, inlandnessArray[index], humidityArray[index], heatArray[index]);
            }
        }
    }
}


        public void SetSingleValue(int index, int size, Vector2 origin, float quadSize, float LODmultiplier, bool upscaling = false) {

        if (upscaling) {
            // Only calculate for new points introduced by upscaling

            int i = index % size;
            int j = index / size;

            if (i % 2 == 1 || j % 2 == 1)
            { // New points condition
                Vector2 pos = new Vector2(i, j) * quadSize * LODmultiplier + origin;
                SetArrayValuesAtIndex(index, pos);
            }

        } else {

            int i = index % size;
            int j = index / size;

            // Original full calculation for non-upscaling scenario
            Vector2 pos = new Vector2(i, j) * quadSize * LODmultiplier + origin;

            SetArrayValuesAtIndex(index,pos);


        }
    }

    private void SetArrayValuesAtIndex(int index, Vector2 pos)
    {

        // Recalculate values for new points
        inlandnessArray[index] = TerrainGenerator.Instance.GetInlandness(pos);
        humidityArray[index] = TerrainGenerator.Instance.GetHumidity(pos);
        heatArray[index] = TerrainGenerator.Instance.GetHeat(pos);
        heightArray[index] = TerrainGenerator.Instance.GetHeight(pos, inlandnessArray[index], humidityArray[index], heatArray[index]);
    }



    /// <summary>
    /// upscales the arrays and copies over the values from the previous LOD. Does NOT generate new values. To do this, call SetValues(upscaling: true) after upscaling.
    /// </summary>
    public void UpscaleArraysLOD() {
    if (currentLOD > 0) {
        currentLOD--;

        int newSize = GetArraySizeLOD();
        NativeArray<float> newInlandnessArray = new NativeArray<float>(newSize * newSize, Allocator.Persistent);
        NativeArray<float> newHumidityArray = new NativeArray<float>(newSize * newSize, Allocator.Persistent);
        NativeArray<float> newHeatArray = new NativeArray<float>(newSize * newSize, Allocator.Persistent);
        NativeArray<float> newHeightArray = new NativeArray<float>(newSize * newSize, Allocator.Persistent);

        int oldSize = newSize / 2 + 1;

        // Copy old values and leave gaps for new values
        for (int i = 0; i < oldSize; i++) {
            for (int j = 0; j < oldSize; j++) {
                int oldIndex = GetIndex(i, j, oldSize);
                int newIndex = GetIndex(i * 2, j * 2, newSize);

                float oldInlandness = inlandnessArray[oldIndex];
                float oldHumidity = humidityArray[oldIndex];
                float oldHeat = heatArray[oldIndex];
                float oldHeight = heightArray[oldIndex];

                newInlandnessArray[newIndex] = oldInlandness;
                newHumidityArray[newIndex] = oldHumidity;
                newHeatArray[newIndex] = oldHeat;
                newHeightArray[newIndex] = oldHeight;

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
    }
}

    public ChunkTiles CreateChunkTiles() {

        int s = GetArraySizeLOD() - 1; // dont need the edge points, they are contained in adjacent chunks

        ChunkTiles tiles = new ChunkTiles {
            sideLength = s,
            chunkPos = chunkPosition,
            tiles = new ChunkTile[s, s]
        };


        Vector2Int coordinatesOrigin = chunkPosition * s;

        for (int i = 0; i < s; i++) {
            for (int j = 0; j < s; j++) {
                int index = GetIndex(i, j, s + 1);
                ChunkTile tile = new ChunkTile {
                    height = heightArray[index],
                    inlandness = inlandnessArray[index],
                    humidity = humidityArray[index],
                    heat = heatArray[index],
                    posInChunk = new Vector2Int(i, j),
                    chunkTiles = tiles,
                    coordinates = coordinatesOrigin + new Vector2Int(i, j)
                };
                tiles.tiles[i, j] = tile;
            }
        }

        return tiles;
    
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

public struct UpscaleLODJob : IJob
{
    public ChunkDataArray chunkDataArray;
    public void Execute()
    {
        chunkDataArray.SetValues(upscaling:true);
    }
}

public struct GenerateChunkJobParallel : IJobParallelFor {

    public ChunkDataArray chunkDataArray;

    public int size;
    public Vector2 origin;
    public float quadSize;
    public float LODmultiplier;
    public bool upscaling;

    public GenerateChunkJobParallel(ChunkDataArray chunkDataArray, bool upscale = false) {
        this.chunkDataArray = chunkDataArray;
        size = chunkDataArray.GetArraySizeLOD();
        origin = WorldDataGenerator.instance.GetChunkWorldPostion(chunkDataArray.chunkPosition, offset: false);
        quadSize = WorldDataGenerator.instance.quadSize;
        LODmultiplier = Mathf.Pow(2, chunkDataArray.currentLOD); // Adjust for LOD
        upscaling = upscale;
    }

    public void Execute(int index) {

        chunkDataArray.SetSingleValue(index, size, origin, quadSize, LODmultiplier, upscaling: upscaling);
    }

    public int GetJobLength() {
        return size * size;
    }

}