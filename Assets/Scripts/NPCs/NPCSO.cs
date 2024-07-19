using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NPC", menuName = "NPC")]
public class NPCSO : ScriptableObject
{

    public string npcName;

    [Header("Visuals")]
    /// <summary>
    /// The prefab of the NPC that will be spawned
    /// </summary>
    public GameObject prefab;

    public NPCBaseStats npcBaseStats;

    public NPCBehaviourType npcBehaviourType;

    public bool isMercenary;

    public Vector3 selectedHitBoxDimensions = new Vector3(3, 3, 3);
}
