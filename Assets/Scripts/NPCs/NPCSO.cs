using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NPC", menuName = "NPC")]
public class NPCSO : ScriptableObject
{

    [Header("Visuals")]
    /// <summary>
    /// The prefab of the NPC that will be spawned
    /// </summary>
    public GameObject prefab;

    public NPCstats npcStats;
}
