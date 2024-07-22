using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCBehaviourBasicMelee : NPCBehaviour
{

    private BasicMeleeAttack basicMeleeAttack;

    public override void OnNPCStart()
    {
        // set the basic melee attack
        foreach (NPCAnimationAction action in npcAnimationActionList.actions)
        {
            if (action is BasicMeleeAttack)
            {
                basicMeleeAttack = (BasicMeleeAttack)action;
                break;
            }
        }
    }

    public override void OnUpdateNPCTick()
    {
        NPC targetNPC = npc.GetClosestNPC(needsToBeEnemy: true);
        if (targetNPC != null) {
            SetAttackTarget(npc.GetClosestNPC(needsToBeEnemy: true));
            basicMeleeAttack.targetNPC = mainAttackTargetNPC;

            if (!TryAttackTarget())
            {
                npc.SetMovementTarget(mainAttackTargetNPC.coordinates);
                npc.MoveToNextTileInQueue();
            }

        }
    }

    public override void OnMovedToNewTile(object sender, ChunkTile e)
    {
        TryAttackTarget();
    }

    public override void OnNpcAnimationActionEnded(NPCAnimationAction action)
    {
        if (action is BasicMeleeAttack)
        {
            TryAttackTarget();
        }
    }



    private bool TryAttackTarget()
    {
        if (mainAttackTargetNPC == null) return false;
        if ((npc.coordinates - mainAttackTargetNPC.coordinates).magnitude <= basicMeleeAttack.maxStartRange)
        {
            npc.TryBeginAnimationAction(basicMeleeAttack);
            return true;
        }
        return false;
    }
}
