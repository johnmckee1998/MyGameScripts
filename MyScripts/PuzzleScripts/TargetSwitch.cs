using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TargetSwitch : MonoBehaviour
{
    [HideInInspector]
    public bool currentSignal = false;

    public UnityEvent hitEvent;

    [Space]
    public GameObject set1;
    public GameObject set2;
    [Space]
    public float timeToSwitch = 0.5f;


    private bool switchSet = false;
    private float switchTimer;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void FixedUpdate()
    {
        if (switchSet)
        {
            if (switchTimer >= timeToSwitch)
            {
                //toggle sets
                if (set1 != null)
                    set1.SetActive(!set1.activeSelf);
                if (set2 != null)
                    set2.SetActive(!set2.activeSelf);
                switchSet = false;
                switchTimer = 0;
            }
            else
                switchTimer += Time.fixedDeltaTime;
        }
    }


    private void HitByBullet()
    {
        currentSignal = !currentSignal;

        if (hitEvent != null)
            hitEvent.Invoke();

        switchSet = true;
    }

}
