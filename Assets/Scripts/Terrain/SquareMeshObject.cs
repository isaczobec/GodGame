using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.Rendering;


/// <summary>
/// Stores information about a flat rectangular mesh. Terrain height is manipulated later
/// </summary>
public class VisibleSquareMesh {

    public Vector2 centerPosition { get; private set; }

    /// <summary>
    /// The size of the rectangle in vertices along the x and z dimensions
    /// </summary>
    public int sizeX { get; private set; }
    public int sizeZ { get; private set; }
    public float quadSize { get; private set; }

    public Vector3[] vertices;
    private int[] triangles;
    private Vector2[] uvs;

    public Mesh mesh { get; private set; }

    public Vector2Int chunkCoordinates { get; private set; }

    public VisibleSquareMesh(Vector2 centerPosition, int sizeX, int sizeZ, float quadSize, Vector2Int chunkCoordinates, bool generateVerticesTriangles = true, bool bakeLighting = false) {
        this.centerPosition = centerPosition;
        this.sizeX = sizeX + 1;
        this.sizeZ = sizeZ + 1;
        this.quadSize = quadSize;
        this.chunkCoordinates = chunkCoordinates;

        mesh = new Mesh();

        if (generateVerticesTriangles) {
            CalculateVerticesArray(quadSize);
            CalculateTrianglesArray();
            UpdateMesh();
        }

        // doesnt work at all
        if (bakeLighting) {
            BakeLighting(LightDirection.Day);
        }

    }

    // Doesnt work at all
    public void BakeLighting(LightDirection lightDirection) {
        Vector3 lightDirectionVector = GlobalLightDirections.GetLightDirection(lightDirection);
        Vector3[] normals = mesh.normals;
        Color[] colors = new Color[vertices.Length];
        for (int i = 0; i < vertices.Length; i++) {
            Debug.Log(normals[i]);
            float lightIntensity = Mathf.Clamp01(Vector3.Dot(normals[i], -1 * lightDirectionVector));
            colors[i] = new Color(lightIntensity, lightIntensity, lightIntensity, 1);
        }
        mesh.colors = colors;
    }

    /// <summary>
    /// Generates the vertices array for this rectangle
    /// </summary>
    public void CalculateVerticesArray(float quadSize, bool setMeshVertices = false, bool setUVs = true) {

        Vector3[] newVertices = new Vector3[sizeX * sizeZ]; // set the size of the array

        Vector2[] newUVs = new Vector2[sizeX * sizeZ]; // create an array for UVs

        int currentVertexIndex = 0;
        for (int x = 0; x < sizeX; x++) {
            for (int z = 0; z < sizeZ; z++) {
                float height = 0; // the height of the vertex
                newVertices[x * sizeZ + z] = new Vector3(
                    centerPosition.x + x * quadSize - (sizeX - 1) * quadSize / 2,
                    height,
                    centerPosition.y + z * quadSize - (sizeZ - 1) * quadSize / 2
                );

                // set UVs to be on a grid between [0,0] and [1,1]
                if (setUVs) {
                    newUVs[currentVertexIndex] = new Vector2((float)x / (sizeX-1), (float)z / (sizeZ-1));
                }

                currentVertexIndex++;
            }
        }
        if (setMeshVertices) {
            mesh.vertices = newVertices;
        } else {
            vertices = newVertices;
        }

        if (setUVs) {
            uvs = newUVs;
        }

    }




    /// <summary>
    /// Generates the triangles array for this rectangle. Must be ran after CalculateVerticesArray().
    /// </summary>
    public void CalculateTrianglesArray() {
        triangles = new int[(sizeX - 1) * (sizeZ - 1) * 6];
        int triangleIndex = 0;
        for (int x = 0; x < sizeX - 1; x++) {
            for (int z = 0; z < sizeZ - 1; z++) {
                triangles[triangleIndex + 2] = x * sizeZ + z;
                triangles[triangleIndex + 1] = (x + 1) * sizeZ + z;
                triangles[triangleIndex] = x * sizeZ + z + 1;

                triangles[triangleIndex + 5] = x * sizeZ + z + 1;
                triangles[triangleIndex + 4] = (x + 1) * sizeZ + z;
                triangles[triangleIndex + 3] = (x + 1) * sizeZ + z + 1;

                triangleIndex += 6;
            }
        }
    }

    public void MoveCenterPosition(int x, int z) {
        float fx = x * quadSize;
        float fz = z * quadSize;
        centerPosition = new Vector2(centerPosition.x + fx, centerPosition.y + fz);
        for (int i = 0; i < vertices.Length; i++) {
            vertices[i].x += fx;
            vertices[i].z += fz;
        }
    }

    public void UpdateMesh() {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    public Vector3[] GetVertices() {
        return vertices;
    }
    public int[] GetTriangles() {
        return triangles;
    }

}


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
        meshCollider.sharedMesh = squareMesh.mesh;
    }   


    // ---------------------------
    public void Initialize(Material baseMaterial = null) {
        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshRenderer = gameObject.AddComponent<MeshRenderer>();

        if (baseMaterial != null) { 
            meshRenderer.material = baseMaterial; 
            ownedMaterial = meshRenderer.material; // create an instance of the material
            ownedMaterial.SetVector("_ChunkCoordinates", new Vector2(squareMesh.chunkCoordinates.x, squareMesh.chunkCoordinates.y));
            }

        meshFilter.mesh = squareMesh.mesh;

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
        if (updateSquareMesh) squareMesh.UpdateMesh();
        meshFilter.mesh = squareMesh.mesh;
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
}