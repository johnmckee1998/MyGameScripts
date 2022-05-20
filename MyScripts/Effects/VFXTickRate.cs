using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class VFXTickRate : MonoBehaviour
{
    public float tickRateModifier = 1.0f;
    private VisualEffect vfxGraph;
    // Start is called before the first frame update
    void Start()
    {
        vfxGraph = GetComponent<VisualEffect>();
        vfxGraph.playRate = tickRateModifier;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
