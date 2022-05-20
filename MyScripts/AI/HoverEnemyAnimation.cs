using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoverEnemyAnimation : MonoBehaviour
{
    private Guard aiScript;

    public GameObject rocketEffect;
    public Renderer faceRen;

    private float blendAmount;

    private Vector3 prevPos;

    private float rotAmount = 25f;

    bool chasing;
    // Start is called before the first frame update
    void Start()
    {
        aiScript = GetComponentInParent<Guard>();

        prevPos = transform.parent.position;

        faceRen.material = Instantiate(faceRen.material);


        rotAmount = 360f - rotAmount;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (transform.parent != null) //so long as it is alive
        {
            if (aiScript != null)
                chasing = aiScript.IsChasing();
            else
                chasing = true; //if no guard script, assume its dumb ai and just always assume chasing

            if (chasing)
            {
                try
                {
                    if (blendAmount > 0) //blend to red
                        blendAmount -= Time.fixedDeltaTime;
                    faceRen.material.SetFloat("Blend_Amount", blendAmount);

                    
                }
                catch
                {
                    Debug.Log("Incorrect variable Name for material " + gameObject.name);
                }
            }
            else
            {
                try
                {
                    if (blendAmount < 1) //blend to green
                        blendAmount += Time.fixedDeltaTime;
                    faceRen.material.SetFloat("Blend_Amount", blendAmount);
                }
                catch
                {
                    Debug.Log("Incorrect variable Name for material " + gameObject.name);
                }
            }

            if (Vector3.Distance( prevPos, transform.parent.position)>0.1f) //have moved
            {
                if (transform.localEulerAngles.x > rotAmount || transform.localEulerAngles.x < 1f) //not fully rotated
                {
                    Vector3 newRot = transform.localEulerAngles;
                    //newRot.x += Time.fixedDeltaTime * rotAmount;
                    //transform.localEulerAngles = newRot;
                    newRot = Vector3.zero;
                    newRot.x = Time.fixedDeltaTime * 2 * -(360-rotAmount);
                    transform.Rotate(newRot, Space.Self);
                }
            }
            else if (transform.localEulerAngles.x > rotAmount-1f) //havent moved - rotate back to normal
            {
                Vector3 newRot = transform.localEulerAngles;
                //newRot.x -= Time.fixedDeltaTime * rotAmount;
                //transform.localEulerAngles = newRot;
                newRot = Vector3.zero;
                newRot.x = Time.fixedDeltaTime * 2 * (360 - rotAmount);
                transform.Rotate(newRot, Space.Self);
            }

            prevPos = transform.parent.position;

        }
        else if (rocketEffect.activeSelf)
            rocketEffect.SetActive(false);
    }
}
