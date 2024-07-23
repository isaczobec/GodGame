using System.Collections.Generic;
using UnityEngine;

public class NPCAnimationActionList : MonoBehaviour {
    public List<NPCAnimationAction> actions;

    public NPC npc;

    public void SetupActions(NPC npc) {
        npc.animationActionList = this;
        this.npc = npc;
        foreach (NPCAnimationAction action in actions) {
            action.npc = npc;
        }
    }

    public void AddAction(NPCAnimationAction action) {
        action.npc = npc;
        actions.Add(action);
    }
}