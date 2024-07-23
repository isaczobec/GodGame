using UnityEngine;

public class BasicMeleeAttack : NPCAnimationAction
{

    public NPC targetNPC;

    /// <summary>
    /// The range at which the npc SHOULD start the attack.
    /// </summary>
    public float maxStartRange = 2.9f;

    /// <summary>
    /// The range at which the target npc needs to be within for this attack to deal damage.
    /// </summary>
    public float maxPerformRange = 5f;


    public float baseDamage = 30f;



    public override void OnActionStarted()
    {
        npc.npcVisual.TurnToDirection(targetNPC.coordinates - npc.coordinates);
    }

    public override void OnActionPerformed()
    {
        if (targetNPC == null) {
            return;
        }
        if ((targetNPC.coordinates - npc.coordinates).magnitude <= maxPerformRange) {
            npc.DealDamage(targetNPC, baseDamage);
        }
    }
}