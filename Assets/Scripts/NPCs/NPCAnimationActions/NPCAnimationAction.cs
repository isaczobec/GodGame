

using UnityEngine;


/// <summary>
/// A class for an npc action that is embodied by and triggered by an animation, ie an attack or an ability.
/// </summary>
public class NPCAnimationAction : MonoBehaviour {

    [HideInInspector] public NPC npc;

    public string animatorTriggerName;
    public bool allowMoving = false;



    public void AnimationActionPerformed() {
        // call methods
        OnActionPerformed();
    }
    
    public void AnimationActionEnded() {
        // call methods
        npc.AnimationActionEnded();
        OnActionEnded();
    }

    // OVERRIDABLE METHODS

    /// <summary>
    /// ran in npc class when the action is called to see if the action can be performed. Can ie check if a target is in range.
    /// </summary>
    /// <returns></returns>
    public virtual bool GetCanBePerformed() {
        if (npc.isPerformingAnimationAction) { // by default, an action cannot be performed if the npc is already performing an action
            return false;
        }
        return true;
    }

    public virtual void OnActionPerformed() {
        // override this method to add functionality to the action
    }

    public virtual void OnActionEnded() {
        // override this method to add functionality to the action
    }

    /// <summary>
    /// ran in npc class when the action is called to see if the action can be ended prematurely. can also perform different things.
    /// </summary>
    public virtual bool GetCanBeEndedPrematurely() {
        return true;
    }

}