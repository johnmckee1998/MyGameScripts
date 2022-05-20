using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicMove : MonoBehaviour
{
    public float moveSpeed;
    public Transform destPos;

    private Vector3 startPos;
    private bool too; //whether or not laser in travelling too or from destPos

    // Start is called before the first frame update
    void Start()
    {
        startPos = transform.localPosition;
    }

    private void FixedUpdate()
    {
        if (Vector3.Distance(transform.localPosition, startPos) < 0.1f)
            too = true;
        else if (Vector3.Distance(transform.localPosition, destPos.localPosition) < 0.1f)
            too = false;

        if (too)
        {
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, destPos.localPosition, moveSpeed * Time.fixedDeltaTime);
        }
        else
        {
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, startPos, moveSpeed * Time.fixedDeltaTime);
        }
    }
}
