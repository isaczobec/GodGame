
using System;
using UnityEngine;

[Serializable]
public class TerrainObject {

    public string name;

    [Header("Visuals")]
    public GameObject[] prefabs; // prefabs that can be spawned
    public float scale = 1f; // the scale of the object
    public bool randomRotation = true;
    public bool randomScale = true;
    public float scaleRandomNess = 0.1f; // the maximum random scale deviation from the original scale


    [Header("Size")]
    public int xSize;   // size of the object in x direction
    public int ySize;   // size of the object in y direction


    [Header("Spawn Parameters")]
    public float spawnWeight; // the "weight/probability" of tring to spawn this object
    public float chanceToSpawn = 1f; // probability of spawning this object if it was selected from its wheight
    public float biomeThreshold = 0.9f; // the threshold for the biome value required to spawn this object
    public float steepnessLimit = 1f; // the maximum steepness of the terrain where this object can be spawned

    [Header("Spawn Clusters")]

    public int maxClusteredObjects = 0; // the maximum number of objects that can be spawned in a cluster with this object
    public ClusterableTerrainObject[] clusterableTerrainObjects; // objects that can be spawned within a cluster with this object
    


    
    [HideInInspector]
    public GameObject createdObject;
}


[Serializable]
public class ClusterableTerrainObject {

    public TerrainObjectSO terrainObjectSO;

    public float chanceToCluster = 0.7f; // the chance that this object will be spawned within the cluster

    public int clusterDistanceTiles; // the distance in tiles from the center of the cluster where this object can be spawned

}