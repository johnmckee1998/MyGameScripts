using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LaserTarget : MonoBehaviour
{
    public Material poweredMaterial;

    private Material offMaterial;
    private Renderer ren;

    [HideInInspector]
    public bool powered;
    private bool prevPowered;

    [Header("Events")]
    public UnityEvent OnPowered; 
    public UnityEvent WhilePowered; 
    // Start is called before the first frame update
    void Start()
    {
        ren = GetComponent<Renderer>();
        offMaterial = ren.material;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (powered)
        {
            ren.material = poweredMaterial;
            if (WhilePowered != null)
                WhilePowered.Invoke();
        }
        else
            ren.material = offMaterial;


        if (powered && !prevPowered)
            if (OnPowered != null)
                OnPowered.Invoke();




        prevPowered = powered;
        powered = false;
    }

    public void TestDebugLog(string s)
    {
        Debug.Log(s);
    }
}
