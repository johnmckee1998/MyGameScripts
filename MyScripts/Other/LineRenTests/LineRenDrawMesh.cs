using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineRenDrawMesh : MonoBehaviour
{
    
    public Mesh refMesh;
    public float minDist = -1f;  //since i changed to using tris, dont really need dist check anymore
    public float maxDist = 1f;
    public bool showGiz;
    public int vertIndex;

    public GameObject lineRenObj;

    public bool optimiseEdges;
    public bool mergeLines;
    public bool debugTest;

    private LineRenderer lineRen;

    [System.Serializable]
    private struct EdgePoints
    {
        public Vector3 point1;
        public Vector3 point2;

        public EdgePoints(Vector3 p1, Vector3 p2)
        {
            point1 = p1;
            point2 = p2;
        }



        public bool AllEquals(EdgePoints b)
        {
            if (point1 == b.point1 && point2 == b.point2)
                return true;

            if (point1 == b.point2 && point2 == b.point1)
                return true;

            return false;
        }
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
          
    }


    

    public void NewNewGenerateLine() //bascially just a path between all verts, not all edges
    {
        List<Vector3> points = new List<Vector3>();
        Vector3[] verticies = refMesh.vertices;
        for (int i = 0; i < verticies.Length; i++)
        {
            if (points.Count <= 0 || !points.Contains(verticies[i]))
                points.Add(verticies[i]);
        }

        lineRen = GetComponent<LineRenderer>();
        lineRen.positionCount = points.Count;

        for (int i = 0; i < points.Count; i++)
            lineRen.SetPosition(i, points[i]);

    }



    public void NewGenerateLine()
    { //nope - doesnt work
        lineRen = GetComponent<LineRenderer>();
        lineRen.positionCount = refMesh.vertexCount;
        Vector3[] verticies = refMesh.vertices;
        Debug.Log("Ren: " + lineRen.positionCount + " Vert: " + verticies.Length);
        Vector3 prevVert = Vector3.zero;

        Vector3 currentvert;
        int count=0;
        for (int i = 0; i < verticies.Length; i++)
        {
            
            if (i == 0)
            {
                currentvert = verticies[i];
                lineRen.SetPosition(count, currentvert);
                count++;
            }
            else
                currentvert = prevVert;

            Debug.Log("YA");

            float minVDist= float.MaxValue;
            int minDistIndex =-1;
            for(int j=0; j<verticies.Length; j++)
            {
                if (true)
                {
                    float dist = Vector3.Distance(currentvert, verticies[j]);
                    if (dist < minVDist && dist>=minDist && dist<=maxDist)
                    {
                        minDist = dist;
                        minDistIndex = j;
                        Debug.Log("NewPoint");
                    }
                }
                Debug.Log("YE");
            }
            if (minDistIndex >= 0 && count<lineRen.positionCount)
            {
                currentvert = verticies[minDistIndex];
                lineRen.SetPosition(count, currentvert);
                count++;
                Debug.Log("Added");
            }

            prevVert = currentvert;
        }
    }


    public void GenerateLine()
    {
        //NewNewGenerateLine();
        //return;

        float avLength = 0;
        lineRen = GetComponent<LineRenderer>();
        //lineRen.positionCount = refMesh.vertexCount;
        Vector3[] verticies = refMesh.vertices;
        int count=0;

        List<EdgePoints> edges = new List<EdgePoints>();
        /*
        for (int i = 1; i < verticies.Length; i++)
        {
            if (minDist<0 || i==0 || Vector3.Distance(verticies[i - 1], verticies[i]) <= maxDist)
            {
                edges.Add(new EdgePoints(verticies[i - 1], verticies[i]));

                
                //lineRen.SetPosition(count, verticies[i]);
                //
                //if (i > 0)
                //    avLength += Vector3.Distance(lineRen.GetPosition(count), lineRen.GetPosition(count - 1));
                //count++;
                
            }
        }*/


        //USE THE TRIANGLES
        for (int i = 0; i < refMesh.triangles.Length-2; i+=3)
        {
            Vector3 p1 = refMesh.vertices[refMesh.triangles[i]];
            Vector3 p2 = refMesh.vertices[refMesh.triangles[i + 1]];
            Vector3 p3 = refMesh.vertices[refMesh.triangles[i + 2]];
            if(Vector3.Distance(p1,p2)<=maxDist) //only add if within max dist 
                edges.Add(new EdgePoints(p1, p2)); //a-b

            if (Vector3.Distance(p2, p3) <= maxDist) //only add if within max dist
                edges.Add(new EdgePoints(p2, p3)); //b-c

            if (Vector3.Distance(p3, p1) <= maxDist) //only add if within max dist
                edges.Add(new EdgePoints(p3, p1)); //c-a
        }


        Debug.Log("Old Edge Count: " + edges.Count);

        //remove duplicate edges
        if (optimiseEdges)
        {
            for (int j = 0; j < edges.Count; j++)
            {
                for (int k = 0; k < edges.Count; k++)
                {
                    if (j != k && edges[j].AllEquals(edges[k]))
                    {
                        edges.RemoveAt(k);
                        k--; //go back on so that when it ++ then it will be back at the same spot which will now be a new edge
                    }
                }
            }
        }
        Debug.Log("New Edge Count: " + edges.Count);




        if (debugTest)
        {
            for (int j = 0; j < edges.Count; j++)
            {
                GameObject g = Instantiate(lineRenObj, lineRen.transform.position, lineRen.transform.rotation, transform);
                LineRenderer l = g.GetComponent<LineRenderer>();
                l.SetPosition(0, edges[j].point1);
                l.SetPosition(1, edges[j].point2);
                g.name += " :" + j;
            }
            if (mergeLines)
            {
                int removedItems = MergeLines();
                Debug.Log("Removed: " + removedItems);
            }
            return;
        }
       
        //old style - all points on one line
        lineRen.positionCount = edges.Count * 2;
        count = 0;
        for(int j=0; j<edges.Count && count < lineRen.positionCount; j++)
        {
            lineRen.SetPosition(count, edges[j].point1);
            count++;
            lineRen.SetPosition(count, edges[j].point2);
        }

        //this is all bullshit i realised - even if i skip long edges or something, the line renderer cant have gaps so it will connect distant edges anyway - if a draw two short lines but they are not next to each other, it will have a line connecting them anyway

        //lineRen.positionCount = count;
        //avLength = avLength / (count-1);
        //Debug.Log("Average length: " + avLength);
        Debug.Log("All good!"); 
    }

    public void ClearLine()
    {
        lineRen = GetComponent<LineRenderer>();
        lineRen.positionCount = 2;
        lineRen.SetPosition(0, Vector3.zero);
        lineRen.SetPosition(1, Vector3.forward);

        for(int i=transform.childCount-1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

    }

    public int MergeLines()
    {
        lineRen = GetComponent<LineRenderer>();
        if (lineRen.transform.childCount == 0)
            return 0; //no children to merge

        LineRenderer[] lines = new LineRenderer[lineRen.transform.childCount];
        for (int i = 0; i < lineRen.transform.childCount; i++)
            lines[i] = lineRen.transform.GetChild(i).GetComponent<LineRenderer>();
        //array is now full of line renderers

        //make a list to hold all the points
        //List<Vector3> points = new List<Vector3>();
        List<int> linesToGo = new List<int>();

        Dictionary<int, int> linesConnections = new Dictionary<int, int>();
        //sort them
        for(int i=0; i<lines.Length; i++)
        {
            if (!linesToGo.Contains(i))
            {
                for (int j = i + 1; j < lines.Length - 1; j++)
                {
                    if (!linesToGo.Contains(j) && lines[i].GetPosition(1) == lines[j].GetPosition(0))
                    {
                        /* //attempt to merge extra lines onto already merged lines
                        if (linesToGo.Contains(i))
                        {
                            int index=-1;
                            linesConnections.TryGetValue(i, out index);
                            if (index >= 0)
                            {
                                lines[index].positionCount++;
                                lines[index].SetPosition(lines[index].positionCount-1, lines[j].GetPosition(1));
                                linesToGo.Add(j);
                            }
                            

                            break;
                        }*/


                        //found a match
                        lines[i].positionCount ++;
                        lines[i].SetPosition(2, lines[j].GetPosition(1));
                        linesToGo.Add(j);
                        linesConnections.Add(j, i);
                        break;
                    }
                }
            }
        }

        for (int i = 0; i < linesToGo.Count; i++)
        {
            DestroyImmediate(lines[linesToGo[i]].gameObject);
        }

        return linesToGo.Count;
    }


    private void OnDrawGizmosSelected()
    {
        if (showGiz && refMesh!=null)
        {
            Vector3[] verticies = refMesh.vertices;

            vertIndex = Mathf.Clamp(vertIndex, 0, verticies.Length);

            Gizmos.color = Color.green;
            Gizmos.DrawSphere(verticies[vertIndex], 0.1f);
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(transform.TransformPoint(verticies[vertIndex]), 0.125f);

        }
    }
}
