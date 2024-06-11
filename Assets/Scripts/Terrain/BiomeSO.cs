using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Biome", menuName = "Biome")]
public class BiomeSO : ScriptableObject
{
    [Tooltip("inlandness, heat, humidity cube")]
    public Biome biome;
}
