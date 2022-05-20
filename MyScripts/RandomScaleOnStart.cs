using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomScaleOnStart : MonoBehaviour
{
    public Vector3 min;
    public Vector3 max;
    public bool uniform;
    public Vector2 uniformMinMax;
    public bool scaleLocalPos;
    // Start is called before the first frame update
    void Start()
    {
        if (uniform)
        {
            float randScale = Random.Range(uniformMinMax.x, uniformMinMax.y);
            transform.localScale = new Vector3(randScale, randScale, randScale);
            if (scaleLocalPos)
                transform.localPosition *= randScale;
        }
        else
        {
            Vector3 newScale = new Vector3(Random.Range(min.x, max.x), Random.Range(min.y, max.y), Random.Range(min.z, max.z));
            transform.localScale = newScale;
            //if (scaleLocalPos)
            //    transform.localPosition *= newScale;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
