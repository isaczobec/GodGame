using System.Collections.Generic;
using UnityEngine;
public class NPCBehaviourMercenaryWarrior : NPCBehaviourMercenary {


    public override void OnUpdateNPCTick() {

        if (currentlyMovingToPlayerAssignedPosition) return;

            NPC targetNPC = playerTargettedEnemyNPCs.Count > 0 ? GetFirstPlayerTargettedEnemyNPC() : null;
            if (targetNPC == null) {
                targetNPC = GetFirstNaturallyTargettedEnemyNPC();
                if (targetNPC == null) {
                    
                    targetNPC = npc.GetClosestNPC(needsToBeEnemy: true);
                    AddNaturallyTargettedEnemyNPC(targetNPC);
                }
            }

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
                

            NPC targetNPC = GetHighestPriorityTargettedNPC();
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
            if (npcAnimationActionList.actions[0] is BasicMeleeAttack) {
                    BasicMeleeAttack basicMeleeAttack = (BasicMeleeAttack)npcAnimationActionList.actions[0];
                    if (basicMeleeAttack.targetNPC != targetNPC) {
                        if ((targetNPC.coordinates - npc.coordinates).magnitude > 3) {
                            npc.TryEndAnimationActionPremautrely();
                        } else {
                            // target new enemy and turn to it
                            npc.npcVisual.TurnToDirection(targetNPC.coordinates - npc.coordinates);
                        }
                    }
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
