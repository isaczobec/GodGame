using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class NPCSelctedDecal : MonoBehaviour
{

    [SerializeField] private DecalProjector decalProjector;

    private NPC npc;

    /// <summary>
    /// Sets the NPC that this decal is attached to.
    /// </summary>
    /// <param name="npc"></param>
    public void SetNPC(NPC npc) {
        this.npc = npc;
    }

    public void SetDecalOn(bool enabled) {
        decalProjector.enabled = enabled;
    }
}
