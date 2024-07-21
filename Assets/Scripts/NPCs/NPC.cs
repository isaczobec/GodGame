using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;



public class NPC : MonoBehaviour, IRenderAround // Irenderaround is an interface that is used to know were chunks should be rendered around the NPC
{


    /// <summary>
    /// A class containig all the baseStats of this npc. Set from the NPCSO (is the same instance)
    /// </summary>
    public NPCBaseStats baseStats;

    public NPCStats npcStats;

    /// <summary>
    /// The current "AI" of the NPC. This class is responsible for the movement and behaviour of the NPC.
    /// </summary>
    public NPCBehaviour npcBehaviour;

    public NPCSO nPCSO; // the scriptable object that holds the stats and more of the npc

    public NPCvisual npcVisual {get; private set;} // the visuals of the npc
    public void SetNpcVisual(NPCvisual npcVisual) {
        this.npcVisual = npcVisual;
    }

    public bool isOwnedByPlayer = true; // if the npc is owned by the player


    // animation actions
    public bool isPerformingMovementRestrictingAction = false; // if the npc is currently performing an action that restricts movement
    public bool isPerformingAnimationAction {get; private set;} = false; // if the npc is currently performing an animation action
    public NPCAnimationAction currentAnimationAction; // the current animation action the npc is performing


    public bool isDead {get; private set;} = false;



    // position and chunktile

    /// <summary>
    /// The coordinates of the NPC in the world.
    /// </summary>
    [HideInInspector] public Vector2Int coordinates { get; private set; }
    /// <summary>
    /// The chunkTile the NPC is currently standing on.
    /// </summary>
    [HideInInspector] public ChunkTile chunkTile { get; private set; }

    private bool isTryingToMoveAroundNPC = false; // if the npc is trying to move around another npc
    private Vector2Int moveAroundNPCDestination; // the direction the npc is trying to move around another npc

    
    private Vector2 _currentForwardDirection = Vector2.right; // the direction the npc is currently facing
    public Vector2 currentForwardDirection {
        get {
            return _currentForwardDirection;
        }
        set {
            _currentForwardDirection = value;
            forwardDirectionUpdated?.Invoke(this, value);
        }
    }
    public event EventHandler<Vector2> forwardDirectionUpdated; // event that is called when the forward direction is updated


    // movement
    private bool _currentlyMoving = false;
    public bool currentlyMoving {
        get {
            return _currentlyMoving;
        }
        set {
            _currentlyMoving = value;
            currentlyMovingUpdated?.Invoke(this, value);
        }
    }
    public event EventHandler<bool> currentlyMovingUpdated;

    /// <summary>
    /// The coroutine that moves the npc to an adjacent tile.
    /// </summary>
    private Coroutine movementCoroutine;
    /// <summary>
    /// Event that is called when the NPC has finished moving to a new tile.
    /// </summary>
    public event EventHandler<ChunkTile> OnMovementFinished;

    /// <summary>
    /// The queue of tiles the NPC is currently moving to. The first tile in the list is the tile the NPC is currently moving to.
    /// </summary>
    public List<ChunkTile> movementQueue = new List<ChunkTile>();

    /// <summary>
    /// Event that is called when the NPC reaches its pathfinding goal.
    /// </summary>
    public event EventHandler<ChunkTile> OnMovementTargetReached;


    /// <summary>
    /// Called when the NPC takes damage. The first parameter is the sender of the event, the second is the amount of damage taken.
    /// </summary>
    public event EventHandler<HitInfo> OnDamageTaken;

    /// <summary>
    /// Called when the NPC deals damage. The first parameter is the sender of the event, the second is the amount of damage dealt.
    /// </summary>
    public event EventHandler<HitInfo> OnDamageDealt;



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
        npcBehaviour.FrameUpdate();

        if (isDead) Die();
    }


    private void MoveToRandomTile() {
        Vector2Int pos = new Vector2Int(1, 1);
        MoveToAdjacentTile(pos);
    }

    public void Initialize() {
        SetWorldPosition();

        // test movement, not final
        // SetMovementTarget(coordinates + new Vector2Int(2, -25));
        // MoveToNextTileInQueue();
        OnMovementFinished += (sender, tile) => OnMovementWasFinnished();
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
    private void MoveToAdjacentTile(Vector2Int relativePosition)
    {
        if (Mathf.Abs(relativePosition.x) > 1 || Mathf.Abs(relativePosition.y) > 1)
        {
            Debug.LogError("Can only move to adjacent tiles");
            return;
        }

        if (relativePosition == Vector2Int.zero) return; // cant move to the same tile

        if (currentlyMoving) return;

        ChunkTile tileToMoveTo = chunkTile.GetChunkTileFromRelativePosition(relativePosition);
        AdministerChunkTileMovement(tileToMoveTo);

    }

    /// <summary>
    /// Moves the NPC to the given chunkTile, given that that chunktile is adjacent to the one the npc is currently standing on.  
    /// Throws an error if that is not the case.
    /// Starts a coroutine of movement That updates this NPC's chunkTile once finnished.
    /// </summary>
    /// <param name="relativePosition"></param>
    private void MoveToAdjacentTile(ChunkTile chunkTile) {
        Vector2Int relativePosition = chunkTile.coordinates - this.chunkTile.coordinates;
        if (Mathf.Abs(relativePosition.x) > 1 || Mathf.Abs(relativePosition.y) > 1) {
            Debug.Log(relativePosition);
            Debug.LogError("Can only move to adjacent tiles");
            return;
        }

        if (relativePosition == Vector2Int.zero) {
            Debug.LogError("Can't move to the same tile");
            return; // cant move to the same tile
        } 
        AdministerChunkTileMovement(chunkTile);
    }

    private void AdministerChunkTileMovement(ChunkTile tileToMoveTo)
    {
        if (tileToMoveTo.terrainObject != null || tileToMoveTo.npc != null)
        { // check if the tile is occupied
            OnMovementWasBlocked(tileToMoveTo); // !!!!!!!!!!!!!!!!!! risk of infinite loop
            return; // cant move to a tile with a terrain object
        }

        if (isPerformingMovementRestrictingAction) return; // if the npc is currently performing an action that restricts movement

        Vector2 f = tileToMoveTo.coordinates - chunkTile.coordinates;
        currentForwardDirection = f.normalized; // update the forward direction of the npc

        if (tileToMoveTo.chunkTiles.chunk != chunkTile.chunkTiles.chunk)
        {
            // if the chunkTile is in a different chunk, update the chunks list of npcs
            chunkTile.chunkTiles.chunk.npcs.Remove(this);
            tileToMoveTo.chunkTiles.chunk.npcs.Add(this);
        }

        // change the npc of the tiles
        chunkTile.npc = null;
        tileToMoveTo.npc = this;
        chunkTile = tileToMoveTo;

        // update the coordinates
        coordinates = chunkTile.coordinates;

        // start the coroutine
        if (movementCoroutine != null)
        {
            StopCoroutine(movementCoroutine);
        }
        movementCoroutine = StartCoroutine(MoveToTileCoroutine(tileToMoveTo));
    }

    /// <summary>
    /// Called when the NPC tries to move to a tile but is blocked by an object or another NPC.
    /// </summary>
    private void OnMovementWasBlocked(ChunkTile blockingTile) {
        if (movementQueue.Count > 0) {
            // get the best closest tile to move to
            Vector2Int posDifference = blockingTile.coordinates - chunkTile.coordinates;

            List<Vector2Int> newPositions = new List<Vector2Int>();

            if (posDifference.x + 1 <= 1) newPositions.Add(posDifference + new Vector2Int(1,0));
            if (posDifference.x - 1 >= -1) newPositions.Add(posDifference + new Vector2Int(-1,0));
            if (posDifference.y + 1 <= 1) newPositions.Add(posDifference + new Vector2Int(0,1));
            if (posDifference.y - 1 >= -1) newPositions.Add(posDifference + new Vector2Int(0,-1));

            moveAroundNPCDestination = movementQueue[movementQueue.Count-1].coordinates;

            int bestIndex = 0;
            float bestDistance = Mathf.Infinity;
            int i = 0;
            foreach (Vector2Int pos in newPositions) {
                float distance = Vector2.Distance(chunkTile.coordinates + pos, moveAroundNPCDestination);
                if (distance < bestDistance) {
                    bestDistance = distance;
                    bestIndex = i;
                }
                i++;
            }

            ChunkTile adjacentTile = chunkTile.GetChunkTileFromRelativePosition(newPositions[bestIndex]);
            isTryingToMoveAroundNPC = true;
            AdministerChunkTileMovement(adjacentTile);
        }
    }

    /// <summary>
    /// Sets the movement target of the NPC to the given coordinates. The NPC will then automatically try to move to that position.
    /// The resulting path will be stored in the movementQueue list, which is then walked through by the NPC using the MoveToNextTileInQueue method.
    /// </summary>
    public void SetMovementTarget(Vector2Int destination, bool npcsAreObstacles = false) {
        // List<ChunkTile> path = NPCPathfinding.instance.NPCGetPathTo(chunkTile, this, destination, 1000);
        List<ChunkTile> path = NPCPathfinding.angledPathfinding.NPCGetPathAngledTo(this, destination, 45, 15, 80, npcsAreObstacles: npcsAreObstacles);

        if (path != null) {
            movementQueue = path;
        } else {
            Debug.LogError("No path found");
        }
        
    }

    private void OnMovementWasFinnished()
    {
        if (isTryingToMoveAroundNPC){
            isTryingToMoveAroundNPC = false;
            SetMovementTarget(moveAroundNPCDestination);
            MoveToNextTileInQueue();
        } else {
            MoveToNextTileInQueue();
        }
    }

    /// <summary>
    /// Moves the NPC to the next tile in the movement queue, that was calculated by SetMovementTarget.
    /// </summary>
    public void MoveToNextTileInQueue() {
        if (movementQueue.Count == 0) {
          OnMovementTargetReached?.Invoke(this, chunkTile);  
        return;
        }

        ChunkTile nextTile = movementQueue[0];
        movementQueue.RemoveAt(0);

        MoveToAdjacentTile(nextTile);
    }

    /// <summary>
    /// Coroutine that moves the NPC to a given adjacent tile. Started by the MoveToAdjacentTile method.
    /// </summary>
    /// <param name="tileToMoveTo"></param>
    /// <returns></returns>
    private IEnumerator MoveToTileCoroutine(ChunkTile tileToMoveTo) {
        Vector3 worldPos1 = transform.position;
        Vector3 worldPos2 = tileToMoveTo.chunkTiles.GetTileWorldPosition(tileToMoveTo.posInChunk.x, tileToMoveTo.posInChunk.y);

        float tileDistance = Vector3.Distance(worldPos1, worldPos2) / WorldDataGenerator.instance.tileSize; // distance in tiles
        float movementTime = tileDistance / baseStats.movementSpeed; // time in seconds

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



    // ----- GETTING VISIBLE OBJECTS -------

    public List<NPC> GetVisibleNPCs() {

        List<NPC> visibleNPCs = new List<NPC>();

        for (int i = - baseStats.renderDistance; i < baseStats.renderDistance; i++) {
            for (int j = -baseStats.renderDistance; j < baseStats.renderDistance; j++) {
                Chunk lookInChunk = chunkTile.chunkTiles.chunk.GetRelativeChunk(new Vector2Int(i, j));
                if (lookInChunk != null && lookInChunk.generated) {
                    foreach (NPC npc in lookInChunk.npcs) {
                        if (npc != this) {
                            visibleNPCs.Add(npc);
                        }
                    }
                }

            }
        }

        return visibleNPCs;
    }

    /// <summary>
    /// Gets the closest npc or returns null if no npc is visible.
    /// </summary>
    /// <returns></returns>
    public NPC GetClosestNPC(bool needsToBeEnemy = false, bool needsToBeFriendly = false) {

        NPC closestNPC = null;
        float closestDistance = Mathf.Infinity;

        float thisNPCsDistanceToChunkborder = Mathf.Min(chunkTile.chunkTiles.sideLength - chunkTile.posInChunk.x, chunkTile.posInChunk.x, chunkTile.chunkTiles.sideLength - chunkTile.posInChunk.y, chunkTile.posInChunk.y);

        int currentSearchRadius = 1;
        while (currentSearchRadius <= baseStats.renderDistance) {

            int iterations = currentSearchRadius * 2 - 1;
            float sureDistance = thisNPCsDistanceToChunkborder + (currentSearchRadius-1) * chunkTile.chunkTiles.sideLength;

            for (int i = 0; i < iterations; i++) {
                for (int j = 0; j < iterations; j++) {

                    // continue if we are not on the edge
                    if (i != 0 && i != currentSearchRadius*2-2 && j != 0 && j != currentSearchRadius*2-2) {
                        continue;
                    }

                    Chunk lookInChunk = chunkTile.chunkTiles.chunk.GetRelativeChunk(new Vector2Int(i - currentSearchRadius, j - currentSearchRadius));
                    if (lookInChunk != null && lookInChunk.generated) {
                        foreach (NPC npc in lookInChunk.npcs) {

                            // check if the npc is an enemy or friendly
                            if (needsToBeEnemy && this.isOwnedByPlayer == npc.isOwnedByPlayer) continue;
                            if (needsToBeFriendly && this.isOwnedByPlayer != npc.isOwnedByPlayer) continue;

                            
                            if (npc != this) {
                                float distance = Vector2.Distance(npc.coordinates, coordinates);
                                if (distance < closestDistance) {
                                    closestDistance = distance;
                                    closestNPC = npc;
                                    if (i == iterations-1 && j == iterations-1 && closestDistance < sureDistance) { // if the closest npc is on the edge of the search radius and the distance is less than the distance to the chunk border + the search radius
                                        return closestNPC;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            currentSearchRadius++;
        }


        return closestNPC;
    }





    // ------ ANIMATION ACTIONS -------


    /// <summary>
    /// Tries to begin an animation action. If the action can be performed, the npcVisual will start the animation and the movement restricting flag will be set.
    /// </summary>
    public void TryBeginAnimationAction(NPCAnimationAction action) {
        if (action.GetCanBePerformed()) {
            npcVisual.StartAnimationAction(action); // check if the action can be performed and then start the animation
            isPerformingMovementRestrictingAction = !action.allowMoving; // set the movement restricting flag
            isPerformingAnimationAction = true; // set the animation action flag
            currentAnimationAction = action; // set the current animation action
            action.OnActionStarted(); // call the action started "event method" of the action
        }
    }

    /// <summary>
    /// Called automatically when an animation action has ended. Resets the movement restricting flag and calls the OnActionEnded method of the action.
    /// </summary>
    public void AnimationActionEnded() {
        isPerformingAnimationAction = false;
        isPerformingMovementRestrictingAction = false;
        currentAnimationAction = null;
    }

    /// <summary>
    /// call this to end the animation action prematurely. This will call the EndAnimationActionPremautrely method of the npcVisual and then call the AnimationActionEnded method.
    /// </summary>
    public void TryEndAnimationActionPremautrely() {
        if (currentAnimationAction == null) return; // we dont have an action to end
        if (currentAnimationAction.GetCanBeEndedPrematurely()) {
            npcVisual.EndAnimationActionPremautrely();
            AnimationActionEnded();
        }
    }



    // ------ DOING AND TAKING DAMAGE ------

    public HitInfo TakeDamage(float damage) {
        float finalDamage = damage;
        bool died = false;

        npcStats.currentHealth -= finalDamage;

        if (npcStats.currentHealth <= 0) {
            died = true;
        }


        if (died) {
            isDead = true;
        }

        HitInfo hitInfo = new HitInfo{killed = died, finalDamage = finalDamage};
        OnDamageTaken?.Invoke(this, hitInfo);

        return hitInfo;
    }

    public HitInfo DealDamage(NPC target, float damage) {
        HitInfo info = target.TakeDamage(damage);
        OnDamageDealt?.Invoke(this, info);
        return info;
    }


    // --------- DEATH ---------

    /// <summary>
    /// Kills the NPC. Removes it from the chunkTile and the list of npcs in the chunk.
    /// </summary>
    public void Die() {
        chunkTile.npc = null;
        chunkTile.chunkTiles.chunk.npcs.Remove(this);
        NpcManager.instance.npcs.Remove(this);
        WorldDataGenerator.instance.RemoveRenderAround(this);
        npcBehaviour = null; // clear the behaviour
        Destroy(gameObject);
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
        return baseStats.renderDistance;
    }

}
