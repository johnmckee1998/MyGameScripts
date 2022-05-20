using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DissolvingAnomaly : MonoBehaviour
{
    public float maxScale = 15f;
    public float growRate = 1f;
    public bool repeat;
    // Start is called before the first frame update
    void Start()
    {
        transform.localScale = Vector3.one * 0.01f;
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.localScale.x < maxScale)
            transform.localScale += Vector3.one * growRate * Time.deltaTime;
        else if (repeat)
            transform.localScale = Vector3.one * 0.01f;
        else
            Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        HitboxScript hb = other.GetComponent<HitboxScript>();
        if (hb != null)
            hb.Dissolve();
        else
        {
            UniversalStats uS = other.GetComponent<UniversalStats>();
            if (uS != null)
                uS.Dissolve();
            else //not a good idea -> maybe make a script or tag to denote object that can dissolve and objecs that are dissolving
            {
                if (other.CompareTag("CanDissolve"))
                {
                    MeshRenderer mr = other.GetComponent<MeshRenderer>();
                    if (mr != null)
                        DeathEffectsManager.instance.Dissovle(mr.transform);
                    other.tag = "Dissolving";
                }
            }
            
        }

    }

}
