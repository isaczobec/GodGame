using System;
using UnityEngine;

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

    public TerrainObject terrainObject;

    public Vector2Int posInChunk;

    public ChunkTiles chunkTiles;

    /// <summary>
    /// returns a ChunkTile at the given offset from this tile. Returns null if the tile has not been generated yet.
    /// </summary>
    /// <param name="offset"></param>
    /// <returns></returns>
    public ChunkTile GetChunkTileFromRelativePosition(Vector2Int offset) {

        Vector2Int newPos = posInChunk + offset;

        // check if we are still in the chunk
        if (newPos.x >= 0 || newPos.x <= chunkTiles.sideLength || newPos.y >= 0 || newPos.y <= chunkTiles.sideLength) {
            return chunkTiles.tiles[newPos.x, newPos.y];
        }

        int chunkXMove = offset.x > 0 ? 1 : -1;
        int chunkYMove = offset.y > 0 ? 1 : -1;
        Vector2Int chunkOffset = offset / chunkTiles.sideLength + new Vector2Int(chunkXMove, chunkYMove);

        Chunk chunk = WorldDataGenerator.instance.chunkTree.CreateOrGetChunk(chunkTiles.chunkPos + chunkOffset, allowCreation: false);
        if (chunk == null) return null;
        ChunkTiles tiles = chunk.chunkTiles;

        Vector2Int newTilePos = new Vector2Int(newPos.x % chunkTiles.sideLength, newPos.y % chunkTiles.sideLength);
        return tiles.tiles[newTilePos.x, newTilePos.y];
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

                float xLen = x * WorldDataGenerator.instance.quadSize;
                float yLen = y * WorldDataGenerator.instance.quadSize;
                float steepness = Mathf.Abs(tile.height - height) / Mathf.Sqrt(xLen * xLen + yLen * yLen);
                if (steepness > max) max = steepness;
            }
        }

        return max;
    }

}