using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MortarTESTINGScript : MonoBehaviour
{

    public Transform target;
    public bool onlyOnStart;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        AiMortarScript.mortarTarget = target.position;

        if (onlyOnStart)
            Destroy(gameObject); //remove after it has been run once
    }
}
