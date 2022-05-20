using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillhouseTarget : MonoBehaviour
{
    public int requiredHits = 2;
    private int numHits = 0;
    [HideInInspector]
    public bool hit;
    public Vector3 rotAxis = Vector3.right;
    private Vector3 startRot;
    private Vector3 hitRot;
    // Start is called before the first frame update
    void Start()
    {
        startRot = transform.localEulerAngles;
        hitRot = startRot + (rotAxis * 90f);
    }

    // Update is called once per frame
    void Update()
    {
        if (hit && transform.localEulerAngles != hitRot)
            transform.localEulerAngles = hitRot;
        //transform.localEulerAngles = Vector3.MoveTowards(transform.localEulerAngles, hitRot, 360f * Time.deltaTime);
        else if (transform.localEulerAngles != startRot)
            transform.localEulerAngles = startRot;
        //transform.localEulerAngles = Vector3.MoveTowards(transform.localEulerAngles, startRot, 360f * Time.deltaTime);
    }

    public void HitByBullet()
    {
        numHits++;
        if(numHits>=requiredHits)
            hit = true;
        
    }

    public void ResetTarget()
    {
        numHits = 0;
        hit = false;
    }
}
