using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class NPCSelctedDecal : MonoBehaviour
{

    [SerializeField] private DecalProjector decalProjector;
    [SerializeField] private GameObject decalGameObject;
    [SerializeField] private Shader decalShader;


    [Header("Decal Settings")]
    [SerializeField] private float decalSizeUnSelected = 1f;
    [SerializeField] private float decalSizeSelected = 1.5f;


    // --- material reference strings ---
    private const string IsEnemy = "_IsEnemy";
    private const string IsSelected = "_IsSelected";
    private const string IsMainSelected = "_IsMainSelected";
    


    // memeber variables    
    private bool currentlySelected = false;

    private NPC npc;

    /// <summary>
    /// Sets the NPC that this decal is attached to.
    /// </summary>
    /// <param name="npc"></param>
    public void SetNPCandSetup(NPC npc) {
        this.npc = npc;
        decalProjector.material = new Material(decalShader); // create a new material instance

        // set the material values
        decalProjector.material.SetFloat(IsEnemy, npc.isOwnedByPlayer ? 0 : 1);
    }

    public void SetDecalOn(bool enabled) {
        if (decalGameObject == null) return;
        decalGameObject.transform.localScale = new Vector3(1f,1f,1f) * (enabled ? decalSizeSelected : decalSizeUnSelected) + new Vector3(0f, 0f, 1f);
        decalProjector.material.SetFloat(IsSelected, enabled ? 1 : 0);
        currentlySelected = enabled; // do this last so we can compare states
    }

    public void SetMainSelected(bool mainSelected) {
        decalProjector.material.SetFloat(IsMainSelected, mainSelected ? 1 : 0);
    }
}
