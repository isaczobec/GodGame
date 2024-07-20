
using System;
using System.Collections.Generic;
using UnityEngine;

public class NPCBehaviour {

    public NPC npc;

    public float npcUpdateInterval = 1f;
    public float npcUpdateTimer = 0f;


    /// <summary>
    /// List of animation actions that the npc can perform.
    /// </summary>
    public NPCAnimationActionList npcAnimationActionList {get; private set;}
    public void SetNPCAnimationActionList(NPCAnimationActionList list) {
        npcAnimationActionList = list;
    }
    
    public void Setup(NPC npc) {
        this.npc = npc;
        npc.OnMovementFinished += OnMovedToNewTile;
        npc.OnMovementFinished += OnReachedMovementTarget;

        npcUpdateTimer += UnityEngine.Random.Range(0, npcUpdateInterval); // randomize the update timer to eliminate bottlenecks
    }


    /// <summary>
    /// Overridable method that is called when the npc moves to a new tile.
    /// </summary>
    public virtual void OnMovedToNewTile(object sender, ChunkTile e) {

    }

    /// <summary>
    /// Overridable method that is called when the npc reaches its movement target.
    /// </summary>
    public virtual void OnReachedMovementTarget(object sender, ChunkTile e) {

    }


    /// <summary>
    /// Overridable method that is called when the npc tick update is reached.
    /// </summary>
    public virtual void OnUpdateNPCTick() {

    }

    public void FrameUpdate() {
        npcUpdateTimer += Time.deltaTime;
        if (npcUpdateTimer >= npcUpdateInterval) {
            npcUpdateTimer = 0;
            OnUpdateNPCTick();
        }
    }

    /// <summary>
    /// Sets the update interval for the npc.
    /// </summary>
    /// <param name="interval"></param>
    public void SetUpdateInterval(float interval) {
        npcUpdateInterval = interval;
    }

}