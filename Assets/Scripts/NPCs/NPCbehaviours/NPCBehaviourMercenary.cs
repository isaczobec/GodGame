
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum MercenaryBehaviourOption {
    passive,
    defend,
    autoAttack,
}

/// <summary>
/// NPC behaviour that mercenary (player controlled) NPCs will inherit from.
/// Contains universal methods and functionality for all mercenary NPCs.
/// </summary>
public class NPCBehaviourMercenary : NPCBehaviour
{
    public List<NPC> playerTargettedEnemyNPCs = new List<NPC>();
    public List<NPC> naturallyTargettedEnemyNPCs = new List<NPC>();
    public NPC friendlyNPCToFollow;
    public Vector2Int PlayerAssignedPositionToMoveTo;
    public bool currentlyMovingToPlayerAssignedPosition = false;
    public MercenaryBehaviourOption mercenaryBehaviourOption = MercenaryBehaviourOption.autoAttack;


    /// <summary>
    /// Overridable method that is called when the npc moves to a new tile. By default, prioritizes moving to the player assigned position.
    /// </summary>
    public virtual void OnMovementTargetSetByPlayer(Vector2Int position) {
        PlayerAssignedPositionToMoveTo = position;
        currentlyMovingToPlayerAssignedPosition = true;

        npc.TryEndAnimationActionPremautrely();

        // clear the targetted npcs if the player assigns a new position to move to
        ClearPlayerTargettedEnemyNPCs();
        ClearNaturallyTargettedEnemyNPCs();
        friendlyNPCToFollow = null;

        npc.SetMovementTarget(position);
        npc.MoveToNextTileInQueue();
    }

    public override void OnReachedMovementTarget(object sender, ChunkTile tile) {
        if (npc.isOwnedByPlayer)Debug.Log("poses: " + tile.coordinates + " " + PlayerAssignedPositionToMoveTo);
        if (tile.coordinates == PlayerAssignedPositionToMoveTo) {
            currentlyMovingToPlayerAssignedPosition = false;
        }
    }


    /// <summary>
    /// Should be called when this mercenary npc dies. Clears targetted npcs and friendly npcs.
    /// </summary>
    public void OnDie() {
        ClearPlayerTargettedEnemyNPCs();
        ClearNaturallyTargettedEnemyNPCs();
        friendlyNPCToFollow = null;
    }



    // Attack Targets

    /// <summary>
    /// Adds an enemy npc to the list of enemy npcs targetted by the player. These npcs are targetted by the mercenary npc when the player attacks them. Use GetFirstPlayerTargettedEnemyNPC() to get the first npc in the list.
    /// </summary>
    /// <param name="targetNPC"></param>
    public void AddPlayerAttackTarget(NPC targetNPC) {
        if (targetNPC != null) {
            currentlyMovingToPlayerAssignedPosition = false;
            playerTargettedEnemyNPCs.Add(targetNPC);
            OnAddAttackTargetGeneral(targetNPC, false);
            OnPlayerAddAttackTarget(targetNPC, targetNPC == playerTargettedEnemyNPCs[0]);
        }
    }

    /// <summary>
    /// Adds an enemy npc to the list of naturally targetted enemy npcs. These npcs are targetted by the mercenary npc on their own. Use GetFirstNaturallyTargettedEnemyNPC() to get the first npc in the list.
    /// </summary>
    /// <param name="targetNPC"></param>
    public void AddNaturallyTargettedEnemyNPC(NPC targetNPC) {
        if (targetNPC != null) {
            naturallyTargettedEnemyNPCs.Add(targetNPC);
            OnAddAttackTargetGeneral(targetNPC, true);
        }
    }

    public void OnAddAttackTargetGeneral(NPC addedTargetNPC, bool wasTargettedNaturally) {
        addedTargetNPC.npcVisual.OnNPCTargettedChanged(true, wasTargettedNaturally);
    }

    /// <summary>
    /// Overridable method that is called when the player adds an attack target to this mercenary npc. 
    /// </summary>
    /// <param name="npc"></param>
    public virtual void OnPlayerAddAttackTarget(NPC targetNPC, bool isFirstTarget) {
        if (isFirstTarget) { // if this is the first npc in the list, try cancelling the current animation action
            npc.TryEndAnimationActionPremautrely();
        }
    }

    /// <summary>
    /// Gets the closest npc the player has targetted. Returns null if no npcs are targetted.
    /// </summary>
    /// <returns></returns>
    public NPC GetClosestPlayerTargettedEnemyNPC() {
        if (playerTargettedEnemyNPCs.Count == 0) return null;

        NPC closestNPC = playerTargettedEnemyNPCs[0];
        float closestDistance = (npc.coordinates - closestNPC.coordinates).magnitude;

        foreach (NPC enemyNPC in playerTargettedEnemyNPCs) {
            float distance = (npc.coordinates - enemyNPC.coordinates).magnitude;
            if (distance < closestDistance) {
                closestDistance = distance;
                closestNPC = enemyNPC;
            }
        }

        return closestNPC;
    }

    /// <summary>
    /// Gets the first npc the player has targetted. Returns null if no npcs are targetted. Also removes dead npcs from the list. This method should be used instead of directly accessing the list.
    /// </summary>
    /// <returns></returns>
    public NPC GetFirstPlayerTargettedEnemyNPC() {
        if (playerTargettedEnemyNPCs.Count == 0) return null;

        while (playerTargettedEnemyNPCs[0] == null) {
            playerTargettedEnemyNPCs.RemoveAt(0);
            if (playerTargettedEnemyNPCs.Count == 0) return null;
        }
        return playerTargettedEnemyNPCs[0];   
    }

    /// <summary>
    /// Gets the first npc this npc has targetted on their own. Returns null if no npcs are targetted. Also removes dead npcs from the list. This method should be used instead of directly accessing the list.
    /// </summary>
    /// <returns></returns>
    public NPC GetFirstNaturallyTargettedEnemyNPC() {
        if (naturallyTargettedEnemyNPCs.Count == 0) return null;

        while (naturallyTargettedEnemyNPCs[0] == null) {
            naturallyTargettedEnemyNPCs.RemoveAt(0);
            if (naturallyTargettedEnemyNPCs.Count == 0) return null;
        }
        return naturallyTargettedEnemyNPCs[0];   
    }


    public NPC GetHighestPriorityTargettedNPC() {
        NPC targetNPC = playerTargettedEnemyNPCs.Count > 0 ? GetFirstPlayerTargettedEnemyNPC() : null;
        if (targetNPC == null) {
            targetNPC = GetFirstNaturallyTargettedEnemyNPC();
        }
        return targetNPC;
    }

    public void ClearPlayerTargettedEnemyNPCs() {
        foreach (NPC npc in playerTargettedEnemyNPCs) {
            npc.npcVisual.OnNPCTargettedChanged(false, false);
        }
        playerTargettedEnemyNPCs.Clear();
    }

    public void ClearNaturallyTargettedEnemyNPCs() {
        foreach (NPC npc in naturallyTargettedEnemyNPCs) {
            npc.npcVisual.OnNPCTargettedChanged(false, true);
        }
        naturallyTargettedEnemyNPCs.Clear();
    }

}

