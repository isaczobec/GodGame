using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCSelectionHitBox : MonoBehaviour
{
    private NPC npc;

    public NPC GetNPC() {
        return npc;
    }

    public void SetNPC(NPC npc) {
        this.npc = npc;
    }
}
