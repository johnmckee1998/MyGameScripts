using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothMoveToPoint : MonoBehaviour
{
    public Transform point;
    public float speed = 10f;
    public float smoothTime = 1f;
    public Vector3 axis = Vector3.one;
    public Vector3 offset;
    private Vector3 prevPos;
    private Vector3 moveRef;
    // Start is called before the first frame update
    void Start()
    {
        prevPos = transform.position;

    }

    // Update is called once per frame
    void Update()
    {
        Vector3 newpos = Vector3.SmoothDamp(prevPos, point.position+offset, ref moveRef, 1f, speed);
        if (axis.x == 0)
            newpos.x = point.position.x + offset.x;
        if (axis.y == 0)
            newpos.y = point.position.y + offset.y;
        if (axis.z == 0)
            newpos.z = point.position.z + offset.z;
        transform.position = newpos;
        prevPos = transform.position;
    }
}
