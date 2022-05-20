using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GenericEventTrigger : MonoBehaviour
{
    public LayerMask collisionLayer;
    public UnityEvent enterEvent;
    public UnityEvent exitEvent;
    public UnityEvent stayEvent;


    private void OnTriggerEnter(Collider other)
    {
        if (enterEvent != null)
        {
            if (((1 << other.gameObject.layer) & collisionLayer) != 0) //weird way that you use to compare layer to layermask
                enterEvent.Invoke();
        }
        
    }

    private void OnTriggerExit(Collider other)
    {
        if (exitEvent != null)
        {
            if (((1 << other.gameObject.layer) & collisionLayer) != 0)
                exitEvent.Invoke();
            
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (stayEvent != null)
        {
            if (((1 << other.gameObject.layer) & collisionLayer) != 0)
                stayEvent.Invoke();
            
        }
    }
}
