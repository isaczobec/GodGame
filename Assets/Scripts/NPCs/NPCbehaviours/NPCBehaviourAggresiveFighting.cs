using System.Collections.Generic;
using UnityEngine;
public class NPCBehaviourAggresiveFighting : NPCBehaviour {


    public override void OnUpdateNPCTick() {

            NPC targetNPC = npc.GetClosestNPC();
            if (npcAnimationActionList.actions[0] is BasicMeleeAttack) {
                BasicMeleeAttack basicMeleeAttack = (BasicMeleeAttack)npcAnimationActionList.actions[0];
                basicMeleeAttack.targetNPC = targetNPC;
            }

            if (targetNPC != null) {
                npc.SetMovementTarget(targetNPC.coordinates);
                npc.MoveToNextTileInQueue();

                if ((npc.coordinates - targetNPC.coordinates).magnitude <= 3) {
                    npc.TryBeginAnimationAction(npcAnimationActionList.actions[0]);
                }
            }

        }

    public override void OnNpcAnimationActionEnded(NPCAnimationAction action) { 
        Debug.Log("OnNpcAnimationActionEnded");
        if (action is BasicMeleeAttack) {
                

            NPC targetNPC = npc.GetClosestNPC();

            if ((npc.coordinates - targetNPC.coordinates).magnitude > 3) return; 

            BasicMeleeAttack basicMeleeAttack = (BasicMeleeAttack)npcAnimationActionList.actions[0];
            basicMeleeAttack.targetNPC = targetNPC;
            if (targetNPC != null) {
                npc.TryBeginAnimationAction(npcAnimationActionList.actions[0]);
            }
        }
    }



}
