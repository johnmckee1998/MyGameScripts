using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.AI;

public class TDBallEnemyAnim : MonoBehaviour
{
    //public bool useNavSpeed;
    //private NavMeshAgent navAgent;
    //public float speed;
    public float radius=1f;
    public Vector3 axis = Vector3.right;
    private float rotationsPerSecond;
    private Vector3 rotVec;
    private Vector3 realRot;

    private Vector3 prevPos;
    private float realSpeed;
    // Start is called before the first frame update
    void Start()
    {
        //if (useNavSpeed)
        //{
        //    navAgent = GetComponentInParent<NavMeshAgent>();
        //    speed = navAgent.speed;
        //}

        //rotationsPerSecond = speed / (2f * radius * Mathf.PI);
        //rotVec = axis * rotationsPerSecond * 360f;
        realRot = Vector3.zero;

        prevPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        //realSpeed = Vector3.Distance(prevPos, transform.position) / Time.deltaTime;
        


        realRot += (rotVec*Time.deltaTime);
        //if(realSpeed!=0 && Time.timeScale>0)
            transform.localEulerAngles = realRot;

        //prevPos = transform.position;
    }

    private void FixedUpdate()
    {
            realSpeed = Vector3.Distance(prevPos, transform.position) / Time.fixedDeltaTime;
            prevPos = transform.position;
            rotationsPerSecond = realSpeed / (2f * radius * Mathf.PI);
            rotVec = axis * rotationsPerSecond * 360f;
    }
}
