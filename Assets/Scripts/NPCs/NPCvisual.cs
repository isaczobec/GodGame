using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCvisual : MonoBehaviour
{
    [SerializeField] private Animator npcAnimator;
    [SerializeField] private GameObject visualGameObject;
    private NPC nPC;


    // string references
    private const string IsMoving = "IsMoving";
    private const string MovementSpeed = "MovementSpeed";



    // ----- animation variables ------

    [SerializeField] private float turningSpeed = 10f;
    private Coroutine turningCoroutine;


    public void Setup(NPC npc) {
        nPC = npc;
        // subscribe to the events
        nPC.currentlyMovingUpdated += CurrentlyMovingUpdated;    
        nPC.forwardDirectionUpdated += ForwardDirectionUpdated;
        nPC.npcStats.OnMovementSpeedChanged += MovementSpeedUpdated;

        // set initial values
        npcAnimator.SetFloat(MovementSpeed, nPC.npcStats.movementSpeed);
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
}
