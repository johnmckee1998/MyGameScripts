using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GenericInteract : MonoBehaviour
{
    
    public UnityEvent interactEvent;
    public bool useMessage = true;
    public string displayMessage = " ";
    private bool lookingAt;
    public float interactDist = 4f;
    public LayerMask rayCastIgnore;
    [Space]
    public bool useInteract2;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
       
        CheckSight();
        if (lookingAt && Time.timeScale > 0 && ((Input.GetButtonDown("Interact")&&!useInteract2) || (Input.GetButtonDown("Interact2") && useInteract2)))
        {
            if (interactEvent != null)
                interactEvent.Invoke();
        }
        if (lookingAt && useMessage)
            CanvasScript.instance.popUp.text = displayMessage;
    }

    private void CheckSight()
    {
        if (CharacterControllerScript.instance != null)
        {
            if (Vector3.Distance(CharacterControllerScript.instance.transform.position, transform.position) < interactDist)
            {
                RaycastHit hit;

                // if raycast hits, it checks if it hit this
                if (Physics.Raycast(CharacterControllerScript.instance.pCam.transform.position, CharacterControllerScript.instance.pCam.transform.forward, out hit, interactDist, ~rayCastIgnore))
                {
                    if (hit.collider.gameObject.Equals(gameObject))
                    {
                        lookingAt = true;
                    }
                    else
                        lookingAt = false;
                }
                else
                    lookingAt = false;

            }
            else lookingAt = false;
        }
    }

    public void DestroyObject(GameObject g)
    {
        Destroy(g);
    }
}
