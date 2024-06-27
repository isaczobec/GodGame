using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;



public class NPC : MonoBehaviour, IRenderAround // Irenderaround is an interface that is used to know were chunks should be rendered around the NPC
{


    /// <summary>
    /// A class containig all the stats of this npc.
    /// </summary>
    public NPCstats stats;


    
    // position and chunktile

    /// <summary>
    /// The coordinates of the NPC in the world.
    /// </summary>
    [HideInInspector] public Vector2Int coordinates { get; private set; }
    /// <summary>
    /// The chunkTile the NPC is currently standing on.
    /// </summary>
    [HideInInspector] public ChunkTile chunkTile { get; private set; }


    // movement
    public bool currentlyMoving {get; private set;}
    /// <summary>
    /// The coroutine that moves the npc to an adjacent tile.
    /// </summary>
    private Coroutine movementCoroutine;
    /// <summary>
    /// Event that is called when the NPC has finished moving to a new tile.
    /// </summary>
    public event EventHandler<ChunkTile> OnMovementFinished;



    // ------- INITIALIZATION --------



    /// <summary>
    /// Sets the chunkTile the NPC is standing on and calculates the grid coordinates of the NPC. Also sets the npc of the chunkTile to this NPC.
    /// </summary>
    public void SetChunkTileAndCoordinates(ChunkTile chunkTile) {
        this.chunkTile = chunkTile;
        chunkTile.npc = this;
        coordinates = chunkTile.posInChunk + chunkTile.chunkTiles.chunkPos * chunkTile.chunkTiles.sideLength; // calculate the tile grid coordinates of the npc 
    }


    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    private void MoveToRandomTile() {
        Vector2Int pos = new Vector2Int(1, 1);
        MoveToAdjacentTile(pos);
    }

    public void Initialize() {
        SetWorldPosition();
        MoveToRandomTile();
        OnMovementFinished += (sender, tile) => MoveToRandomTile();
    }


    /// <summary>
    /// Sets the world position of the NPC to the world position of the chunkTile it is standing on.
    /// </summary>
    private void SetWorldPosition() {
        transform.position = chunkTile.chunkTiles.GetTileWorldPosition(chunkTile.posInChunk.x, chunkTile.posInChunk.y);
    }





    // --------- MOVEMENT --------


    /// <summary>
    /// Moves the NPC to the given relative position. The relative position is the position of the tile relative to the current tile of the NPC. 
    /// Throws an error if the relative position is not adjacent to the current tile.
    /// Starts a coroutine of movement That updates this NPC's chunkTile once finnished.
    /// </summary>
    /// <param name="relativePosition"></param>
    private void MoveToAdjacentTile(Vector2Int relativePosition) {
        if (Mathf.Abs(relativePosition.x) > 1 || Mathf.Abs(relativePosition.y) > 1) {
            Debug.LogError("Can only move to adjacent tiles");
            return;
        }

        if (currentlyMoving) return;

        ChunkTile tileToMoveTo = chunkTile.GetChunkTileFromRelativePosition(relativePosition);
        if (tileToMoveTo.terrainObject != null || tileToMoveTo.npc != null) { // check if the tile is occupied
            return; // cant move to a tile with a terrain object
        }

        // change the npc of the tiles
        chunkTile.npc = null;
        tileToMoveTo.npc = this;
        chunkTile = tileToMoveTo;

        // start the coroutine
        if (movementCoroutine != null) {
            StopCoroutine(movementCoroutine);
        }
        movementCoroutine = StartCoroutine(MoveToTileCoroutine(tileToMoveTo));

    }

    private IEnumerator MoveToTileCoroutine(ChunkTile tileToMoveTo) {
        Vector3 worldPos1 = transform.position;
        Vector3 worldPos2 = tileToMoveTo.chunkTiles.GetTileWorldPosition(tileToMoveTo.posInChunk.x, tileToMoveTo.posInChunk.y);

        float tileDistance = Vector3.Distance(worldPos1, worldPos2) / WorldDataGenerator.instance.tileSize; // distance in tiles
        float movementTime = tileDistance / stats.movementSpeed; // time in seconds

        currentlyMoving = true;
        float passedTime = 0f;

        while (passedTime < movementTime) {
            transform.position = Vector3.Lerp(worldPos1, worldPos2, passedTime / movementTime);
            passedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = worldPos2;

        currentlyMoving = false; // reset the moving flag

        OnMovementFinished?.Invoke(this, tileToMoveTo);

    }





    // ------ IMPLEMENTATION OF IRenderAround INTERFACE -------

    // return the position of the NPC. Used to know were chunks should be rendered around the NPC. used in the irenderaround interface
    public Vector2 getCenterPosition()
    {
        return new Vector2(transform.position.x, transform.position.z);
    }

    // returns how many chunks should be rendered around the NPC. used in the irenderaround interface
    public int getRenderDistanceChunks()
    {
        return stats.renderDistance;
    }

}
