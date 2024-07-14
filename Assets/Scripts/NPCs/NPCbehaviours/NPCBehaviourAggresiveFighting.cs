using System.Collections.Generic;
using UnityEngine;
public class NPCBehaviourAggresiveFighting : NPCBehaviour {


    public override void OnUpdateNPCTick() {
        if (!npc.currentlyMoving) {

            List<NPC> visibleNPCs = npc.GetVisibleNPCs();
            if (visibleNPCs.Count > 0) {
                npc.SetMovementTarget(visibleNPCs[0].coordinates);
                npc.MoveToNextTileInQueue();
            }
        }

        }

}
