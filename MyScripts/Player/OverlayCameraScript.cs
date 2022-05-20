using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverlayCameraScript : MonoBehaviour
{
    public static OverlayCameraScript instance;
    public float maxFov = 70f;
    private Camera pcam;
    private Camera thiscam;
    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        thiscam = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if(pcam==null)
            pcam = CameraMove.instance.GetComponent<Camera>();

        if (pcam.fieldOfView < maxFov)
            thiscam.fieldOfView = pcam.fieldOfView;
        else
            thiscam.fieldOfView = maxFov;
    }
}
