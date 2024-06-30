using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcManager : MonoBehaviour
{

    [SerializeField] private GameObject npcSelectedDecalPrefab;

    public static NpcManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }

        NPCPathfinding.instance = new NPCPathfinding(); // initialize the NPC pathfinding class
    }



    // Update is called once per frame
    void Update()
    {
        
    }


    /// <summary>
    /// Spawns and returns a new NPC at the given chunkTile. Returns null if it couldnt be spawned.
    /// </summary>
    /// <param name="npc"></param>
    /// <param name="chunkTile"></param>
    public NPC SpawnNPC(NPCSO npcso, ChunkTile chunkTile) {

        if (chunkTile.terrainObject != null) {
            return null; // cant spawn an NPC on a terrain object
        }

        GameObject gameObject = new GameObject();
        gameObject.transform.parent = transform;
        NPC spawnedNPC = gameObject.AddComponent<NPC>();

        spawnedNPC.stats = new NPCstats(npcso.npcStats); // create a copy of the npc stats

        GameObject npcGameObject = Instantiate(npcso.prefab, Vector3.zero, Quaternion.identity, gameObject.transform); // spawn the NPC prefab


        spawnedNPC.SetChunkTileAndCoordinates(chunkTile);

        spawnedNPC.Initialize();

        // instantiate and initialize the NPC selected decal
        GameObject decalObject = Instantiate(npcSelectedDecalPrefab, Vector3.zero, Quaternion.identity, gameObject.transform); // spawn the selected decal and parent it to the npc
        decalObject.transform.localPosition = new Vector3(0, 0.1f, 0);
        NPCSelctedDecal decal = decalObject.GetComponent<NPCSelctedDecal>();
        decal.SetNPC(spawnedNPC);


        WorldDataGenerator.instance.AddRenderAround(spawnedNPC);

        return spawnedNPC;
        
    }
}
