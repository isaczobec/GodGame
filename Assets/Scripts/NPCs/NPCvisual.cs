using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCvisual : MonoBehaviour
{
    [SerializeField] private Animator npcAnimator;
    [SerializeField] private GameObject visualGameObject;

    private NPCSelctedDecal npcSelctedDecal;
    private NPC nPC;


    private NPCAnimationAction waitingOnActionToFinish;


    // string references
    private const string IsMoving = "IsMoving";
    private const string MovementSpeed = "MovementSpeed";
    private const string EndAnimationAction = "EndAnimationAction";



    // ----- animation variables ------

    [SerializeField] private float turningSpeed = 10f;
    private Coroutine turningCoroutine;


    public void Setup(NPC npc, NPCSelctedDecal npcSelctedDecal) {
        nPC = npc;
        // subscribe to the events
        nPC.currentlyMovingUpdated += CurrentlyMovingUpdated;    
        nPC.forwardDirectionUpdated += ForwardDirectionUpdated;
        nPC.npcStats.OnMovementSpeedChanged += MovementSpeedUpdated;

        // set initial values
        npcAnimator.SetFloat(MovementSpeed, nPC.npcStats.movementSpeed);

        // set decal values
        this.npcSelctedDecal = npcSelctedDecal;
    }

    private void MovementSpeedUpdated(object sender, float e)
    {
        npcAnimator.SetFloat(MovementSpeed, e);
    }

    private void ForwardDirectionUpdated(object sender, Vector2 e)
    {
        if (turningCoroutine != null) {
            StopCoroutine(turningCoroutine);
        }
        turningCoroutine = StartCoroutine(TurnToDirection(e));
    }

    private IEnumerator TurnToDirection(Vector2 direction) {
        Vector3 targetDirection = new Vector3(direction.x, 0, direction.y);
        while (Vector3.Angle(visualGameObject.transform.forward, targetDirection) > 0.1f) {
            visualGameObject.transform.forward = Vector3.RotateTowards(visualGameObject.transform.forward, targetDirection, turningSpeed * Time.deltaTime, 0.0f);
            yield return null;
        }
    }

    private void CurrentlyMovingUpdated(object sender, bool e)
    {
        npcAnimator.SetBool(IsMoving, e);
    }

    public void SetNPC(NPC npc) {
        nPC = npc;
    }

    /// <summary>
    /// should be called from PlayerActions When the changed npc is selected or deselected.
    /// </summary>
    /// <param name="selected"></param>
    public void OnNPCSelectedChanged(bool selected) {
        npcSelctedDecal.SetDecalOn(selected);
    }


    // ANIMATION ACTIONS


    public void OnAnimationActionPerformed() {
        if (waitingOnActionToFinish != null) {
            waitingOnActionToFinish.AnimationActionPerformed();
        }
    }

    public void OnAnimationActionEnded() {
        if (waitingOnActionToFinish != null) {
            waitingOnActionToFinish.AnimationActionEnded();
            waitingOnActionToFinish = null;
        }
    
    }

    public void StartAnimationAction(NPCAnimationAction action) {
        npcAnimator.SetTrigger(action.animatorTriggerName);
        waitingOnActionToFinish = action;
    }

    public void EndAnimationActionPremautrely() {
        npcAnimator.SetTrigger(EndAnimationAction);
        waitingOnActionToFinish = null;
    }
}
