using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class NPCTargettedDecal : MonoBehaviour
{
    [SerializeField] private DecalProjector decalProjector;
    [SerializeField] private GameObject decalGameObject;
    [SerializeField] private Shader decalShader;


    [Header("Decal Settings")]
    [SerializeField] private float decalSizeUnSelected = 1f;
    [SerializeField] private float decalSizeSelected = 1.5f;


    // --- material reference strings ---

    private const string WasTargettedNaturally = "_WasTargettedNaturally";

    private const string IsEnemy = "_IsEnemy";


    // memeber variables    
    private bool currentlyTargetted = false;

    private NPC npc;

    /// <summary>
    /// Sets the NPC that this decal is attached to.
    /// </summary>
    /// <param name="npc"></param>
    public void SetNPCandSetup(NPC npc) {
        this.npc = npc;
        decalProjector.material = new Material(decalShader); // create a new material instance
        decalProjector.material.SetFloat(IsEnemy, npc.isOwnedByPlayer ? 0 : 1);

    }

    public void EnableTargetDecal(bool wasSelectedNaturally) {
        decalGameObject.SetActive(true);
        decalProjector.material.SetFloat(WasTargettedNaturally, wasSelectedNaturally ? 1 : 0);
    }

    public void DisableTargetDecal() {
        decalGameObject.SetActive(false);
    }

    public void SetDecalOn(bool enabled) {

        decalGameObject.SetActive(enabled);
        currentlyTargetted = enabled; // do this last so we can compare states
    }
}
