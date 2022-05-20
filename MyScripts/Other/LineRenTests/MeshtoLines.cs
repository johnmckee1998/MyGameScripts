using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshtoLines : MonoBehaviour
{
    public Mesh mesh;
    //public MeshRenderer meshRen;
    public MeshFilter meshFilter;
    private Mesh newMesh;
    public MeshTopology meshType;

    public void SwitchToLines()
    {
        newMesh = new Mesh();
        newMesh.vertices = mesh.vertices;

        int[] indis = new int[mesh.vertexCount];

        for (int i = 0; i < indis.Length; i++)
            indis[i] = i;

        newMesh.SetIndices(indis, meshType, 0);
        newMesh.RecalculateBounds();
        meshFilter.mesh = newMesh;
    }

    public void ResetToNormal()
    {

    }
}
