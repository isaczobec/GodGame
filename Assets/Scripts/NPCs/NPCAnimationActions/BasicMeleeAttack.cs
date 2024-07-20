using UnityEngine;

public class BasicMeleeAttack : NPCAnimationAction
{

    public NPC targetNPC;


    public override void OnActionStarted()
    {
        npc.npcVisual.TurnToDirection(targetNPC.coordinates - npc.coordinates);
    }

    public override void OnActionPerformed()
    {
        npc.DealDamage(targetNPC, 30f);
    }
}