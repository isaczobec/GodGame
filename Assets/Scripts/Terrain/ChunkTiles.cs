using UnityEngine;

public class ChunkTiles {

    public int sideLength;

    public Vector2Int chunkPos;

    public float[,] heightMap;
    public float[,] inlandnessMap;
    public float[,] humididtyMap;
    public float[,] heatMap;

    public TerrainObject[,] terrainObjects;


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
        return new Vector3(tileWorldPos.x, heightMap[x,y], tileWorldPos.y);
    }

}