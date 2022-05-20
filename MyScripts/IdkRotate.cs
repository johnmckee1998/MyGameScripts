using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdkRotate : MonoBehaviour
{
    public float Rspeed = 3.0f;
    public GameObject focus;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Rspeed * Time.deltaTime, 0f, 0f);
        transform.LookAt(focus.transform.position);
    }
}
