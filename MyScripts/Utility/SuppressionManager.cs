using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class SuppressionManager : MonoBehaviour
{
    public static SuppressionManager instance;

    public float surpressionDecay = 1f;

    public Volume surpressionEffect; 

    private float surpression;
    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        surpression = Mathf.Clamp(surpression, 0f, 100f); //clamp just before applying to eliminate the chance of passing too high/low value

        surpressionEffect.weight = surpression / 100f;

        surpression -= surpressionDecay * Time.deltaTime;

    }

    public void AddSurpression(float f=10)
    {
        surpression += f;
    }
}
