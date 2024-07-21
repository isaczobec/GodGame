using System.Collections.Generic;
using UnityEngine;
public class NPCBehaviourAggresiveFighting : NPCBehaviourMercenary {


    public override void OnUpdateNPCTick() {

            NPC targetNPC = playerTargettedEnemyNPCs.Count > 0 ? GetFirstTargettedEnemyNPC() : null;
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
        if (action is BasicMeleeAttack) {
                

            NPC targetNPC = playerTargettedEnemyNPCs.Count > 0 ? GetFirstTargettedEnemyNPC() : null;
            if (targetNPC == null) return;

            if ((npc.coordinates - targetNPC.coordinates).magnitude > 3) return; 

            BasicMeleeAttack basicMeleeAttack = (BasicMeleeAttack)npcAnimationActionList.actions[0];
            basicMeleeAttack.targetNPC = targetNPC;
            if (targetNPC != null) {
                npc.TryBeginAnimationAction(npcAnimationActionList.actions[0]);
            }
        }
    }

    public override void OnPlayerAddAttackTarget(NPC targetNPC, bool isFirstTarget) {
        if (isFirstTarget) {
            npc.TryEndAnimationActionPremautrely();
            if (npcAnimationActionList.actions[0] is BasicMeleeAttack) {
                    BasicMeleeAttack basicMeleeAttack = (BasicMeleeAttack)npcAnimationActionList.actions[0];
                    basicMeleeAttack.targetNPC = targetNPC;

                    npc.SetMovementTarget(targetNPC.coordinates);
                    npc.MoveToNextTileInQueue();

                    if ((npc.coordinates - targetNPC.coordinates).magnitude <= 3) {
                        npc.TryBeginAnimationAction(npcAnimationActionList.actions[0]);
                    }
            }
        }

    }



}
