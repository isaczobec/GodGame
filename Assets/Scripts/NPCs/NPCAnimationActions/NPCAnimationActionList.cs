using System.Collections.Generic;
using UnityEngine;

public class NPCAnimationActionList : MonoBehaviour {
    public List<NPCAnimationAction> actions;

    public void SetupActions(NPC npc) {
        foreach (NPCAnimationAction action in actions) {
            action.npc = npc;
        }
    }
}