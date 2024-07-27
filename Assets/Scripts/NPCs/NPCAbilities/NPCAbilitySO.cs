using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NPCAbility", menuName = "NPCAbility", order = 1)]
public class NPCAbilitySO : ScriptableObject {

    public string abilityName;
    
    /// <summary>
    /// An ability prefab that is supposed to have a script that inherits from NPCAbility and optionally an animationaction.
    /// </summary>
    public GameObject abilityPrefab;
}