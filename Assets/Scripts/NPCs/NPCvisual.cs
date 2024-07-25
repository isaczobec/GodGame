using System;
using System.Collections;
using System.Collections.Generic;
using RPGCharacterAnims.Actions;
using UnityEngine;

public class NPCvisual : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator npcAnimator;
    [SerializeField] private GameObject visualGameObject;
    [SerializeField] private NPCHealthBar npcHealthBar;
    public NPCHealthBar NPCHealthBar => npcHealthBar;

    [Header("Refs to Renderers")]
    [SerializeField] private MeshRenderer[] meshRenderers;
    [SerializeField] private SkinnedMeshRenderer[] skinnedMeshRenderers;

    [Header("Refs to materials")]

    [SerializeField] private Material invincibilityMaterial;
    [SerializeField] private Material hoveredMaterial;



    private NPCSelctedDecal npcSelctedDecal;
    private NPCTargettedDecal nPCTargettedDecal;
    private NPC nPC;


    private NPCMaterialSetter npcMaterialSetter;
    


    private NPCAnimationAction waitingOnActionToFinish;


    // string references
    private const string IsMoving = "IsMoving";
    private const string MovementSpeed = "MovementSpeed";
    private const string AttackSpeed = "AttackSpeed";
    private const string EndAnimationAction = "EndAnimationAction";



    // ----- animation variables ------

    [SerializeField] private float turningSpeed = 10f;
    private Coroutine turningCoroutine;


    public void Setup(NPC npc, NPCSelctedDecal npcSelctedDecal, NPCTargettedDecal nPCTargettedDecal) {
        nPC = npc;
        // subscribe to the events
        nPC.currentlyMovingUpdated += CurrentlyMovingUpdated;    
        nPC.forwardDirectionUpdated += ForwardDirectionUpdated;
        nPC.npcStats.OnMovementSpeedChanged += MovementSpeedUpdated;
        nPC.npcStats.OnAttackSpeedChanged += AttackSpeedUpdated;

        npc.OnDamageTaken += OnDamageTaken;

        // set initial values
        npcAnimator.SetFloat(MovementSpeed, nPC.npcStats.movementSpeed);
        npcAnimator.SetFloat(AttackSpeed, nPC.npcStats.attackSpeed);

        // set decal values
        this.npcSelctedDecal = npcSelctedDecal;
        this.nPCTargettedDecal = nPCTargettedDecal;

        // setup the npcMaterialSetter
        npcMaterialSetter = new NPCMaterialSetter(skinnedMeshRenderers, meshRenderers, npc);
        npcMaterialSetter.SetInvincibilityMaterial(invincibilityMaterial);
        npcMaterialSetter.SetupInvincibiltyMaterials();
        npcMaterialSetter.SetHoveredMaterial(hoveredMaterial);
        npcMaterialSetter.SetupHoveredMaterials();
    }


    private void AttackSpeedUpdated(object sender, float e)
    {
        Debug.Log("new attack speed: " + e);
        npcAnimator.SetFloat(AttackSpeed,e);
    }

    private void MovementSpeedUpdated(object sender, float e)
    {
        npcAnimator.SetFloat(MovementSpeed, e);
    }

    private void ForwardDirectionUpdated(object sender, Vector2 e)
    {
        TurnToDirection(e);
    }

    public void TurnToDirection(Vector2 direction) {
        if (turningCoroutine != null) {
            StopCoroutine(turningCoroutine);
        }
        turningCoroutine = StartCoroutine(TurnToDirectionCoroutine(direction));
    }

    private IEnumerator TurnToDirectionCoroutine(Vector2 direction) {
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

    public void OnMainSelectedChanged(bool mainSelected) {
        npcSelctedDecal.SetMainSelected(mainSelected);
    }

    public void OnNPCTargettedChanged(bool isTargetted, bool wasTargettedNaturally) {
        if (isTargetted) nPCTargettedDecal.EnableTargetDecal(wasTargettedNaturally);
        else if (nPC != null) nPCTargettedDecal.DisableTargetDecal();
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
    private void OnDamageTaken(object sender, HitInfo hitInfo)
    {
        if (hitInfo.wasHit) {
            DamageNumberManager.instance.CreateDamageNumber(nPC.transform.position, hitInfo.finalDamage, !nPC.isOwnedByPlayer);
        }
    }
}
