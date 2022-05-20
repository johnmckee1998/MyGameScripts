using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateToVelocity : MonoBehaviour
{
    private Vector3 prevPos;
    // Start is called before the first frame update
    void Start()
    {
        prevPos = transform.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 travelDir = transform.position-prevPos;
        travelDir.Normalize();
        transform.rotation = Quaternion.LookRotation(travelDir);

        prevPos = transform.position;
    }
}
