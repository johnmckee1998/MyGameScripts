using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPath : MonoBehaviour
{
    public Transform origin;
    public Transform[] path;
    public enum AIPathtype {Guard, SmallDrone, LargeDrone } //too be used
    public AIPathtype pathType;

    private Color pathcolour = Color.blue;
    // Start is called before the first frame update
    void Start()
    {
        //AIPathManager.instance.UpdatePaths(this);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.25f);
        Gizmos.color = Color.cyan;
        if (path != null)
        {
            for (int i = 0; i < path.Length; i++)
            {
                if (path[i] != null)
                {
                    Gizmos.color = Color.cyan;
                    if(i==0)
                        Gizmos.DrawSphere(path[i].position, 0.5f);
                    else if (i == 1)
                        Gizmos.DrawWireSphere(path[i].position, 0.5f);
                    else
                        Gizmos.DrawWireSphere(path[i].position, 0.25f);
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(path[i].position, path[i].position + path[i].forward);
                    
                    float ler = ((float)i / (float)path.Length);
                    if(i%2==0)
                        pathcolour = Color.Lerp(Color.blue, Color.red, ler);
                    else
                        pathcolour = Color.Lerp(Color.green, Color.cyan, ler);

                    Gizmos.color = pathcolour;
                    if (i + 1 < path.Length && path[i+1]!=null)
                        Gizmos.DrawLine(path[i].position, path[i + 1].position);
                    
                    else
                        Gizmos.DrawLine(path[i].position, path[0].position);
                }
            }
        }

        if (origin != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(origin.position, 0.5f);
        }
    }

}
