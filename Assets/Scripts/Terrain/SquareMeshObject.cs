using System.Collections;
using System.Collections.Generic;
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
    public void CalculateVerticesArray(float quadSize, bool setMeshVertices = false) {

        Vector3[] newVertices = new Vector3[sizeX * sizeZ]; // set the size of the array
        int currentVertexIndex = 0;
        for (int x = 0; x < sizeX; x++) {
            for (int z = 0; z < sizeZ; z++) {
                float height = 0; // the height of the vertex
                newVertices[x * sizeZ + z] = new Vector3(
                    centerPosition.x + x * quadSize - (sizeX - 1) * quadSize / 2,
                    height,
                    centerPosition.y + z * quadSize - (sizeZ - 1) * quadSize / 2
                );
                currentVertexIndex++;
            }
        }
        if (setMeshVertices) {
            mesh.vertices = newVertices;
        } else {
            vertices = newVertices;
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

public class SquareMeshObject : MonoBehaviour {

    public VisibleSquareMesh squareMesh;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    public void Initialize(Material material = null) {
        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        if (material != null) { meshRenderer.material = material; }
        meshFilter.mesh = squareMesh.mesh;
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