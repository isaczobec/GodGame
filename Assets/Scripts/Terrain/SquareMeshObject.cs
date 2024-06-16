using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.Rendering;



public class BiomeMaskTextures {

        // biome mask textures
    public Texture2D biomeMaskTexture1;
    public string biomeMaskTextureName1 = "_BiomeMaskTexture1";
    public Texture2D biomeMaskTexture2;
    public string biomeMaskTextureName2 = "_BiomeMaskTexture2";

    private Texture2D[] biomeMaskTextures;

    public int textureSize = 32;

    public float[,,,] biomeMaskValues;

    public void InitializeBiomeMaskTextures(Material ownedMaterial) {
        biomeMaskTexture1 = new Texture2D(textureSize, textureSize) {wrapMode = TextureWrapMode.Mirror};
        ownedMaterial.SetTexture(biomeMaskTextureName1, biomeMaskTexture1);
        biomeMaskTexture2 = new Texture2D(textureSize, textureSize) {wrapMode = TextureWrapMode.Mirror};
        ownedMaterial.SetTexture(biomeMaskTextureName2, biomeMaskTexture2);

        biomeMaskTextures = new Texture2D[] { biomeMaskTexture1, biomeMaskTexture2 };
        biomeMaskValues = new float[biomeMaskTextures.Length, textureSize, textureSize, 4]; // four color channels
    }

    /// <summary>
    /// Sets the value of a pixel in the biome mask texture. Does not apply the changes to the texture!
    /// </summary>
    public void SetBiomeMask(int index, int x, int y, float value) {
        int bigIndex = index / 4;
        int smallIndex = index % 4;

        float[] vals = new float[] {0f,0f,0f,0f};
        vals[smallIndex] = value;
        // set all values
        for (int i = 0; i < 4; i++) {
            biomeMaskValues[bigIndex, x, y, i] += vals[i];
        }

    }

    public void ApplyAllBiomeMaskChanges() {
        for (int i = 0; i < biomeMaskTextures.Length; i++) {

            for (int x = 0; x < textureSize; x++) {
                for (int y = 0; y < textureSize; y++) {
                    Color color = new Color(biomeMaskValues[i, x, y, 0], biomeMaskValues[i, x, y, 1], biomeMaskValues[i, x, y, 2], biomeMaskValues[i, x, y, 3]);
                    biomeMaskTextures[i].SetPixel(x, y, color);
                }
            }

            biomeMaskTextures[i].Apply();
        }
    }

}

public class SquareMeshObject : MonoBehaviour {

    public VisibleSquareMesh squareMesh;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    private Material ownedMaterial;

    private MeshCollider meshCollider;


    private float visibilityMultiplier = 1f; // the visibility of the square mesh object. 1 if an entity can see it. lower values makes the material darker
    public string visibilityMultiplierName = "_VisibilityMultiplier";
    private Coroutine visibilityMultiplierCoroutine;
    


    // ----- TEXTURE SETTINGS -----
    private int textureSize = 32;

    public Texture2D inlandnessTexture;
    public string inlandnessTextureName = "_InlandnessTexture";
    public Texture2D plainnessTexture;
    public string plainnessTextureName = "_PlainnessTexture";
    public Texture2D bumpinessTexture;
    public string bumpinessTextureName = "_BumpinessTexture";
    public Texture2D humidityTexture;
    public string humidityTextureName = "_HumidityTexture";
    public Texture2D heatTexture;
    public string heatTextureName = "_HeatTexture";
    public Texture2D steepnessTexture;
    public string steepnessTextureName = "_SteepnessTexture";


    public Texture2D inlandnessHumidityHeatTexture;
    public string inlandnessHumidityHeatTextureName = "_InlandnessHumidityHeatTexture";

    public Texture2D plainnessBumpinessSteepnessTexture;
    public string plainnessBumpinessSteepnessTextureName = "_PlainnessBumpinessSteepnessTexture";



    public BiomeMaskTextures biomeMaskTextures;






    public void AddMeshCollider() {
        meshCollider = gameObject.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = squareMesh.meshes[squareMesh.currentLOD];
    }   


    // ---------------------------
    public void Initialize(Material baseMaterial = null) {
        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshRenderer = gameObject.AddComponent<MeshRenderer>();

        // meshFilter.mesh = squareMesh.meshes[squareMesh.currentLOD];


        if (baseMaterial != null) { 
            meshRenderer.material = baseMaterial; 
            ownedMaterial = meshRenderer.material; // create an instance of the material
            ownedMaterial.SetVector("_ChunkCoordinates", new Vector2(squareMesh.chunkCoordinates.x, squareMesh.chunkCoordinates.y));
            }


        InitializeTextures();

    }

    private void InitializeTextures() {
        inlandnessTexture = new Texture2D(textureSize, textureSize) {wrapMode = TextureWrapMode.Mirror};
        ownedMaterial.SetTexture(inlandnessTextureName, inlandnessTexture);
        plainnessTexture = new Texture2D(textureSize, textureSize) {wrapMode = TextureWrapMode.Mirror};
        ownedMaterial.SetTexture(plainnessTextureName, plainnessTexture);
        bumpinessTexture = new Texture2D(textureSize, textureSize) {wrapMode = TextureWrapMode.Mirror};
        ownedMaterial.SetTexture(bumpinessTextureName, bumpinessTexture);
        humidityTexture = new Texture2D(textureSize, textureSize) {wrapMode = TextureWrapMode.Mirror};
        ownedMaterial.SetTexture(humidityTextureName, humidityTexture);
        heatTexture = new Texture2D(textureSize, textureSize) {wrapMode = TextureWrapMode.Mirror};
        ownedMaterial.SetTexture(heatTextureName, heatTexture);
        steepnessTexture = new Texture2D(textureSize, textureSize) {wrapMode = TextureWrapMode.Mirror};
        ownedMaterial.SetTexture(steepnessTextureName, steepnessTexture);

        inlandnessHumidityHeatTexture = new Texture2D(textureSize, textureSize) {wrapMode = TextureWrapMode.Mirror};
        ownedMaterial.SetTexture(inlandnessHumidityHeatTextureName, inlandnessHumidityHeatTexture);

        plainnessBumpinessSteepnessTexture = new Texture2D(textureSize, textureSize) {wrapMode = TextureWrapMode.Mirror};
        ownedMaterial.SetTexture(plainnessBumpinessSteepnessTextureName, plainnessBumpinessSteepnessTexture);

        biomeMaskTextures = new BiomeMaskTextures();
        biomeMaskTextures.InitializeBiomeMaskTextures(ownedMaterial);
    }

    public void UpdateMesh(bool updateSquareMesh = true) {
        if (updateSquareMesh) squareMesh.UpdateMesh(squareMesh.currentLOD);
        meshFilter.mesh = squareMesh.meshes[squareMesh.currentLOD];
    }

    public void SetLOD(int LOD) {
        if (squareMesh.generatedLODs[LOD] == false) squareMesh.GenerateLOD(LOD); // we need to generate the LOD
        meshFilter.mesh = squareMesh.meshes[LOD];
    }

    public void MoveCenterPosition(int x, int z) {
        squareMesh.MoveCenterPosition(x, z);
    }


    public void SetVisibilityMultiplier(float targetVisibilityMultiplier, float duration = 0.2f) {
        if (visibilityMultiplierCoroutine != null) {
            StopCoroutine(visibilityMultiplierCoroutine);
        }
        visibilityMultiplierCoroutine = StartCoroutine(UpdateVisibilityMultiplier(targetVisibilityMultiplier,duration));
    }


    public IEnumerator UpdateVisibilityMultiplier(float targetVisibilityMultiplier, float time) {
        float startVisibilityMultiplier = visibilityMultiplier;
        float currentTime = 0;
        while (currentTime < time) {
            visibilityMultiplier = Mathf.Lerp(startVisibilityMultiplier, targetVisibilityMultiplier, currentTime / time);
            ownedMaterial.SetFloat(visibilityMultiplierName, visibilityMultiplier);
            currentTime += Time.deltaTime;
            yield return null;
        }
        visibilityMultiplier = targetVisibilityMultiplier;
        ownedMaterial.SetFloat(visibilityMultiplierName, visibilityMultiplier);
    }


    public IEnumerator testCouroutine() {

        float secToWait = 4;
        while (true) {

            yield return new WaitForSeconds(secToWait);
            SetLOD(2);
            yield return new WaitForSeconds(secToWait);
            SetLOD(1);
            yield return new WaitForSeconds(secToWait);
            SetLOD(0);
            yield return new WaitForSeconds(secToWait);
            SetLOD(3);

        }

    }


    
}