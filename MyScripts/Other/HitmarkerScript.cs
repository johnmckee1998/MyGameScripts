using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HitmarkerScript : MonoBehaviour
{
    public static HitmarkerScript instance;

    
    public Image hitMarker;
    public AudioSource hitSound;


    private bool waitForUpdate;
    private float hitTimer;

    private Vector3 hitStartScale;

    private float colourLerp;
    // Start is called before the first frame update
    void Start()
    {
        instance = this;

        hitMarker.enabled = false;

        hitStartScale = hitMarker.transform.localScale;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (waitForUpdate)  
            waitForUpdate = false;

        if (hitTimer > 0) 
            hitTimer -= Time.fixedDeltaTime;
        else
            hitMarker.enabled = false;

        if (hitMarker.color != Color.white)
        {
            hitMarker.color = Color.Lerp(Color.red, Color.white, colourLerp);
            colourLerp += Time.fixedDeltaTime * 4f;
            if (colourLerp > 1f)
                colourLerp = 1f;
        }

        if (hitMarker.transform.localScale.x > hitStartScale.x)
            hitMarker.transform.localScale = Vector3.MoveTowards(hitMarker.transform.localScale, hitStartScale, 12f * Time.deltaTime);
    }

    public void Hit()
    {
        if(!waitForUpdate && Time.timeScale > 0)
        {
            waitForUpdate = true;
            hitMarker.enabled = true;
            hitSound.Play();
            hitTimer = 0.25f;
            hitMarker.transform.localScale = 2f * hitStartScale;
        }
    }

    public void KillHit()
    {
        //Debug.Log("Killhit");

        hitMarker.enabled = true;
        hitSound.Play();
        hitTimer = 0.25f;
        hitMarker.transform.localScale = 2f * hitStartScale;
        hitMarker.color = Color.red;
        colourLerp = 0;
    }

}
