using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCAbilityList
{
    public int maxAbilities {get; private set;} = 4;
    private NPC ownerNPC;

    private NPCAbility[] abilities;

    public NPCAbilityList(NPC ownerNPC) {
        this.ownerNPC = ownerNPC;

        abilities = new NPCAbility[maxAbilities];
    }

    public void SetAbilityAtIndex(NPCAbility ability, int index) {
        if (ability != null) {
            abilities[index] = ability;
            abilities[index].SetupAbility(ownerNPC);
        }
    }

    public NPCAbility GetAbilityAtIndex(int index) {
        return abilities[index];
    }

    public bool TryCastAbilityAtIndex(int index) {
        if (abilities[index-1] != null) {
            return abilities[index-1].TryCastAbility();
        }
        Debug.Log("Is null");
        return false;
    }
}
