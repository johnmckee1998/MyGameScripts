using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class AlignToWind : MonoBehaviour
{
    [Tooltip("If true, then only aligns on start and does not react to changes to wind after starting")]
    public bool onlyOnStart;

    public bool updateEffectWind;
    public VisualEffect vfx;
    public string vfxWind = "Windforce";

    private bool aligned;

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!onlyOnStart || !aligned)
        {
            transform.LookAt(transform.position + GlobalStats.instance.windforce.normalized);

            if (updateEffectWind && vfx!=null)
            {
                try
                {
                    vfx.SetFloat(vfxWind, GlobalStats.instance.windforce.magnitude);
                }
                catch
                {
                    Debug.LogWarning("Wrong WindForce name OR no Windforce param avaliable");
                }
            }

            aligned = true;
        }
        else
            enabled = false;
    }
}
