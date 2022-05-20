using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurnParent : MonoBehaviour
{
    public Material burnMat;
    private Material appliedBurnMat;
    [Tooltip("How Many Seconds to burn")]
    public float burnRate = 1f;
    private float burnAmount;
    public bool destroyOnBurn;
    public bool dissolveAfterBurning;
    public GameObject dissolveObj;
    private bool dissolving;
    void Start()
    {
        appliedBurnMat = new Material(burnMat);

        transform.GetComponentInParent<MeshRenderer>().material = appliedBurnMat;

        burnRate = Mathf.Clamp(burnRate, 0.000001f, float.MaxValue);
    }

    
    void Update()
    {
        burnAmount = Mathf.Clamp01(burnAmount);

        appliedBurnMat.SetFloat("Dissolve", burnAmount);

        if (burnAmount >= 1)
        {
            if (destroyOnBurn)
                Destroy(transform.parent.gameObject);
            else if (dissolveAfterBurning && !dissolving)
            {
                DeathEffectsManager.instance.Dissovle(transform.parent);
                dissolving = true;
            }
            //else
            //    this.enabled = false;//remove effect;
        }
        else
            burnAmount += (1f / burnRate) * Time.deltaTime;

    }
}
