using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class MultiplayerTestMovement : NetworkBehaviour
{
    private bool dir;
    private float val;
    //these are both in local space, will be converted in start
    [SerializeField] private Vector3 pos1;
    [SerializeField] private Vector3 pos2;
    // Start is called before the first frame update
    void Start()
    {
        //convert into world space
        pos1 = transform.TransformPoint(pos1);
        pos2 = transform.TransformPoint(pos2);
    }

    // Update is called once per frame
    void Update()
    {

        if (!IsServer)
            return;

        if (dir)
            val += Time.deltaTime;
        else
            val -= Time.deltaTime;

        if (val <= 0)
            dir = true;
        else if (val >= 1)
            dir = false;

        val = Mathf.Clamp01(val);

        transform.position = Vector3.Lerp(pos1, pos2, val);
    }
}
