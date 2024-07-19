using System.Collections.Generic;
using UnityEngine;
public class NPCBehaviourAggresiveFighting : NPCBehaviour {


    public override void OnUpdateNPCTick() {
        if (!npc.currentlyMoving) {

            NPC targetNPC = npc.GetClosestNPC();
            if (targetNPC != null) {
                npc.SetMovementTarget(targetNPC.coordinates);
                npc.MoveToNextTileInQueue();
            }
        }

        }

}
