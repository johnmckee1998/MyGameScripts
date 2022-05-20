using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineRenDrawMesh : MonoBehaviour
{
    private LineRenderer lineRen;
    public Mesh refMesh;
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
          
    }

    public void GenerateLine()
    {
        float avLength = 0;
        lineRen = GetComponent<LineRenderer>();
        lineRen.positionCount = refMesh.vertexCount;
        Vector3[] verticies = refMesh.vertices;
        
        for (int i = 0; i < verticies.Length; i++)
        {
            lineRen.SetPosition(i, verticies[i]);
            if (i > 0)
                avLength += Vector3.Distance(lineRen.GetPosition(i) , lineRen.GetPosition(i - 1));
        }
        avLength = avLength / (verticies.Length-1);
        Debug.Log("Average length: " + avLength);
    }

    public void ClearLine()
    {
        lineRen.positionCount = 2;
        lineRen.SetPosition(0, Vector3.zero);
        lineRen.SetPosition(1, Vector3.forward);
    }
}
