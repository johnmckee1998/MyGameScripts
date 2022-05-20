using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DissolveParent : MonoBehaviour
{
    public Material dissolveMat;
    private Material appliedDissolveMat;
    [Tooltip("How many seconds it takes to dissolve")]
    public float dissolveRate = 1f;
    private float dissolveAmount;
    private MeshRenderer meshRen;
    // Start is called before the first frame update
    void Start()
    {
        appliedDissolveMat = new Material(dissolveMat);

        meshRen = transform.GetComponentInParent<MeshRenderer>();
        if (meshRen == null) //no mesh found, try child
            meshRen = transform.parent.GetComponentInChildren<MeshRenderer>();
        if(meshRen!=null)
            meshRen.material = appliedDissolveMat;
        else //still no mesh found
        {
            Debug.Log("No Mesh to dissolve");
            Destroy(transform.parent.gameObject);
        }

        dissolveRate = Mathf.Clamp(dissolveRate, 0.000001f, float.MaxValue);
    }

    // Update is called once per frame
    void Update()
    {
        dissolveAmount = Mathf.Clamp01(dissolveAmount);

        appliedDissolveMat.SetFloat("dissolve", dissolveAmount);

        if (dissolveAmount >= 1)
            Destroy(transform.parent.gameObject);
        else
            dissolveAmount += (1f / dissolveRate) * Time.deltaTime;

    }
}
