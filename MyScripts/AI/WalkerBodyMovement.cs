using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkerBodyMovement : MonoBehaviour
{


    public float shakeSpeed = 1f;
    public float shakeIntensity = 1f;
    public Vector3 shakeAxis;
    private Vector3 startPos;

    // Start is called before the first frame update
    void Start()
    {
        startPos = transform.localPosition;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.localPosition = startPos + (shakeAxis * Mathf.Sin(Time.time * shakeSpeed) * shakeIntensity);
    }
}
