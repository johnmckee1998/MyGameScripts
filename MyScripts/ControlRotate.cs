using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlRotate : MonoBehaviour
{
    public bool limitRotation;
    public Vector2 xMinMax;
    public Vector2 yMinMax;
    public Vector2 zMinMax;

    public bool rotateX;
    public bool rotateY;
    public bool rotateZ;

    public float rotateSpeed = 10f;

    private bool active;

    private float pitch;
    private float yaw;
    private float roll;
    // Start is called before the first frame update
    void Start()
    {
        pitch = transform.localEulerAngles.x;
        yaw = transform.localEulerAngles.y;
        roll = transform.localEulerAngles.z;
    }

    // Update is called once per frame
    void Update()
    {
        if (active)
        {
            if (rotateY)
            {
                if (Time.timeScale > 0)
                {
                    yaw += Input.GetAxis("Horizontal") * rotateSpeed;
                }
            }

            if (limitRotation)
            {
                //Convert to negative if needed
                if (pitch > 180)
                    pitch -= 360f;
                if (yaw > 180)
                    yaw -= 360f;
                pitch = Mathf.Clamp(pitch, xMinMax.x, xMinMax.y);
                yaw = Mathf.Clamp(yaw, yMinMax.x, yMinMax.y);
            }

            transform.localEulerAngles = new Vector3(pitch, yaw, roll);
        }
    }

    public void Activate(bool b)
    {
        active = b;
    }
}
