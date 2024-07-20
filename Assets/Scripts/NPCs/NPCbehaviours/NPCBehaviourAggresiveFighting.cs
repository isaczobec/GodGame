using System.Collections.Generic;
using UnityEngine;
public class NPCBehaviourAggresiveFighting : NPCBehaviour {


    public override void OnUpdateNPCTick() {

            NPC targetNPC = npc.GetClosestNPC();

            if (targetNPC != null) {
                npc.SetMovementTarget(targetNPC.coordinates);
                npc.MoveToNextTileInQueue();

                if ((npc.coordinates - targetNPC.coordinates).magnitude <= 3) {
                    npc.TryBeginAnimationAction(npcAnimationActionList.actions[0]);
                }
            }

        }



}
