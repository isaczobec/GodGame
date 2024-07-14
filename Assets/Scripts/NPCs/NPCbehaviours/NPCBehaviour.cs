
using System;
using UnityEngine;

public class NPCBehaviour {

    public NPC npc;

    public float npcUpdateInterval = 0.3f;
    public float npcUpdateTimer = 0f;

    public void Setup(NPC npc) {
        this.npc = npc;
        npc.OnMovementFinished += OnMovedToNewTile;
        npc.OnMovementFinished += OnReachedMovementTarget;
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

}