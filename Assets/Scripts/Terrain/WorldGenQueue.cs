
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenQueue {

    private class WorldGenQueueItem {

        public WorldGenQueueItem(Chunk chunk, bool generateMesh, int priority) {
            this.chunk = chunk;
            this.generateMesh = generateMesh;
            this.priority = priority;
        }

        public Chunk chunk;
        public bool generateMesh;
        public int priority;
    }

    private List<WorldGenQueueItem> chunkGenerationQueue = new List<WorldGenQueueItem>();

    public void EnqueueChunk(Chunk chunk, bool generateMesh, int priority = 0) {    

        WorldGenQueueItem worldGenQueueItem = new WorldGenQueueItem(chunk, generateMesh, priority);
        chunkGenerationQueue.Add(worldGenQueueItem);
    }


    private void GenerateQueuedChunks() {
        foreach (WorldGenQueueItem wgq in chunkGenerationQueue) {

            Chunk chunk = wgq.chunk;
            Vector2Int chunkCoordinates = chunk.chunkPosition;
            int maxChunkSize = WorldDataGenerator.instance.maxChunkSize;

            chunk.GenerateChunk();

            // generate mesh
            if (wgq.generateMesh)
            {
                Vector2 worldPosition = WorldDataGenerator.instance.GetChunkWorldPostion(chunk.chunkPosition); 
                SquareMeshObject sqr = MeshGenerator.instance.CreateSquareMeshGameObject(worldPosition, maxChunkSize, maxChunkSize, chunkCoordinates);
                chunk.GenerateMesh(sqr, generateMeshCollider: true);
            }
        }
        chunkGenerationQueue.Clear();
    }

    public void StartGenCoroutine() {
        MonoBehaviour mono = WorldDataGenerator.instance;
        mono.StartCoroutine(GenerateChunksCoroutine());
    }


    private IEnumerator GenerateChunksCoroutine() {
        while (true) {

            yield return new WaitForSeconds(1f);
            GenerateQueuedChunks();
        }
    }

}

