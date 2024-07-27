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

    /// <summary>
    /// Instantiates an ability from the given NPCAbilitySO and sets it at the given index.
    /// </summary>
    /// <param name="abilitySO"></param>
    /// <param name="index"></param>
    public void AddAbilityFromAbilitySO(NPCAbilitySO abilitySO, int index) {
        if (abilitySO != null) {
            GameObject abilityObject = GameObject.Instantiate(abilitySO.abilityPrefab);
            abilityObject.transform.parent = ownerNPC.transform;
            NPCAbility ability = abilityObject.GetComponent<NPCAbility>();
            SetAbilityAtIndex(ability, index);
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
