using System;
using UnityEngine;

/// <summary>
/// A class that holds all the tiles of a chunk. The tiles are stored in a 2D array.
/// </summary>
public class ChunkTiles {

    public int sideLength;

    public Vector2Int chunkPos;


    public ChunkTile[,] tiles;


    /// <summary>
    /// returns the world position of the tile at the given x and y position
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public Vector3 GetTileWorldPosition(int x, int y) {
        Vector2 chunkWorldPos = WorldDataGenerator.instance.GetChunkWorldPostion(chunkPos, offset: false);

        float tileSize = Mathf.Pow(2, WorldDataGenerator.instance.LODlevels-1) * WorldDataGenerator.instance.quadSize;
        Vector2 tileWorldPos = new Vector2(chunkWorldPos.x + x * tileSize, chunkWorldPos.y + y * tileSize);
        return new Vector3(tileWorldPos.x, tiles[x,y].height, tileWorldPos.y);
    }



}


public class ChunkTile {

    public float height;

    public float inlandness;

    public float humidity;

    public float heat;

    /// <summary>
    /// The terrain object that is currently standing on this tile. Null if no terrain object is standing on this tile.
    /// </summary>
    public TerrainObject terrainObject;

    /// <summary>
    /// The NPC that is currently standing on this tile. Null if no NPC is standing on this tile.
    /// </summary>
    public NPC npc;

    public Vector2Int posInChunk;
    public Vector2Int coordinates;

    public ChunkTiles chunkTiles;

    /// <summary>
    /// returns a ChunkTile at the given offset from this tile. Returns null if the tile has not been generated yet.
    /// </summary>
    /// <param name="offset"></param>
    /// <returns></returns>
    public ChunkTile GetChunkTileFromRelativePosition(Vector2Int offset) {
    Vector2Int newPos = posInChunk + offset;

    // Check if the new position is within the current chunk
    if (newPos.x >= 0 && newPos.x < chunkTiles.sideLength && newPos.y >= 0 && newPos.y < chunkTiles.sideLength) {
        return chunkTiles.tiles[newPos.x, newPos.y];
    }


    int chunkX = newPos.x < 0 ? -1 + newPos.x / chunkTiles.sideLength : newPos.x / chunkTiles.sideLength;
    int chunkY = newPos.y < 0 ? -1 + newPos.y / chunkTiles.sideLength : newPos.y / chunkTiles.sideLength;

    // Calculate the chunk offset based on the relative position
    Vector2Int chunkOffset = new Vector2Int(
        chunkX,
        chunkY
    );

    // Adjust the chunk position to get the correct chunk
    Vector2Int newChunkPos = chunkTiles.chunkPos + chunkOffset;

    // Get the chunk from the world data generator
    Chunk chunk = WorldDataGenerator.instance.chunkTree.CreateOrGetChunk(newChunkPos, allowCreation: false);
    if (chunk == null) return null; // Return null if the chunk does not exist

    // Calculate the position within the new chunk
    Vector2Int newTilePos = new Vector2Int(
        (newPos.x % chunkTiles.sideLength + chunkTiles.sideLength) % chunkTiles.sideLength,
        (newPos.y % chunkTiles.sideLength + chunkTiles.sideLength) % chunkTiles.sideLength
    );

    // Return the tile from the new chunk
    return chunk.tiles.tiles[newTilePos.x, newTilePos.y];
}

    /// <summary>
    /// returns the maximum steepness of the terrain around this tile. Ignores tiles that havent been generated yet.
    /// </summary>
    public float GetMaxSteepness() {
        float max = 0;

        for (int x = -1; x <= 1; x++) {
            for (int y = -1; y <= 1; y++) {
                if (x == 0 && y == 0) continue;

                ChunkTile tile = GetChunkTileFromRelativePosition(new Vector2Int(x, y));
                if (tile == null) continue;

                float xLen = x * WorldDataGenerator.instance.tileSize;
                float yLen = y * WorldDataGenerator.instance.tileSize;
                float steepness = Mathf.Abs(tile.height - height) / Mathf.Sqrt(xLen * xLen + yLen * yLen);
                if (steepness > max) max = steepness;
            }
        }

        return max;
    }

}