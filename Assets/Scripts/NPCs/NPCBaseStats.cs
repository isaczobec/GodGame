using System;
using UnityEngine;

/// <summary>
/// A class that holds all the stats of an NPC.
/// </summary>
[Serializable]
public class NPCBaseStats {

    [Header("Rendering")]
    /// <summary>
    /// How many chunks should be rendered around the NPC
    /// </summary>
    public int renderDistance = 3;

    [Header("Movement")]
    /// <summary>
    /// The speed at which the NPC moves. 1 means 1 tile per second. (scales linearly)
    /// </summary>
    public float movementSpeed = 1;

    /// <summary>
    /// The maximum steepness of the terrain the NPC can walk on. 1 means 45 degrees (calculus style lets go).
    /// </summary>
    public float maxWalkableSteepness = 1f;

    public float maxHealth = 100;




    // a constructor to create a copy of the object
    public NPCBaseStats(NPCBaseStats original) {
        renderDistance = original.renderDistance;
        movementSpeed = original.movementSpeed;
        maxWalkableSteepness = original.maxWalkableSteepness;
    }

}
