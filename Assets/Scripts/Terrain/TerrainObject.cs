
using System;
using UnityEngine;

[Serializable]
public class TerrainObject {

    public string name;
    public GameObject prefab;

    [Header("Size")]
    public int xSize;   // size of the object in x direction
    public int ySize;   // size of the object in y direction


    [Header("Spawn Parameters")]
    public float spawnWeight; // the "weight/probability" of tring to spawn this object
    public float chanceToSpawn = 1f; // probability of spawning this object if it was selected from its wheight
    public float biomeThreshold = 0.9f; // the threshold for the biome value required to spawn this object

    public bool randomRotation = true;
    public bool randomScale = true;
    public float scaleRandomNess = 0.1f; // the maximum random scale deviation from the original scale
}



