
using System.Collections.Generic;
using UnityEngine;

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
    public NPC friendlyNPCToFollow;
    public Vector2Int PlayerAssignedPositionToMoveTo;
    public bool reachedPlayerAssignedPosition = false;
    public MercenaryBehaviourOption mercenaryBehaviourOption = MercenaryBehaviourOption.autoAttack;


    /// <summary>
    /// Overridable method that is called when the npc moves to a new tile. By default, prioritizes moving to the player assigned position.
    /// </summary>
    public virtual void OnMovementTargetSetByPlayer(Vector2Int position) {
        PlayerAssignedPositionToMoveTo = position;
        reachedPlayerAssignedPosition = false;

        npc.TryEndAnimationActionPremautrely();

        // clear the targetted npcs if the player assigns a new position to move to
        ClearTargettedEnemyNPCs();
        friendlyNPCToFollow = null;

        npc.SetMovementTarget(position);
        npc.MoveToNextTileInQueue();
    }


    public void AddPlayerAttackTarget(NPC targetNPC) {
        playerTargettedEnemyNPCs.Add(targetNPC);
        OnPlayerAddAttackTarget(targetNPC, targetNPC == playerTargettedEnemyNPCs[0]);
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
    public NPC GetFirstTargettedEnemyNPC() {
        if (playerTargettedEnemyNPCs.Count == 0) return null;

        while (playerTargettedEnemyNPCs[0] == null) {
            playerTargettedEnemyNPCs.RemoveAt(0);
            if (playerTargettedEnemyNPCs.Count == 0) return null;
        }
        return playerTargettedEnemyNPCs[0];   
    }

    public void ClearTargettedEnemyNPCs() {
        playerTargettedEnemyNPCs.Clear();
    }

}

