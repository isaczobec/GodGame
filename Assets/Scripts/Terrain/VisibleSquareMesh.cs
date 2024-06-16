using System.Collections.Generic;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

/// <summary>
/// Stores information about a flat rectangular mesh. Terrain height is manipulated later
/// </summary>
public class VisibleSquareMesh {

    public Vector2 centerPosition { get; private set; }

    /// <summary>
    /// The size of the rectangle in vertices along the x dimension
    /// </summary>
    public int sizeX { get; private set; }
    /// <summary>
    /// The size of the rectangle in vertices along the z dimension
    /// </summary>
    public int sizeZ { get; private set; }
    public float quadSize { get; private set; }

    public Vector3[] vertices;
    private List<int[]> triangles = new List<int[]>(); // the first one is the highest LOD
    private Vector2[] uvs;

    public Mesh[] meshes { get; private set; } // the first one is the ghighest LOD

    public int levelsOfDetail; // how many subdivisions of 2 there are in the LOD

    public int currentLOD { get; private set; }
    public bool[] generatedLODs { get; private set; }

    public Vector2Int chunkCoordinates { get; private set; }

    public VisibleSquareMesh(Vector2 centerPosition, int sizeX, int sizeZ, float quadSize, Vector2Int chunkCoordinates, bool generateVerticesTriangles = true, bool bakeLighting = false, int levelsOfDetail = 4) {

        this.levelsOfDetail = levelsOfDetail;
        currentLOD = levelsOfDetail - 1;
        generatedLODs = new bool[levelsOfDetail];

        // for LOD to work, sizeX must be a multiple of 2^levelsOfDetail
        if ((sizeX-1) % Mathf.Pow(2,levelsOfDetail) != 0) {
            Debug.LogError("SizeX-1 must be a multiple of 2^levelsOfDetail");
        }

        this.centerPosition = centerPosition;
        this.sizeX = sizeX;
        this.sizeZ = sizeZ;
        this.quadSize = quadSize;
        this.chunkCoordinates = chunkCoordinates;



        InitializeVerticiesArray();
        InitializeTrianglesArray();
        meshes = new Mesh[levelsOfDetail];

        if (generateVerticesTriangles) {
            CalculateVerticesArray(quadSize);
        }

    }

    public void InitializeVerticiesArray() {
        vertices = new Vector3[sizeX * sizeZ];
    }

    private void InitializeTrianglesArray() {
        for (int i = 0; i < levelsOfDetail; i++) {
            int s = sizeX / (int)Mathf.Pow(2, i);
            triangles.Add(new int[s * s * 6]);
        }
    }


    // Doesnt work at all
    // public void BakeLighting(LightDirection lightDirection) {
    //     Vector3 lightDirectionVector = GlobalLightDirections.GetLightDirection(lightDirection);
    //     Vector3[] normals = mesh.normals;
    //     Color[] colors = new Color[vertices.Length];
    //     for (int i = 0; i < vertices.Length; i++) {
    //         Debug.Log(normals[i]);
    //         float lightIntensity = Mathf.Clamp01(Vector3.Dot(normals[i], -1 * lightDirectionVector));
    //         colors[i] = new Color(lightIntensity, lightIntensity, lightIntensity, 1);
    //     }
    //     mesh.colors = colors;
    // }

    /// <summary>
    /// Generates the vertices array for this rectangle
    /// </summary>
    public void CalculateVerticesArray(float quadSize, bool setMeshVertices = false, bool setUVs = true) { // if memory becomes a problem this can perhaps be optimised by not generating the full res array at once
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
                    newUVs[currentVertexIndex] = new Vector2((float)x / (sizeX - 1), (float)z / (sizeZ - 1));
                }

                currentVertexIndex++;
            }
        }
        vertices = newVertices;

        if (setUVs) {
            uvs = newUVs;
        }
    }


    private int GetVertLODRowSize(int LOD)
    {
        if (LOD == 0)return sizeX;

        int LODIncrement = (int)Mathf.Pow(2, LOD);
        int vertRowSize = sizeX / LODIncrement + 1;
        return vertRowSize;
    }


    /// <summary>
    /// Generates the triangles array for this rectangle. Must be ran after CalculateVerticesArray().
    /// </summary>
    public void CalculateTrianglesArray(int LOD)
{
    int stepSize = (int)Mathf.Pow(2, LOD); // Step size for the current level of detail
    int vertRowSize = (sizeX - 1) / stepSize + 1; // Number of vertices per row for the current LOD

    int triangleIndex = 0;
    for (int x = 0; x < vertRowSize - 1; x++)
    {
        for (int z = 0; z < vertRowSize - 1; z++)
        {
            int topLeft = x * vertRowSize + z;
            int topRight = topLeft + 1;
            int bottomLeft = topLeft + vertRowSize;
            int bottomRight = bottomLeft + 1;

            // First triangle (topLeft, bottomLeft, topRight)
            triangles[LOD][triangleIndex + 2] = topLeft;
            triangles[LOD][triangleIndex + 1] = bottomLeft;
            triangles[LOD][triangleIndex] = topRight;

            // Second triangle (topRight, bottomLeft, bottomRight)
            triangles[LOD][triangleIndex + 5] = topRight;
            triangles[LOD][triangleIndex + 4] = bottomLeft;
            triangles[LOD][triangleIndex + 3] = bottomRight;

            triangleIndex += 6;
        }
    }
}


    public void GenerateLOD(int LOD) {
        CalculateTrianglesArray(LOD);
        if (generatedLODs[LOD] == false) CreateMesh(LOD);
        generatedLODs[LOD] = true;
    }

    public void CreateMesh(int LOD) {
    meshes[LOD] = new Mesh();

    int vertRowSize = GetVertLODRowSize(LOD);
    int vertCount = vertRowSize * vertRowSize;
    Vector3[] nv = new Vector3[vertCount];
    Vector2[] nuv = new Vector2[vertCount];

    int LODIncrement = (int)Mathf.Pow(2, LOD);

    for (int i = 0; i < vertRowSize; i++) {
        for (int j = 0; j < vertRowSize; j++) {
            int originalIndex = (i * LODIncrement) * sizeZ + (j * LODIncrement);
            nv[i * vertRowSize + j] = vertices[originalIndex];
            nuv[i * vertRowSize + j] = uvs[originalIndex];
        }
    }


    meshes[LOD].vertices = nv;
    meshes[LOD].uv = nuv;
    meshes[LOD].triangles = triangles[LOD];

    meshes[LOD].RecalculateNormals();
    meshes[LOD].RecalculateBounds();
}

    public void SetMeshHeights(float[,] heights, int LOD, bool createMesh) {

        int inc = (int)Mathf.Pow(2, LOD);
        int heightSize = heights.GetLength(0);

        for (int i = 0; i < heightSize; i++) {
            for (int j = 0; j < heightSize; j++) {
                int vertexIndex = (i * inc) * sizeZ + (j * inc);
                if (vertexIndex < vertices.Length) {
                    vertices[vertexIndex].y = heights[i, j];
                }
            }
        }

        if (createMesh) {
            CreateMesh(LOD);
        } else {
            UpdateMesh(LOD);
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

    public void UpdateMesh(int LOD) {
        meshes[LOD].Clear();
        CreateMesh(LOD);
    }

    public Vector3[] GetVertices() {
        return vertices;
    }

}