using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcManager : MonoBehaviour
{

    [SerializeField] private GameObject npcSelectedDecalPrefab;
    [SerializeField] private GameObject npcTargettedDecalPrefab;

    [SerializeField] private GameObject npcSelectedHitBoxPrefab;

    public static NpcManager instance;

    public List<NPC> npcs = new List<NPC>(); // for debbuyding?


    public event EventHandler<NPC> OnNPCSpawned;

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
        NPCPathfinding.bigPathFinding = new BigPathFinding();
        NPCPathfinding.angledPathfinding = new AngledPathfinding();
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
    public NPC SpawnNPC(NPCSO npcso, ChunkTile chunkTile, bool isOwnedByPlayer) {

        if (chunkTile.terrainObject != null) {
            return null; // cant spawn an NPC on a terrain object
        }

        GameObject gameObject = new GameObject();
        gameObject.transform.parent = transform;
        NPC spawnedNPC = gameObject.AddComponent<NPC>();

        // set the basestats and npcso
        spawnedNPC.baseStats = npcso.npcBaseStats;
        spawnedNPC.nPCSO = npcso;

        // init the npcStats
        spawnedNPC.npcStats = new NPCStats(spawnedNPC.baseStats);

        spawnedNPC.isOwnedByPlayer = isOwnedByPlayer;


        GameObject npcGameObject = Instantiate(npcso.prefab, Vector3.zero, Quaternion.identity, gameObject.transform); // spawn the NPC prefab

        // add the Animation actions to the npc
        NPCAnimationActionList nPCAnimationActionList = npcGameObject.GetComponent<NPCAnimationActionList>();
        nPCAnimationActionList.SetupActions(spawnedNPC);

        // init the npcBehaviour. this is perhaps temporary, not sure if this is the best way to do it
        spawnedNPC.npcBehaviour = NPCBehavioursList.GetNPCbehaviour(npcso.npcBehaviourType);
        spawnedNPC.npcBehaviour.Setup(spawnedNPC);
        spawnedNPC.npcBehaviour.SetNPCAnimationActionList(nPCAnimationActionList);



        // set the chunkTile and coordinates of the NPC
        spawnedNPC.SetChunkTileAndCoordinates(chunkTile);

        // init the npc
        spawnedNPC.Initialize();

        // instantiate and initialize the NPC selected decal
        GameObject selectedDecalObject = Instantiate(npcSelectedDecalPrefab, Vector3.zero, Quaternion.identity, gameObject.transform); // spawn the selected decal and parent it to the npc
        selectedDecalObject.transform.localPosition = new Vector3(0, 0.1f, 0);
        NPCSelctedDecal selectedDecal = selectedDecalObject.GetComponent<NPCSelctedDecal>();
        selectedDecal.SetNPCandSetup(spawnedNPC);

        GameObject targettedDecalObject = Instantiate(npcTargettedDecalPrefab, Vector3.zero, Quaternion.identity, gameObject.transform); // spawn the targetted decal and parent it to the npc
        targettedDecalObject.transform.localPosition = new Vector3(0, 0.1f, 0);
        NPCTargettedDecal targettedDecal = targettedDecalObject.GetComponent<NPCTargettedDecal>();
        targettedDecal.SetNPCandSetup(spawnedNPC);

        // get and assign the NPCvisual
        NPCvisual npcVisual = npcGameObject.GetComponent<NPCvisual>();
        npcVisual.Setup(spawnedNPC, selectedDecal, targettedDecal);
        spawnedNPC.SetNpcVisual(npcVisual);

        // add the hitboxes of the npc
        GameObject hitBoxObject = Instantiate(npcSelectedHitBoxPrefab, Vector3.zero, Quaternion.identity, npcGameObject.transform); // spawn the selection hitbox and parent it to the npc
        hitBoxObject.transform.localPosition = new Vector3(0, 0, 0);
        Collider selectionCollider = hitBoxObject.GetComponent<Collider>();
        hitBoxObject.transform.localScale = npcso.selectedHitBoxDimensions; // change the scale of the hitbox
        // get and set the npc of the hitbox
        NPCSelectionHitBox hitBox = hitBoxObject.GetComponent<NPCSelectionHitBox>();
        hitBox.SetNPC(spawnedNPC);



        WorldDataGenerator.instance.AddRenderAround(spawnedNPC);

        
        chunkTile.chunkTiles.chunk.npcs.Add(spawnedNPC);


        npcs.Add(spawnedNPC);

        OnNPCSpawned?.Invoke(this, spawnedNPC);

        return spawnedNPC;
        
    }

}
