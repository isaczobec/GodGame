using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCAbility : MonoBehaviour
{
    private NPC ownerNPC;

    /// <summary>
    /// The animationAction that will be played when the ability is cast. Can be null if this ability does not have an animation.
    /// </summary>
    public NPCAnimationAction npcAnimationAction;

    public void SetupAbility(NPC ownerNPC) {
        this.ownerNPC = ownerNPC;
        
        if (npcAnimationAction != null) {
            ownerNPC.animationActionList.AddAction(npcAnimationAction);
        }
    }

    public float currentCooldown = 0;
    public float maxCooldown = 3;

    private void Update() {
        if (currentCooldown > 0) {
            currentCooldown -= Time.deltaTime;
        }
    }

    public bool TryCastAbility() {

        if (currentCooldown > 0) return false; // cant cast if on cooldown
        
        if (GetCanBeCast()) {
            currentCooldown = maxCooldown;
            if (npcAnimationAction != null) {
                if (ownerNPC.TryBeginAnimationAction(npcAnimationAction)) {
                    OnCast();
                }
            } else {
                OnCast();
            }
            return true;
        }
        return false;
    }

    /// <summary>
    /// Override this method to add checks for if a specific ability can be cast.
    /// </summary>
    /// <returns></returns>
    public virtual bool GetCanBeCast() {
        return true;
    }

    /// <summary>
    /// Override this method to add functionality to the ability. If this ability has an animation, 
    /// it will be played before this method is called. If this is the case, 
    /// this method usually does not have to do anything as stuff is dont in the animation actions performed method.
    /// </summary>
    public virtual void OnCast() {
    }
}
