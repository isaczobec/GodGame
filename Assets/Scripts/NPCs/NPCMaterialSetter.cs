using System;
using System.Collections.Generic;
using UnityEngine;

public class NPCMaterialSetter {

    public List<NPCRenderer> nPCRenderers {get; private set;} = new List<NPCRenderer>();

    public NPC npc {get; private set;}

    private Material invincibilityMaterial;

    bool isHovered = false;
    public void SetInvincibilityMaterial(Material material) {
        invincibilityMaterial = material;
    }
    private Material hoveredMaterial;
    public void SetHoveredMaterial(Material material) {
        hoveredMaterial = material;
    }


    public NPCMaterialSetter(SkinnedMeshRenderer[] skinnedMeshRenderers, MeshRenderer[] meshRenderers, NPC npc) {

        this.npc = npc;

        foreach (SkinnedMeshRenderer skinnedMeshRenderer in skinnedMeshRenderers) {
            NPCRenderer nPCRenderer = new NPCRenderer();
            nPCRenderer.skinnedMeshRenderer = skinnedMeshRenderer;
            nPCRenderer.originalMaterials = skinnedMeshRenderer.materials;
            nPCRenderers.Add(nPCRenderer);
        }
        foreach (MeshRenderer meshRenderer in meshRenderers) {
            NPCRenderer nPCRenderer = new NPCRenderer();
            nPCRenderer.meshRenderer = meshRenderer;
            nPCRenderer.originalMaterials = meshRenderer.materials;
            nPCRenderers.Add(nPCRenderer);
        }
    }

    public void SetupInvincibiltyMaterials() {
        npc.npcStats.OnInvincibilityChanged += ChangeInvincibiltyMaterials;
    }

    public void SetupHoveredMaterials() {
        PlayerActions.instance.OnNpcHoveredChanged += ChangeHoveredMaterials;
    }


    private void ChangeInvincibiltyMaterials(object sender, bool invincible)
    {
        if (invincible)
        {
            ApplyMaterials(new Material[] { invincibilityMaterial });
        }
        else
        {
            ApplyDefaultMaterials();
            // make sure to apply hovered material if npc is hovered
            if (isHovered) {
                AddMaterial(hoveredMaterial);
            }
        }
    }

    private void ChangeHoveredMaterials(object sender, NPCHoveredEventArgs args)
    {
        if (args.npc == npc)
        {
            if (!isHovered) {
                AddMaterial(hoveredMaterial);
                isHovered = true;
            }
        }
        else if (args.unHoveredOtherNPCs)
        {
            ApplyDefaultMaterials();
            isHovered = false;
        }
    }

    /// <summary>
    /// Applies the default materials to all NPC renderers; all meshes of an npc visual.
    /// </summary>
    public void ApplyDefaultMaterials() {
        foreach (NPCRenderer nPCRenderer in nPCRenderers) {
            if (nPCRenderer.skinnedMeshRenderer != null) {
                nPCRenderer.skinnedMeshRenderer.materials = nPCRenderer.originalMaterials;
            }
            if (nPCRenderer.meshRenderer != null) {
                nPCRenderer.meshRenderer.materials = nPCRenderer.originalMaterials;
            }
        }
    }

    /// <summary>
    /// Applies a given set of materials to all NPC renderers; all meshes of an npc visual.
    /// </summary>
    /// <param name="materials"></param>
    public void ApplyMaterials(Material[] materials) {
        foreach (NPCRenderer nPCRenderer in nPCRenderers) {
            if (nPCRenderer.skinnedMeshRenderer != null) {
                nPCRenderer.skinnedMeshRenderer.materials = materials;
            }
            if (nPCRenderer.meshRenderer != null) {
                nPCRenderer.meshRenderer.materials = materials;
            }
        }
    }

    /// <summary>
    /// Adds a material to all NPC renderers; all meshes of an npc visual.
    /// </summary>
    /// <param name="material"></param>
    public void AddMaterial(Material material) {
        foreach (NPCRenderer nPCRenderer in nPCRenderers) {
            if (nPCRenderer.skinnedMeshRenderer != null) {
                Material[] newMaterials = new Material[nPCRenderer.skinnedMeshRenderer.materials.Length + 1];
                for (int i = 0; i < nPCRenderer.skinnedMeshRenderer.materials.Length; i++) {
                    newMaterials[i] = nPCRenderer.skinnedMeshRenderer.materials[i];
                }
                newMaterials[newMaterials.Length - 1] = material;
                nPCRenderer.skinnedMeshRenderer.materials = newMaterials;
            }
            if (nPCRenderer.meshRenderer != null) {
                Material[] newMaterials = new Material[nPCRenderer.meshRenderer.materials.Length + 1];
                for (int i = 0; i < nPCRenderer.meshRenderer.materials.Length; i++) {
                    newMaterials[i] = nPCRenderer.meshRenderer.materials[i];
                }
                newMaterials[newMaterials.Length - 1] = material;
                nPCRenderer.meshRenderer.materials = newMaterials;
            }
        }
    }

}

public class NPCRenderer {

    /// <summary>
    /// A reference to a skinned mesh renderer belonging to an NPC.
    /// </summary>
    public SkinnedMeshRenderer skinnedMeshRenderer;
    /// <summary>
    /// A reference to a mesh renderer belonging to an NPC.
    /// </summary>
    public MeshRenderer meshRenderer;

    public Material[] originalMaterials;
}