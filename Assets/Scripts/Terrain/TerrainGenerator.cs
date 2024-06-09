using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator: MonoBehaviour
{



    private int seed = 0;
    [SerializeField] private float multiplyRandomNumbersWith = 1000f;

    [Header("WORLD GENERATION SETTINGS")]

    [Header("Perlin generator settings")]
    [SerializeField] private PerlinGenerator inlandnessPerlinGenerator; // a perlin generator for the inlandness of the terrain, ie how elevated and mountainous it is
    [SerializeField] private PerlinGenerator plainnessPerlinGenerator; // a perlin generator for the plainness of the terrain, ie how flat it is
    [SerializeField] private PerlinGenerator bumpinessPerlinGenerator; // a perlin generator for the bumpiness of the terrain, ie how bumpy it is. Used in tandem with the plainness perlin generator, which is a multiplier with this one



    // -------------------------

    public void SetSeed(int seed) {
        this.seed = seed;
    }

    void Start()
    {
        InitializePerlinGenerators(seed);
    }


    private void InitializePerlinGenerators(int seed) {

        Random.InitState(seed);
        inlandnessPerlinGenerator.SetOrigin(GetPseudoRandomVector2());

    }

    private Vector2 GetPseudoRandomVector2() {
        return new Vector2(Random.Range(1f,2f), Random.Range(1f,2f)) * multiplyRandomNumbersWith; // random between 1 and 2 because sampling negative numbers gives symmetric noise
    }

    /// <summary>
    /// Generates the height of the terrain at a given position
    /// </summary>
    public float GenerateHeight(Vector2 position) {
        float height = inlandnessPerlinGenerator.SampleNosie(position);
        return height;
    }

}
