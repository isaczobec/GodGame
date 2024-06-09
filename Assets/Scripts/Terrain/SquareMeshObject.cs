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

    public VisibleSquareMesh(Vector2 centerPosition, int sizeX, int sizeZ, float quadSize, bool generateVerticesTriangles = true) {
        this.centerPosition = centerPosition;
        this.sizeX = sizeX + 1;
        this.sizeZ = sizeZ + 1;
        this.quadSize = quadSize;

        mesh = new Mesh();

        if (generateVerticesTriangles) {
            CalculateVerticesArray(quadSize);
            CalculateTrianglesArray();
            UpdateMesh();
        }

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
                    newUVs[currentVertexIndex] = new Vector2((float)x / sizeX, (float)z / sizeZ);
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

    public int textureSize = 10;

    public float[,,,] biomeMaskValues;

    public void InitializeBiomeMaskTextures(Material ownedMaterial) {
        biomeMaskTexture1 = new Texture2D(textureSize, textureSize);
        ownedMaterial.SetTexture(biomeMaskTextureName1, biomeMaskTexture1);
        biomeMaskTexture2 = new Texture2D(textureSize, textureSize);
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
    


    // ----- TEXTURE SETTINGS -----
    private int textureSize = 32;
    private int smallerTextureSize = 10;

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
    public Texture2D colorTexture;
    public string colorTextureName = "_ColorTexture";

    public BiomeMaskTextures biomeMaskTextures;









    // ---------------------------
    public void Initialize(Material baseMaterial = null) {
        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshRenderer = gameObject.AddComponent<MeshRenderer>();

        if (baseMaterial != null) { 
            meshRenderer.material = baseMaterial; 
            ownedMaterial = meshRenderer.material; // create an instance of the material
            }

        meshFilter.mesh = squareMesh.mesh;

        InitializeTextures();
    }

    private void InitializeTextures() {
        inlandnessTexture = new Texture2D(textureSize, textureSize);
        ownedMaterial.SetTexture(inlandnessTextureName, inlandnessTexture);
        plainnessTexture = new Texture2D(textureSize, textureSize);
        ownedMaterial.SetTexture(plainnessTextureName, plainnessTexture);
        bumpinessTexture = new Texture2D(textureSize, textureSize);
        ownedMaterial.SetTexture(bumpinessTextureName, bumpinessTexture);
        humidityTexture = new Texture2D(textureSize, textureSize);
        ownedMaterial.SetTexture(humidityTextureName, humidityTexture);
        heatTexture = new Texture2D(textureSize, textureSize);
        ownedMaterial.SetTexture(heatTextureName, heatTexture);
        colorTexture = new Texture2D(textureSize, smallerTextureSize);
        ownedMaterial.SetTexture(colorTextureName, colorTexture);

        biomeMaskTextures = new BiomeMaskTextures();
        biomeMaskTextures.InitializeBiomeMaskTextures(ownedMaterial);
    }

    public void UpdateMesh(bool updateSquareMesh = true) {
        if (updateSquareMesh) squareMesh.UpdateMesh();
        meshFilter.mesh.Clear();
        meshFilter.mesh = squareMesh.mesh;
        meshFilter.mesh.RecalculateNormals();
    }

    public void MoveCenterPosition(int x, int z) {
        squareMesh.MoveCenterPosition(x, z);
    }
}