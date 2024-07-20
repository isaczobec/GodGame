using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCSelectionHitBox : MonoBehaviour
{
    private NPC npc;

    /// <summary>
    /// The middle of the hitbox. Used to find the most aimed at npc.
    /// </summary>
    [SerializeField] private Transform middlePoint;

    public Transform GetMiddlePoint() {
        return middlePoint;
    }

    public NPC GetNPC() {
        return npc;
    }

    public void SetNPC(NPC npc) {
        this.npc = npc;
    }
}
