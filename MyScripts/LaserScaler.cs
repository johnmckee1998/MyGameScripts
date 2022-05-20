using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserScaler : MonoBehaviour
{
    public float range = 50f;

    public LayerMask rayCastIgnore;

    private LineRenderer lineRen;
    // Start is called before the first frame update
    void Start()
    {
        lineRen = GetComponent<LineRenderer>();
        try
        {
            range = GetComponentInParent<TurretAI>().ShootDist;
        }
        catch
        {
            //range = range;
        }
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit rayHit;
        if (Physics.Raycast(transform.position, transform.forward, out rayHit, range, ~rayCastIgnore))
        {
            lineRen.SetPosition(1, new Vector3(0,0, rayHit.distance));
        }
        else
        {
            lineRen.SetPosition(1, new Vector3(0, 0, range));
        }
    }
}
